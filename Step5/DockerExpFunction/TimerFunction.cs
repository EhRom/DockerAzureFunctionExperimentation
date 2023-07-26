using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DockerExpFunction
{
    public class TimerFunction
    {
        private readonly ILogger logger;

        public TimerFunction(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<TimerFunction>();
        }

        [Function("TimerFunction")]
        public void Run([TimerTrigger("*/15 * * * * *", UseMonitor = true)] string timerInformationCore)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            logger.LogInformation($"Next timer schedule at: {timerInformationCore}");
        }
    }
}
