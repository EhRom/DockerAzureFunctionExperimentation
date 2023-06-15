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
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
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