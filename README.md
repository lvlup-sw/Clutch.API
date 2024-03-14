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

## Sub-project Outline:

### ASP.NET Web API Backend
- **Authentication and Authorization:** Secure user management and role-based access control for the web portal. Consider technologies like ASP.NET Core Identity for authentication.
- **Server Management API Endpoints:** Create RESTful API endpoints in your ASP.NET controllers for:
    - Creating/deleting/updating game server configurations
    - Launching/stopping/monitoring game servers
    - Listing available game server images
    - Handling user configuration/data uploads
- **Job Queue:** Employ a reliable job/task queue (e.g., Hangfire, RabbitMQ) to asynchronously handle long-running server deployment processes.

### MariaDB Database
- **Game Server Configurations:** Store base game server images, user-selected configurations, and server deployment metadata.
- **User Data:** Maintain user accounts, permissions, and any related billing or usage information.

### Web Portal (Frontend)
    Technology Choices: Popular choices include React, Angular, or Vue.js. You may even consider Blazor for a full C# stack option.
    UI Components: Design reusable UI components for:
        Displaying available game images
        User-friendly configuration forms (settings, mod lists, etc.)
        Server status dashboard
        User profile and settings management

### Sub-Projects (Modularization)

    Image Management
        Image Repository: A storage solution (object storage like AWS S3 or equivalent) for storing game server images.
        Image Handling Service: A service layer for uploading, indexing, and versioning images, including any necessary metadata.

    Server Deployment & Provisioning
        Deployment Integration: Decide whether to deploy servers on cloud providers (AWS, Azure, GCP) or manage your own hardware. Accordingly, integrate with cloud provider APIs or create a provisioning system for local infrastructure.
        Containerization (Consider): Container technologies like Docker greatly simplify server image deployment and management.

    Configuration Management
        Game-Specific Configuration: Develop a way to manage configuration files, templates, or scripts for the various games you support, enabling users to customize them easily.

### 1. Add gRPC Service
- Install the necessary NuGet packages for gRPC.
- Define your gRPC service using Protocol Buffers in `.proto` files.
- Write tests for the service interface.
- Implement the service interface generated from the `.proto` file.

### 2. Configure gRPC
- Update the Startup.cs file to include the gRPC service in the application's services.
- Configure the app to use HTTP/2, as required by gRPC.
- Write integration tests to ensure the gRPC configuration works as expected.

### 3. Add Swagger Documentation
- Install the Swashbuckle.AspNetCore NuGet package.
- Configure Swagger in the Startup.cs file.
- Add and configure Swagger middleware in the Configure method.
- Write integration tests to ensure the Swagger documentation works as expected.

### 4. Configure Swagger for gRPC
- Use the protobuf-net.Grpc.AspNetCore package to generate Swagger documentation for gRPC services.
- Write integration tests to ensure the Swagger documentation for gRPC services works as expected.

### 5. Connect to a Database
- Write tests for the data access logic.
- Use Entity Framework Core to connect to a database.
- Implement data access logic for your API.

### 6. Add Authentication and Authorization
- Write tests for the authentication and authorization mechanisms.
- Implement authentication and authorization for your API using ASP.NET Core Identity, JWT, or another method.

### 7. Add Logging and Monitoring
- Write tests for the logging and monitoring mechanisms.
- Implement logging and monitoring for your API using Serilog, Application Insights, or another method.

### 8. Implement API Endpoints
- Define controllers for your API endpoints.
- Write tests for each endpoint.
- Implement the logic for each endpoint.

### 9. Configure Caching
- Write tests for the caching mechanism.
- Implement caching for your API using the CacheProvider library.

### 10. Write Terraform Scripts for Infrastructure as Code
- Write tests to ensure the Terraform scripts work as expected.
- Write Terraform scripts to define and provision the infrastructure for your application.

### 11. Create a CI/CD Pipeline
- Write tests to ensure the CI/CD pipeline works as expected.
- Set up a continuous integration and continuous deployment pipeline using GitHub Actions.

### 12. Deploy the Application
- Publish the application to a server or cloud platform.

### 13. Setup Load Balancing and Scaling
- Write tests to ensure the load balancing and scaling configuration works as expected.
- Configure load balancing and scaling for your application using Kubernetes.

### 14. Add Rate Limiting and Throttling
- Write tests for the rate limiting and throttling mechanisms.
- Implement rate limiting and throttling for your API.

### 15. Add Health Checks
- Write tests for the health checks.
- Implement health checks for your API using the Health Checks library.

### 16. Connect to AWS Services (EKS, Parameter Store, S3, etc)
- Write tests to ensure the connection to AWS services works as expected.
- Connect your application to AWS services such as EKS, Parameter Store, S3, etc.