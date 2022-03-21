# Assignment

## Justin's Assignment

The projecte consists of three apps where are implemented with .Net Core 6.0 
-  Console app
-  Taxation Service Library
-  Unit Test 

The Console app instantiates the tax proxy service with TaxJar REST API required key, version and baseUrl to make two method calls defined in tax proxy service itself and show the result. 

The taxation service library has some key implemenation details

- TaxServiceModel vs TaxJarModel 
  - With Bounded Context designed in mind, the two separated domain models direct the outer layer and tax domain layer to communicate with multiple tax calculators that you need to connect with application services, controller, and so on.
  - IMapper class to map between these two model contracts when the request passed in. 
  
- Dependency Injection & Inversion of control on ITaxCalculator for TaxProxyService
  - The TaxProxyService takes list of ITaxCalcuators in the constructor as parameter and determines what tax calculator should be used durning runtime. 
  - The calculatorType in both TaxRateRequest and TaxForOrderRequest to indicate what correponding tax calculator is selected during the request. It can be determined by rule engine for business rule required.
  
- HttpClient vs RestShap 
  - When making lots of asynchronous calls to REST Api, the httpClient performs two times much better than RestShap does.
  - Using IHttpClientFactory manages the pooling and lifetime of underlying HttpClientMessageHandler instances to avoids common DSN problems that occur when manually manging HttpClient lifetime.

The MSUnit Test
  - Using Mog object framework to mock objects. Test various cases on both TaxProxyService and TaxJarCalculator instance.
