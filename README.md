# 🚀 Dynamic Web Service Tester

<p align="center">
  <img src=https://img.shields.io/badge/C%23-7.3-%23178600>
  <img src=https://img.shields.io/badge/.NET%20Framework-4.8-%239780E5>
</p>

A **WCF (Windows Communication Foundation) Web Service** designed for testing **WSDL** and **RESTful** web services. This service provides operations to verify service accessibility, test method execution, and inspect responses.  

## 📌 API Reference  

### 🔍 **Test Operation with Input**

```csharp
ServiceResults TestOperationGivenInput(string url, string opName, object[] input)
```
Tests an operation of a WSDL-based web service by invoking it with the provided input values.  

#### 📌 Parameters

- **`string url`** – The URL of the **WSDL** page for the service to test.  
- **`string opName`** – The **name of the operation** to test.  
- **`object[] input`** – The **input arguments** for the operation call.  

### 🔄 **Test Multiple Services**

```csharp
Dictionary<string, ServiceResults> TestServices(string[] urls)
```
Tests multiple WSDL-based services to check their accessibility. Generates default input values for **value-type parameters**, though the generated values may not always be contextually valid.  

#### 📌 Parameters

- **`string[] urls`** – An array of **WSDL URLs** for the services to test.  

### 🌐 **Test RESTful Service**

```csharp
string TestRESTfulService(string baseUrl, string opName, string[] paramName, string[] input)
```
Invokes a **RESTful** service operation by passing parameter names and corresponding input values, then returns the operation's response.  

#### 📌 Parameters

- **`string baseUrl`** – The **base URL** of the RESTful service.  
- **`string opName`** – The **name of the operation** to invoke.  
- **`string[] paramName`** – An array containing the **parameter names** for the operation.  
- **`string[] input`** – An array containing the **argument values** for the operation.  
