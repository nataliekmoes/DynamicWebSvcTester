using System;
using System.Collections.Generic;
using WcfSamples.DynamicProxy;

namespace DynamicTesting
{
    public class Service1 : IService1
    {
        public ServiceResults TestOperationGivenInput(string url, string opName, object[] input)
        {
            ServiceResults serviceResults = null;

            // Determine if this instance of ServiceResults is already in cache
            if (ServiceResults.Cache.TryGetValue(url, out ServiceResults s) && s != null
                && s.OpInfo.TryGetValue(opName, out OpInfoInput op) && op.InputEquals(new List<object>(input))) // is in cache -- url, operation name, and input are all the same 
            {
                serviceResults = s;
                serviceResults.FromCache = true;
            }
            else   // not in the cache
            {   // make proxy, call method, store results in cache, and return them              
                serviceResults = new ServiceResults(url);
                try
                {
                    WsOperations.Service1Client client = new WsOperations.Service1Client();
                    WsOperations.WebServiceInfo info = client.GetWebServiceInfo(url); // get service information

                    if (info.Ops.ContainsKey(opName)) // service has the operation
                    {   // store relevent information from WebServiceInfo                    
                        serviceResults.InitializeFrom(info.Ops);
                        serviceResults.Name = info.ServiceName;
                        serviceResults.OpInfo[opName].Input = new List<object>(input);

                        DynamicProxyFactory factory = new DynamicProxyFactory(serviceResults.Url);
                        serviceResults.Result = true;
                        TestOperation(factory, serviceResults, opName);
                    }
                    else // service does not provide the given operation
                    {
                        serviceResults.ExceptionMessages.Add("Operation could not be found for service.");
                    }
                }
                catch (Exception exc)  // failed to get operation info
                {
                    serviceResults.Result = false;
                    serviceResults.ExceptionMessages.Add(exc.Message);
                }
            }

            if (serviceResults != null && serviceResults.Result && serviceResults.OpInfo[opName].IsValid) // add result to cache if operation call was successful
            {
                if (ServiceResults.Cache.ContainsKey(url))
                {
                    ServiceResults.Cache[url] = serviceResults;
                }
                else
                {
                    ServiceResults.Cache.Add(url, serviceResults);
                }
            }

            return serviceResults;
        }


        public Dictionary<string, ServiceResults> TestServices(string[] urls)
        {
            WsOperations.Service1Client client = new WsOperations.Service1Client();
            Dictionary<string, ServiceResults> dict = new Dictionary<string, ServiceResults>();
            DynamicProxyFactory factory;
            WsOperations.WebServiceInfo info;

            foreach (string wsdl in urls)
            {
                if (ServiceResults.Cache.TryGetValue(wsdl, out ServiceResults s) && s != null)  // if in cache 
                {
                    s.FromCache = true;
                    try
                    {
                        factory = new DynamicProxyFactory(wsdl);

                        foreach (KeyValuePair<string, OpInfoInput> opIn in s.OpInfo) // check/test each operation
                        {
                            //  do not test if it was successfully tested or if it failed w/ generated input 
                            if (!opIn.Value.IsValid)  // was either not tested or tested w/ bad input
                            {
                                List<object> input = GenerateInput(opIn.Value);
                                if (!opIn.Value.InputEquals(input)) // not tested or tested w/ bad non-generated input
                                {
                                    opIn.Value.Input = input;
                                    TestOperation(factory, s, opIn.Key);
                                }
                            }
                        }
                    }
                    catch (Exception exc)  // DynamicProxyFactory creation failed 
                    {
                        s.Result = false;
                        s.ExceptionMessages.Add(exc.Message);
                    }
                    finally
                    {
                        try // add to returned dictionary
                        {
                            dict.Add(wsdl, s);
                        }
                        catch (ArgumentException) { }
                    }
                }

                else   // not in cache
                {
                    ServiceResults serviceResults = new ServiceResults(wsdl);
                    try
                    {
                        factory = new DynamicProxyFactory(wsdl);
                        info = client.GetWebServiceInfo(wsdl);   // instantiate webmethodinfo using WsOperations
                        serviceResults.Result = true;
                        serviceResults.Name = info.ServiceName;
                        serviceResults.InitializeFrom(info.Ops);

                        foreach (KeyValuePair<string, OpInfoInput> opIn in serviceResults.OpInfo) // for each operation
                        {
                            opIn.Value.Input = GenerateInput(opIn.Value);
                            TestOperation(factory, serviceResults, opIn.Key);
                        }
                    }
                    catch (Exception exc)  // proxy instantiation failed
                    {
                        serviceResults.Result = false;
                        serviceResults.ExceptionMessages.Add(exc.Message);
                    }
                    finally
                    {
                        try // add ServiceResults to returned dictionary and cache
                        {
                            dict.Add(wsdl, serviceResults);
                            // add to cache
                            if (serviceResults.Result)
                            {
                                ServiceResults.Cache.Add(wsdl, serviceResults);
                            }
                        }
                        catch (ArgumentException) { }
                    }
                }
            }
            return dict;
        }


