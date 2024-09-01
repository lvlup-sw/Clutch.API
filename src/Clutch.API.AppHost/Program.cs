using Microsoft.Extensions.Hosting;
using Clutch.API.AppHost;

// Note that this is setting up the ORCHESTRATION
// If we have external resources already provisioned
// (ie for Prod env) we need to point the AppHost
// to those resources, otherwise it will provision
// them locally as containers

var builder = DistributedApplication.CreateBuilder(args);

// Add secrets from Azure KeyVault
builder.Configuration.AddAzureKeyVaultSecrets("AzureKeyVault");

// We need to bind the secrets to the correct
// configuration section depending on Environment
if (builder.Environment.IsProduction())
{
    builder.BindProductionSecrets();
}
else
{
    builder.BindDevelopmentSecrets();
}

// Add our redis cache to management
// Prod connects remotely to a resource
// while Dev creates a local container
var redis = (builder.Environment.IsProduction()) switch
{
    true  => builder.AddConnectionString("Redis"),
    false => builder.AddRedis("Redis")
};

// Add our postgresql database to management
// Prod connects remotely to a resource
// while Dev creates a local container
// It is also possible to specify the exact image to use:
// .WithImage
// .WithImageTag
var postgres = (builder.Environment.IsProduction()) switch
{
    true  => builder.AddConnectionString("ContainerImageDb"),
    false => builder.AddPostgres("Postgres")
                    .PublishAsAzurePostgresFlexibleServer()
};

// This is not actually creating a database
// It is creating a CONNECTION STRING for a database
// The provisioning is still required to happen before runtime
// We accomplish this by using EF Core migrations at startup
var containerImageDb = (postgres is IResourceBuilder<PostgresServerResource> server)
    ? server.AddDatabase("ContainerImageDb")
    : postgres;

// Add our app insights to management
var insights = builder.AddConnectionString("AzureAppInsights", "APPLICATIONINSIGHTS_CONNECTION_STRING");

// Add projects to Aspire management
// with references to the resources
builder.AddProject<Projects.Clutch_API>("clutch-api")
    .WithReference(redis)
    .WithReference(containerImageDb)
    .WithReference(insights);

builder.Build().Run();