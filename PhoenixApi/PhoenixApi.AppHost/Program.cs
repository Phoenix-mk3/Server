var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithPgWeb();
var postgresdb = postgres.AddDatabase("ApiDb");



builder.AddProject<Projects.PhoenixApi>("phoenixapi")
    .WithReference(postgresdb);

builder.Build().Run();
