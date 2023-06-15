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

        private const string LastCallCacheKey = "LastCall";
        private const string TotalCallCountCacheKey = "TotalCallCount";
        private const string RedisConnectionStringCacheKey = "redisConnectionString";
        private const string RedisInstanceNameCacheKey = "redisInstanceName";

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
                logger.LogInformation($"Get values from redis cache (host: {Environment.GetEnvironmentVariable(RedisConnectionStringCacheKey)} / port: {Environment.GetEnvironmentVariable(RedisInstanceNameCacheKey)}");

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
            catch (Exception error)
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