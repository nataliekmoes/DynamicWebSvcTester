# DynamicWebSvcTester_SVC

WCF (Windows Communication Foundation) Web Service that provides operations for testing WSDL and RESTful web services.


## API Reference

```C#
ServiceResults TestOperationGivenInput(string url, string opName, object[] input)
```

Tests an operation of a service passing the argument input as the input to the operation call. 

#### Parameters
- `string url` | &nbsp;&nbsp;The URL to the WSDL page of the service to test.
- `string opName` | &nbsp;&nbsp;The name of the operation to test.
- `object[] input` | &nbsp;&nbsp;The argument values for the operation being tested.

<br/>

```C#
Dictionary<string, ServiceResults> TestServices(string[] urls)
```

Tests a number of services to verify whether they can be accessed and generates input that will be of the correct type for value-type parameters, but may not be contextually appropriate for those parameters.  Thus, this operation will potentially get successful output for operations offered by a service, but is not garaunteed to do so.  

#### Parameters
- `string[] url` | &nbsp;&nbsp;An array containing the urls of the WSDL pages for the services to test.

<br/>

```C#
string TestRESTfulService(string baseUrl, string opName, string[] paramName, string[] input)
```

Invokes a given operation from the RESTful service at the given parameter names and corresponding input values returns the operation's output.

#### Parameters
- `baseUrl` | &nbsp;&nbsp;The base url of the RESTful service to test.
- `opName` | &nbsp;&nbsp;The name of the operation of the RESTful service to test.
- `paramName` | &nbsp;&nbsp;An array containing the parameter names for the operation to test.
 - `input` | &nbsp;&nbsp;An array containing the argument values for the operation to test.

<br/>

## Specs
**.NET Framework 4.8**&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;**C# 7.3**

<br/>
