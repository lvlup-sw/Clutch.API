var builder = DistributedApplication.CreateBuilder(args);

// Add our redis cache and postgres DB to management
// One question I still have is how this is converted to remote deployments
// IE are these resources pushed to the managed services on Azure?
var redis = builder.AddRedis("redis");
var postgres = builder.AddPostgres("postgres");
// We can specify the exact image to use:
// .WithImage
// .WithImageTag

var containerImageDb = postgres.AddDatabase("containerImageDb");

builder.AddProject<Projects.Clutch_API>("clutch-api")
    .WithReference(redis)
    .WithReference(containerImageDb);

builder.Build().Run();
