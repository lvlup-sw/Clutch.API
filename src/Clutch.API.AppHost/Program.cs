var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Clutch_API>("clutch-api");

builder.Build().Run();
