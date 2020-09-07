# Financial-grade Identity Hub & Provider (FIdHP)
The Financial-grade Identity Hub & Provider (FIdHP) with API that authenticates users/authorizes clients and returns various tokens. 
This way this IdP federates multiple service providers (SaaS apps) and can act as the key part of the whole FIdM 
(Federated Identity Management) mechanism at any company. \
It is an extremely secure, rugged and highly available microservice with Multi-AZ deployment for AWS, decent level of redundancy 
of its key components in place for high scalability and eliminating SPFs (Single Point of Failures), 
and with smart performance optimizations for efficient multithreaded processing and data operations. \
For fault tolerance, several fungible token generation approaches are implemented. \
The IdP microservice is the state of the art, using innovative architectural and engineering approaches. \
It can be treated as the key component for all tamper-proof SSO workflows between various products.

#### For creating JWT tokens, Identity Provider microservice has 4 approaches:
1. via AWS Cognito User Pool - Identity Token creation/validation with RSA DS algorithm
2. via Google Firebase - Identity Token creation/validation with RSA DS algorithm
3. via classes from CoreFX (from System.Security.Cryptography namespace) and Portable.BouncyCastle NuGet package - Identity Token creation/validation with RSA DS algorithm
4. via classes from CoreFX (from System.Security.Cryptography namespace) - Identity Token creation/validation with HMAC DS algorithm

https://github.com/ZhenyaP/cognito-sso-demo \
This is an ASP.NET Web API 2 application whis is a sample on how to programmatically use Cognito Services by using AWS SDK for .NET and
AWS SDK for JavaScript

https://github.com/ZhenyaP/sso-service-caller \
This is a .NET Core 2 Console application with an example how we can call Identity Provider microservice from .NET

Here is the link to the high-level architectural diagram of the FIdHP system:
https://drive.google.com/file/d/1d6JfiGAv8L3lrxaKoaWNE0_0kdYwsRow/view?usp=sharing
