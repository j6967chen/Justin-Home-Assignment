# IMCTaxationService

## IMC Assignment

The projecte consists of three apps where are implemented with .Net Core 6.0 
-  Console app
-  Taxation Service Library
-  Unit Test 

The Console app instantiates the tax proxy service with TaxJar REST API required key, version and baseUrl to make two method calls defined in tax proxy service itself and show the result. 

The taxation service library has some key implemenation details
- TaxServiceModel vs TaxJarModel 
  - The two models separate bounded domain context so that presentation layer will use the same model contract to communicate no matter what underneath tax calculator it chooses.
  - IMapper class to map between these two model contracts.
- Dependency Injection & Inversion of control on ITaxCalculator 
  - The calculatorType in TaxRateRequest & TaxForOrderRequest to indicate what correponding tax calculator is selected during the request. It can be determined by rule engine for business rule required.
  - The TaxProxyService takes list of ITaxCalcuators in the constructor as parameter and determines what tax calculator should be used. 
- HttpClient vs RestShap 
  - When making lots of asynchronous calls to REST Api, the httpClient performs two times much better than RestShap does.
  - Using IHttpClientFactory manages the pooling and lifetime of underlying HttpClientMessageHandler instances to avoids common DSN problems that occur when manually manging HttpClient lifetime.

The MSUnit Test
  - Using Mog library to mock class instance. Test various cases on both TaxProxyService and TaxJarCalculator instance.
