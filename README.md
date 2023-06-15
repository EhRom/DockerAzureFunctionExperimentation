# Azure Functions on Docker

Experimentation to host Azure Function in Docker containers:

- Basic function,
- Function using an environment variable,
- Function using a Redis cache (a Console application is also available to test the Redis cache, hosted in Docker).

## Create a basic Azure Function (basic)

### Create the Azure Function

Create an Azure Function to run on Docker: [source](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-arc-custom-container)

Create the function with the `func` tool:

```bash
func init <Azure Function App> --dotnet --docker --worker-runtime dotnet-isolated --target-framework net7.0
```

Navigate to the Function App directory and the create a new function (HTTP Trigger):

```bash
cd <Azure Function App>
func new --name <Function name> --template "HTTP trigger" --authlevel "anonymous"
```

> Set the access level to the function as *Anonymous* (`AuthorizationLevel`)

List outdated NuGet packages:

```bash
dotnet list package --outdated
```

Update all the packages. For each outdated package, run :

```bash
dotnet add package <Outdated package name>
```

Update the function code:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DockerExpFunction
{
    public class HttpFunction
    {
        private readonly ILogger logger;

        public HttpFunction(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<HttpFunction>();
        }

        [Function("HttpFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            string responseContent = $"Welcome to Azure Functions! Date & time (UTC): {DateTime.UtcNow}";

            logger.LogInformation($"Response sent: {responseContent}");

            await response.WriteStringAsync(responseContent);

            return response;
        }
    }
}
```

> Note: the Azure Function can be created using Visual Studio 2022. Use .NET 7 isolated runtime, and check **Enable Docker** option.

### Build & local run the function

Run the function locally:

```bash
func start --port 9000
```

Navigate to the following [uri](http://localhost:9000/api/HttpFunction) to test the function

### Run the Azure Function with Docker

Build the docker image of the Azure Function App:

```bash
docker build -t <image name>:<version> -f <dockerfile path> .
```

E.g.: `docker build -t dockerexpfunction:0.1 -f .\Dockerfile .`

Run the container:

```bash
docker run -p <local machine target port>:<container port> --name <container name> <image name>:<version>
```

E.g.: `docker run -p 9090:80 --name dockerexpfunction dockerexpfunction:0.1`

Navigate to the following [uri](http://localhost:9090/api/HttpFunction) to test the function

### Run the Azure Function using Docker compose

Create a YAML file named `docker-compose.yml` in the above folder of the function project. Add the following content :

```yaml
version: '3'
services:
  function:
    image: <image name>:<version>
    container_name: <compose container name>
    ports:
      - <local machine target port>:<container port>
```

E.g.:

```yaml
version: '3'
services:
  function:
    image: dockerexpfunction:0.1
    container_name: dockercomposeexpfunction
    environment:
      TestEnvVariable: DockerComposeFunction
    ports:
      - 9099:80
```

In the docker compose file directory, run the following command:

```bash
docker-compose up --build -d
```

Navigate to the following [uri](http://localhost:9099/api/HttpFunction) to test the function

#### Sources

- [Docker compose overview](https://docs.docker.com/compose/)
- [Getting started](https://docs.docker.com/compose/gettingstarted/)

### Clean containers and images

Stop the containers created via *docker compose*:

```bash
docker compose down
```

List containers:

```bash
docker ps -a
```

List docker images:

```bash
docker images
```

Remove container

```bash
docker rm <container id>
```

Remove image:

```bash
docker rmi <image id>
```

## Function with an environment variable

In this experiment, an environment variable will be loaded, to complete the response to the user.

The goal is to show how to set an environment variable, in the 3 following cases:

- Local function,
- Function in Docker
- Function in Docker Compose

### Update the Azure Function code

Update the `HttpFunction.cs` class file with the following content:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DockerExpFunction
{
    public class HttpFunction
    {
        private readonly ILogger logger;

        private const string TestEnvVariableCacheKey = "TestEnvVariable";

        public HttpFunction(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<HttpFunction>();
        }

        [Function("HttpFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            string responseContent = $"Welcome to Azure Functions! Date & time (UTC): {DateTime.UtcNow}. Env: {Environment.GetEnvironmentVariable(TestEnvVariableCacheKey)}.";

            logger.LogInformation($"Response sent: {responseContent}");

            await response.WriteStringAsync(responseContent);

            return response;
        }
    }
}
```

Update the `` configuration file with the following content:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "TestEnvVariable": "LocalFunction"
  }
}
```

### Build & local run the function

Run the function locally:

```bash
func start --port 9000
```

Navigate to the following [uri](http://localhost:9000/api/HttpFunction) to test the function and see the new response.

### Run the Azure Function with Docker

Build the docker image of the Azure Function App:

```bash
docker build -t <image name>:<version> -f <dockerfile path> . 
```

E.g.: `docker build -t dockerexpfunction:0.2 -f .\Dockerfile .`

Run the container:

```bash
docker run -p <local machine target port>:<container port> --name <container name> <image name>:<version>
```

E.g.: `docker run -p 9090:80 --name dockerexpfunction -e TestEnvVariable="DockerFunc" dockerexpfunction:0.2`

Navigate to the following [uri](http://localhost:9090/api/HttpFunction) to test the function

### Run the Azure Function using Docker compose

Update the YAML file named `docker-compose.yml` with the following content :

```yaml
version: '3'
services:
  function:
    image: <image name>:<version>
    container_name: <compose container name>
    environment:
      TestEnvVariable: DockerComposeFunction
    ports:
      - <local machine target port>:<container port>
