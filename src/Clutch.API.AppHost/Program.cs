using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add our redis cache and postgres DB to management
// One question I still have is how this is converted to remote deployments
// IE are these resources pushed to the managed services on Azure?
var redis = builder.AddRedis("redis");
// We change the configuration depending on env
// It is also possible to specify the exact image to use:
// .WithImage
// .WithImageTag
var postgres = (builder.Environment.IsProduction()) switch
{
    true  => builder.AddPostgres("postgres")
                    .PublishAsAzurePostgresFlexibleServer(),
    false => builder.AddPostgres("postgres")
};

var containerImageDb = postgres.AddDatabase("containerImageDb");

builder.AddProject<Projects.Clutch_API>("clutch-api")
    .WithReference(redis)
    .WithReference(containerImageDb);

builder.Build().Run();