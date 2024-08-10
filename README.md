# Clutch.API

## What is Clutch.API?

### Current State:

Clutch.API is a .NET 8 web API designed to fetch, build, and store container image manifests. This production-ready microservice is a learning exercise in modern cloud-native architecture and SOLID principles, and is meant to service as a model for future services.

### Future Vision:

Clutch.API will evolve into a suite of microservices facilitating the seamless deployment of Kubernetes-managed workloads. The primary use case is currently gaming servers, with potential expansion to general business applications.

## What is Clutch.API's Stack?

### Manifest API Service
- .NET 8 with Aspire integration
- Redis caching via StackExchange.Redis
- PostgreSQL database
- Polly for resiliency
- Terraform for infrastructure scripting
- GitHub Actions for CI/CD

### Distributed Deployment
- Azure App Service
- Azure Container Registry
- Azure Service Bus
- Azure API Management
- Azure Database for PostgreSQL Flexible Server
- Azure App Configuration & Key Vault
- Azure Monitor Application Insights

## Building Locally
WIP