```

E.g.:

```yaml
version: '3'
services:
  function:
    image: dockerexpfunction:0.2
    container_name: dockercomposeexpfunction
    environment:
      TestEnvVariable: DockerComposeFunction
    ports:
      - 9099:80
```

In the docker compose file directory, run the following command:

```bash
docker-compose up --build -d
```

Navigate to the following [uri](http://localhost:9099/api/HttpFunction) to test the function

### Clean containers and images

cf. previous §.

## Create a Redis container and a console app

### Load the image and create the container

Pull the redis image:

```bash
docker pull redis:alpine
```

Create a container and run it:

```bash
docker run --name <container-name> -p 6379:6379 -d redis:alpine
```

E.g.: `docker run --name redis-console -p 6379:6379 -d redis:alpine`

Test the redis cache. Run the command:

```bash
docker exec -it <container-name> redis-cli
```

E.g.: `docker exec -it redis-console redis-cli`

Sample test:

```bash
127.0.0.1:6379> KEYS *
(empty array)
127.0.0.1:6379> MSET test "test string"
OK
127.0.0.1:6379> KEYS *
1) "test"
127.0.0.1:6379> MGET test
1) "test string"
```

### Create a new Console App to test redis

Create a C # console app. Run the command:

```bash
dotnet new console -o <outpout directory path> -n <project name>
```

E.g.: `dotnet new console -o RedisTest.ConsoleApp -n RedisTest.ConsoleApp`

Navigate into the output directory where the console project has just been generated.

Add the following NuGet packaes:

- [Puffix.ConsoleLogMagnifier](https://www.nuget.org/packages/Puffix.ConsoleLogMagnifier/). Command `dotnet add package Puffix.ConsoleLogMagnifier`.
- [StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis/). Command `dotnet add package StackExchange.Redis`.

Edit the `Program.cs` file and add the following code:

```csharp
using Puffix.ConsoleLogMagnifier;
using StackExchange.Redis;

ConsoleHelper.WriteInfo($"Welcome to the Redis test console app.");

const string redisCacheUri = "localhost:6379";
try
{
    ConsoleHelper.Write($"Initialize connection to the redis cache instance ({redisCacheUri})");
    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisCacheUri);

    IDatabase db = redis.GetDatabase();

    const string keyName = "mykey";
    string expectedValue = $"value inserted in Redis cache at {DateTime.UtcNow}";
    ConsoleHelper.Write($"Set key {keyName}");
    await db.StringSetAsync(keyName, expectedValue);

    await Task.Delay(1000);

    string? retrievedValue = await db.StringGetAsync(keyName);

    ConsoleHelper.WriteSuccess($"Retrieved value for the key {keyName}: {retrievedValue} (expected value: {expectedValue})");
}
catch (Exception error)
{
    ConsoleHelper.WriteError("Error while testing Redis", error);
}

ConsoleHelper.Write("Press any key to quit...");
Console.ReadKey();
```

Run the console with the following command:

```bash
dotnet run .\<project name>.csproj
```

E.g.: `dotnet run .\RedisTest.ConsoleApp.csproj`

### Clean containers and images

cf. previous §.

## Function with a redis cache

### Update the Azure Function code

In the Function project, reference the **[Microsoft.Extensions.Caching.StackExchangeRedis](https://www.nuget.org/packages/Microsoft.Extensions.Caching.StackExchangeRedis)** NuGet package. Run the command (in the Function project directory):

```bash
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

Update the `Program.cs` file class with the folloqing code:

```csharp
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
```

