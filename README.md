# Dynamic Web Service Tester
![Static Badge](https://img.shields.io/badge/C%23-7.3-%23178600)
![Static Badge](https://img.shields.io/badge/.NET%20Framework-4.8-%239780E5)

WCF (Windows Communication Foundation) Web Service that provides operations for testing WSDL and RESTful web services.

## API Reference
### Test Operation with Input
```C#
ServiceResults TestOperationGivenInput(string url, string opName, object[] input)
```
Tests an operation of a service passing the argument input as the input to the operation call. 

#### Parameters
- `string url`: The URL to the WSDL page of the service to test.
- `string opName`: The name of the operation to test.
- `object[] input`: The argument values for the operation being tested.

### Test Services
```C#
Dictionary<string, ServiceResults> TestServices(string[] urls)
```
Tests a number of services to verify whether they can be accessed and generates input that will be of the correct type for value-type parameters, but may not be contextually appropriate for those parameters.  Thus, this operation will potentially get successful output for operations offered by a service, but is not garaunteed to do so.  

#### Parameters
- `string[] url`: An array containing the urls of the WSDL pages for the services to test.

### Test RESTful Service
```C#
string TestRESTfulService(string baseUrl, string opName, string[] paramName, string[] input)
```
Invokes a given operation from the RESTful service at the given parameter names and corresponding input values returns the operation's output.

#### Parameters
- `baseUrl`: The base url of the RESTful service to test.
- `opName`: The name of the operation of the RESTful service to test.
- `paramName`: An array containing the parameter names for the operation to test.
- `input`: An array containing the argument values for the operation to test.
