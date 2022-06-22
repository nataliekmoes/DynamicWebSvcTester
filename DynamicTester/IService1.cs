using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace DynamicTesting
{
    [ServiceContract]
    public interface IService1
    {
        /// <summary>
        /// Tests an operation of a service passing the argument input as the
        /// input to the operation call. As this only tests one operation, the other
        /// operations in ServiceResults.OpInfo will have null Input and output values.
        /// This operation will be able to modify one operation's results from the same 
        /// ServiceResults instance, and will test the same operation again if the input 
        /// argument differs from the instance passed to it in a preceding call.  
        /// This operation can also be invoked before invoking TestServices to test the
        /// services' operations that take contextually-sensitive or non-value-type input to
        /// generate valid output for all operations for those services.
        /// </summary>
        /// <param name="url">The URL to the WSDL page of the service to test.</param>
        /// <param name="opName">The name of the operation to test.</param>
        /// <param name="input">The argument values for the operation being tested.</param>
        /// <returns></returns>
        [OperationContract]
        ServiceResults TestOperationGivenInput(string url, string opName, object[] input);


        /// <summary>
        /// Tests a number of services to verify whether they can be accessed and 
        /// generates input that will be of the correct type for value-type parameters,
        /// but may not be contextually appropriate for those parameters.  Thus, this
        /// operation will potentially get successful output for operations
        /// offered by a service, but is not garaunteed to do so.  This operation also
        /// caches services that were successfully bound to and shares this cache with 
        /// TestOperationGivenInput, so it only tests the operations of a service that
        /// have either not been tested or were tested with different input but were 
        /// unsuccessful.
        /// </summary>
        /// <param name="urls">An array containing the urls of the WSDL pages for the services to test.</param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, ServiceResults> TestServices(string[] urls);


        /// <summary>
        /// Invokes a given operation from the RESTful service at the given parameter names and corresponding 
        /// input values returns the operation's output.
        /// </summary>
        /// <param name="baseUrl">The base url of the RESTful service to test.</param>
        /// <param name="opName">The name of the operation of the RESTful service to test.</param>
        /// <param name="paramName">An array containing the parameter names for the operation to test.</param>
        /// <param name="input">An array containing the argument values for the operation to test.</param>
        /// <exception cref="ArgumentException(string)"></exception>
        /// <exception cref="ArgumentNullException(string)"></exception>
        /// <returns></returns>    
        [OperationContract]
        string TestRESTfulService(string baseUrl, string opName, string[] paramName, string[] input);
    }


    /// <summary>
    /// Contains information related to the testing of a WSDL
    /// service and its operations.
    /// </summary>
    [DataContract]
    public class ServiceResults
    {
        private string _url;
        private string _name = "";
        private bool _result = false;
        private bool _fromCache = false;
        private Dictionary<string, OpInfoInput> _opInfo = new Dictionary<string, OpInfoInput>();
        private List<string> _exceptionMessages = new List<string>();
        private static Dictionary<string, ServiceResults> _cache = new Dictionary<string, ServiceResults>();
        
        
        /// <summary>
        /// The default Constructor for the ServiceResults class.
        /// </summary>
        public ServiceResults() { }


        /// <summary>
        /// Initializes an instance of the ServiceResults class with
        /// given URL value.
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException(string)"></exception>
        public ServiceResults(string url)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }


        /// <summary>
        /// Loads _opInfo with OpInfoInput objects that have been initialized from
        /// the given OperationInfoCollection instance
        /// </summary>
        /// <param name="operationInfos"></param>
        public void InitializeFrom(Dictionary<string, WsOperations.OperationInfo> operationInfos)
        {
            foreach (KeyValuePair<string, WsOperations.OperationInfo> info in operationInfos)
            {
                _opInfo.Add(info.Key, new OpInfoInput(info.Value));
            }
        }


        /// <summary>
        /// The URL of this service's WSDL page.
        /// </summary>
        [DataMember]
        public string Url { get => _url; set => _url = value; }

        /// <summary>
        /// Indicates if this service was available at the time it was tested.
        /// </summary>
        [DataMember]
        public bool Result { get => _result; set => _result = value; }

        /// <summary>
        /// The name of the service.
        /// </summary>
        [DataMember]
        public string Name { get => _name; set => _name = value; }

        /// <summary>
        /// A list of the messages of exceptions that occurred during testing.
        /// </summary>
        [DataMember]
        public List<string> ExceptionMessages { get => _exceptionMessages; set => _exceptionMessages = value; }

        /// <summary>
        /// The cache shared by all instances of ServiceResults, storing ServiceResults instances keyed
        /// by URL.
        /// </summary>
        [DataMember]
        public static Dictionary<string, ServiceResults> Cache { get => _cache; set => _cache = value; }

        /// <summary>
        /// A dictionary containing OpInfoInput instances keyed by operation name,
        /// which store information about the service's operations, including the 
        /// input and output values obtained when the operation was tested and 
        /// whether the operation was executed successfully.
        /// </summary>
        [DataMember]
        public Dictionary<string, OpInfoInput> OpInfo { get => _opInfo; set => _opInfo = value; }

        /// <summary>
        /// Indicates whether TestOperationGivenInput or TestServices obtained
        /// this ServiceResults instance while it was in the cache. 
        /// </summary>
        [DataMember]
        public bool FromCache { get => _fromCache; set => _fromCache = value; }
    }


    /// <summary>
    /// Extends WsOperations.OperationInfo to include information relevant 
    /// to testing an operation, such as the input and output values gathered
    /// and whether the operation executed successfully during testing.
    /// </summary>
    [DataContract]
    public class OpInfoInput : WsOperations.OperationInfo
    {
        private List<object> _input = null;
        private object _output;
        private bool _isValid = false;


        /// <summary>
        /// Initializes an OpInfoInput object from an OperationInfo instance
        /// to instantiate members inherited from OperationInfo 
        /// </summary>
        /// <param name="opInfo"></param>
        public OpInfoInput(WsOperations.OperationInfo opInfo)
        {
            Name = opInfo.Name;
            PortTypeName = opInfo.PortTypeName;
            InputParameters = opInfo.InputParameters;
            OutputParameters = opInfo.OutputParameters;
        }
 

        /// <summary>
        /// Determines if another List<object> instance is identical to the
        /// member _input of this OpInfoInput instance.
        /// </summary>
        /// <param name="list2"></param>
        /// <returns></returns>
        public bool InputEquals(List<object> list2)
        {
            if (list2 == null || _input == null) return false;

            var firstNotSecond = _input.Except(list2).ToList();
            var secondNotFirst = list2.Except(_input).ToList();
            return !firstNotSecond.Any() && !secondNotFirst.Any();
        }


        /// <summary>
        /// A list of objects storing the argument values corresponding to
        /// the parameters of the operation.
        /// </summary>
        [DataMember]
        public List<object> Input { get => _input; set => _input = value; }

        /// <summary>
        /// The output returned by the operation, which will be null if the 
        /// operation did not execute or was not invoked successfully.
        /// </summary>
        [DataMember]
        public object Output { get => _output; set => _output = value; }

        /// <summary>
        /// Indicates whether the operation executed and/or was invoked successfully.
        /// </summary>
        [DataMember]
        public bool IsValid { get => _isValid; set => _isValid = value; }     
    }   

}
