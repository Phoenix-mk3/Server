var builder = DistributedApplication.CreateBuilder(args);

var rabbitMQ = builder.AddRabbitMQ("rabbit")
    .WithManagementPlugin(15672)
    .WithEnvironment("NODENAME", "rabbitph@localhost")
    .WithVolume("phoenix-rabbitmq-host-vol", "/var/lib/rabbitmq");



builder.Build().Run();
