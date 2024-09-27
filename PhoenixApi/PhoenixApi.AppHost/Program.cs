var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PhoenixApi>("phoenixapi");

builder.Build().Run();
