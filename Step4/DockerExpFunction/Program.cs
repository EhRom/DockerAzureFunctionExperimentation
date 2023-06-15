using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((IFunctionsWorkerApplicationBuilder builder) =>
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = Environment.GetEnvironmentVariable("redisConnectionString");
            options.InstanceName = Environment.GetEnvironmentVariable("redisInstanceName");
        });
    })
    .Build();

host.Run();