Add the `redisConnectionString` and `redisInstanceName` paramters to the `local.settings.json` file:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "redisConnectionString": "localhost:6379",
    "redisInstanceName": "LocalFuncCache"
  }
}
```

Update the `HttpFunction.cs` file class with the folloqing code:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DockerExpFunction
{
    public class HttpFunction
    {
        private readonly ILogger logger;
        private readonly IDistributedCache distributedCache;

        private const string RedisInstanceNameCacheKey = "redisInstanceName";

        private const string LastCallCacheKey = "LastCall";
        private const string TotalCallCountCacheKey = "TotalCallCount";

        public HttpFunction(ILoggerFactory loggerFactory, IDistributedCache distributedCache)
        {
            logger = loggerFactory.CreateLogger<HttpFunction>();
            this.distributedCache = distributedCache;
        }

        [Function("HttpFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string? previousCallDate = await distributedCache.GetStringAsync(LastCallCacheKey);
                string? totalCallCountValue = await distributedCache.GetStringAsync(TotalCallCountCacheKey);

                if (string.IsNullOrEmpty(totalCallCountValue) || !int.TryParse(totalCallCountValue, out int totalCallCount))
                    totalCallCount = 0;

                await distributedCache.SetStringAsync(TotalCallCountCacheKey, (++totalCallCount).ToString());
                await distributedCache.SetStringAsync(LastCallCacheKey, DateTime.UtcNow.ToString("O"));

                HttpResponseData response = request.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                string responseContent = $"Welcome to Azure Functions! Date & time (UTC): {DateTime.UtcNow}. Env: {Environment.GetEnvironmentVariable(RedisInstanceNameCacheKey)}. {totalCallCount} calls to this function. Last call date: {previousCallDate}";
                logger.LogInformation($"Response sent: {responseContent}");

                await response.WriteStringAsync(responseContent);

                return response;
            }
            catch(Exception error)
            {
                logger.LogError(error, "Error while processing request.");

                HttpResponseData response = request.CreateResponse(HttpStatusCode.InternalServerError);

                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString($"Internal server. You don't need to know more :P");

                return response;
            }
        }
    }
}
```

### Build & local run the function

Pull the redis image:

```bash
docker pull redis:alpine
```

Create a container and run it:

```bash
docker run --name <container-name> -p 6379:6379 -d redis:alpine
```

E.g.: `docker run --name redis-console -p 6379:6379 -d redis:alpine`

Run the function locally:

```bash
func start --port 9000
```

Navigate to the following [uri](http://localhost:9000/api/HttpFunction) to test the function and see the new response.

### Run the Azure Function with Docker

Build the docker image of the Azure Function App:

```bash
docker build -t <image name>:<version> -f <dockerfile path> . 
```

E.g.: `docker build -t dockerexpfunction:0.4 -f .\Dockerfile .`

Create the network:

```bash
docker network create <network name>
```

E.g.: `docker network create dockerexpnetwork`

List the docker network:

```bash
docker network ls
```

Run the Redis container:

```bash
docker run --name <container-name> -p 6379:6379 --net <network name> -d redis:alpine
```

E.g.: `docker run --name redis-console -p 6379:6379 --net dockerexpnetwork -d redis:alpine`

Run the container:

```bash
docker run -p <local machine target port>:<container port> --name <container name> --net <network name> -e FUNCTIONS_WORKER_RUNTIME="dotnet-isolated" -e redisConnectionString="<redis host name>.<container name>:<redis port>" -e redisInstanceName="<redis instance name>" <image name>:<version>
```

E.g.: `docker run -p 9090:80 --name dockerexpfunction --net dockerexpnetwork -e FUNCTIONS_WORKER_RUNTIME="dotnet-isolated" -e redisConnectionString="redis-console.dockerexpnetwork:6379" -e redisInstanceName="DockerFuncCache" dockerexpfunction:0.4`

Navigate to the following [uri](http://localhost:9090/api/HttpFunction) to test the function

### Run the Azure Function using Docker compose

>>> TODO ICI : update code

Update the YAML file named `docker-compose.yml` with the following content :

```yaml
version: '3'
services:
  redis:
    image: redis:alpine
    container_name: <redis container name>
    ports:
      - <redis local machine target port>:<redis container port>
  function:
    image: <image name>:<version>
    container_name: <compose container name>
    environment:
      redisConnectionString: <redis container name>:<redis local machine target port>
      redisInstanceName: <redis instance name>
    ports:
      - <local machine target port>:<container port>
    depends_on:
      - <redis container name>
```

E.g.:

```yaml
version: '3'
services:
  redis:
    image: redis:alpine
    container_name: redis
    ports:
      - 6379:6379
  function:
    image: dockerexpfunction:0.4
    container_name: dockercomposeexpfunction
    environment:
      redisConnectionString: redis:6379
      redisInstanceName: DockerComposeFuncCache
    ports:
      - 9099:80
    depends_on:
      - redis
```

In the docker compose file directory, run the following command:

```bash
docker-compose up --build -d
```

Navigate to the following [uri](http://localhost:9099/api/HttpFunction) to test the function

### Run the Azure Function using Docker compose

### Clean containers and images

cf. previous §.

Remove the network:

```bash
docker network rm <network name>
```

E.g.: `docker network rm dockerexpnetwork`


## Next steps

- Register the image in Azure Container Registry
- Test to build the image in the docker compose
- Run the containers to Rapsberry
- Add the deconz container
- Store the docker compose on the Raspberry

---
---
---

```bash
```

E.g.: ``

## Host on Raspberry

Install Docker Compose

```bash
sudo apt-get install docker-compose-plugin
```
