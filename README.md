# Clutch.API

## What is Clutch.API?

Clutch.API is a .NET web API designed to fetch, build, and store container image manifests. This production-ready microservice is a learning exercise in modern cloud-native architecture and SOLID principles, and is meant to serve as a model for future services.

## Future Vision

Clutch.API will evolve into a suite of microservices facilitating the seamless deployment of Kubernetes-managed workloads. The primary use case is currently gaming servers, with potential expansion to general business applications.

## Key Achievements

*   **Minimal API:** Clutch.API is built using the .NET Minimal API framework, which allows for smaller, more efficient APIs.
*   **Polly Resiliency:** The API is resilient to network failures and other disruptions, thanks to the use of the Polly library.
*   **Advanced Caching Strategies:** Clutch.API uses Redis caching and DataFerry to improve performance and reduce load on the database.
*   **Builder Pattern:** The builder pattern is used to create services, which enforces the single responsibility principle.
*   **Full .NET Aspire Integration:** Clutch.API is fully integrated with .NET Aspire, which allows for distributed local development.

## Technologies Used

*   **.NET 8**
*   **Redis**
*   **PostgreSQL**
*   **Polly**
*   **Terraform**
*   **GitHub Actions**
*   **Azure App Service**
*   **Azure Container Registry**
*   **Azure Service Bus**
*   **Azure API Management**
*   **Azure Database for PostgreSQL Flexible Server**
*   **Azure App Configuration & Key Vault**
*   **Azure Monitor Application Insights**

## Building Locally

Clone the repo and build the solution. Make sure `launchSettings.json` has the `ASPNETCORE_ENVIRONMENT` property set to `Development`.

## Contributing/Questions

If you have any questions or would like to contribute to the project, please feel free to reach out to me on GitHub.