var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

builder.AddProject<Projects.Clutch_API>("clutch-api")
    .WithReference(cache);

builder.Build().Run();
