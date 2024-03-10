# ASP.NET Web API Development Task List

The objective of this project is to develop a comprehensive ASP.NET Web API that will act as the backbone for a microservices architecture.
This involves setting up a local SQL database, a Redis cluster, and a Kubernetes cluster to host the application.
Additionally, this requires establishing all the necessary supporting infrastructure, including CI/CD pipelines, logging, monitoring, etc.

High-Level Task List for Developing an ASP.NET Web API:
	- gRPC Service
	- Swagger Page
	- MariaDB Database
	- JWT Authentication
	- Serilog Logging
	- API Endpoints
	- Redis Caching
	- Terraform Scripts
	- Github Actions CI/CD
	- AWS EKS Orchestration

## 1. Add gRPC Service
- Install the necessary NuGet packages for gRPC.
- Define your gRPC service using Protocol Buffers in `.proto` files.
- Write tests for the service interface.
- Implement the service interface generated from the `.proto` file.

## 2. Configure gRPC
- Update the Startup.cs file to include the gRPC service in the application's services.
- Configure the app to use HTTP/2, as required by gRPC.
- Write integration tests to ensure the gRPC configuration works as expected.

## 3. Add Swagger Documentation
- Install the Swashbuckle.AspNetCore NuGet package.
- Configure Swagger in the Startup.cs file.
- Add and configure Swagger middleware in the Configure method.
- Write integration tests to ensure the Swagger documentation works as expected.

## 4. Configure Swagger for gRPC
- Use the protobuf-net.Grpc.AspNetCore package to generate Swagger documentation for gRPC services.
- Write integration tests to ensure the Swagger documentation for gRPC services works as expected.

## 5. Connect to a Database
- Write tests for the data access logic.
- Use Entity Framework Core to connect to a database.
- Implement data access logic for your API.

## 6. Add Authentication and Authorization
- Write tests for the authentication and authorization mechanisms.
- Implement authentication and authorization for your API using ASP.NET Core Identity, JWT, or another method.

## 7. Add Logging and Monitoring
- Write tests for the logging and monitoring mechanisms.
- Implement logging and monitoring for your API using Serilog, Application Insights, or another method.

## 8. Implement API Endpoints
- Define controllers for your API endpoints.
- Write tests for each endpoint.
- Implement the logic for each endpoint.

## 9. Configure Caching
- Write tests for the caching mechanism.
- Implement caching for your API using the CacheProvider library.

## 10. Write Terraform Scripts for Infrastructure as Code
- Write tests to ensure the Terraform scripts work as expected.
- Write Terraform scripts to define and provision the infrastructure for your application.

## 11. Create a CI/CD Pipeline
- Write tests to ensure the CI/CD pipeline works as expected.
- Set up a continuous integration and continuous deployment pipeline using GitHub Actions.

## 12. Deploy the Application
- Publish the application to a server or cloud platform.

## 13. Setup Load Balancing and Scaling
- Write tests to ensure the load balancing and scaling configuration works as expected.
- Configure load balancing and scaling for your application using Kubernetes.

## 14. Add Rate Limiting and Throttling
- Write tests for the rate limiting and throttling mechanisms.
- Implement rate limiting and throttling for your API.

## 15. Add Health Checks
- Write tests for the health checks.
- Implement health checks for your API using the Health Checks library.

## 16. Connect to AWS Services (EKS, Parameter Store, S3, etc)
- Write tests to ensure the connection to AWS services works as expected.
- Connect your application to AWS services such as EKS, Parameter Store, S3, etc.