        public string TestRESTfulService(string baseUrl, string opName, string[] paramName, string[] input)
        {
            //   Validate argument values 
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException($"'{nameof(baseUrl)}' cannot be null or empty.", nameof(baseUrl));
            }

            if (string.IsNullOrEmpty(opName))
            {
                throw new ArgumentException($"'{nameof(opName)}' cannot be null or empty.", nameof(opName));
            }

            //  construct complete url
            string completeUrl = baseUrl + "/" + opName;

            if (paramName != null && input != null && paramName.Length > 0 && input.Length > 0) // operation has params. & args.
            {
                if (paramName.Length != input.Length)   // check argument validity
                {
                    throw new ArgumentException($"'{nameof(paramName)} and '{nameof(input)}' must have the same length.");
                }

                // format parameters and input to form complete url             
                string[] templateStr = new string[paramName.Length];
                for (int i = 0; i < paramName.Length; i++)
                {
                    templateStr[i] = string.Format("{0}={1}", paramName[i], input[i]);
                }
                completeUrl += "?" + string.Join("&", templateStr);  // add formatted parameters and arguments
            }

            // use Web2String service to download the service's output and return it
            Web2String.ServiceClient client = new Web2String.ServiceClient();
            return client.GetWebContent(completeUrl);
        }



        /// <summary>
        /// Instantiates a DynamicProxy from the DynamicProxyFactory instance factory and
        /// calls the operation given by opName using the input instance for the operation
        /// and stores the output as well as whether the operation successfully executed 
        /// in the operation's corresponding OpInfoInput instance located in serviceResults.OpInfo.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="serviceResults"></param>
        /// <param name="opName"></param>
        private void TestOperation(DynamicProxyFactory factory, ServiceResults serviceResults, string opName)
        {
            try
            {
                DynamicProxy proxy = factory.CreateProxy(serviceResults.OpInfo[opName].PortTypeName);

                // store output from operation invocation
                try
                {
                    object output = proxy.CallMethod(opName, serviceResults.OpInfo[opName].Input.ToArray());
                    if (output != null)
                    {
                        serviceResults.OpInfo[opName].Output = output.ToString();
                    }
                    else
                    {
                        serviceResults.OpInfo[opName].Output = "null";
                    }

                    serviceResults.OpInfo[opName].IsValid = true;  // operation successfully returned
                }
                catch (Exception exc)  // operation invocation likely failed
                {
                    serviceResults.ExceptionMessages.Add(exc.Message);
                }
            }
            catch (Exception exc)  // creation of dynamic proxy likely failed
            {
                serviceResults.Result = false;
                serviceResults.ExceptionMessages.Add(exc.Message);
            }
        }

        /// <summary>
        /// Generates sample argument values for the input parameter values
        /// of the operation represented by opInfo.
        /// NOTE: for non-value types and other types that have not been considered, 
        /// the null value is returned.
        /// </summary>
        /// <param name="opInfo"></param>
        /// <returns></returns>
        private List<object> GenerateInput(OpInfoInput opInfo)
        {
            List<object> input = new List<object>();
            // extract type(s) from input parameter(s) and generate input value(s)
            foreach (WsOperations.Parameter parameter in opInfo.InputParameters)
            {
                switch (parameter.Type)     // generate certain input based on type
                {
                    case "string":
                        input.Add("Test Input");
                        break;
                    case "char":
                        input.Add('a');
                        break;
                    case "int":
                        input.Add(50);
                        break;
                    case "long":
                        input.Add(50);
                        break;
                    case "unsignedlong":
                        input.Add(50);
                        break;
                    case "short":
                        input.Add(50);
                        break;
                    case "unsignedshort":
                        input.Add(50);
                        break;
                    case "float":
                        input.Add(50.5);
                        break;
                    case "double":
                        input.Add(50.5);
                        break;
                    case "decimal":
                        input.Add(50.5);
                        break;
                    case "boolean":
                        input.Add(false);
                        break;
                    default:     // most likely an object 
                        input.Add(null);
                        break;
                }
            }
            return input;
        }

    }
}
