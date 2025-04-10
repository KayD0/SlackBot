using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SlackBot
{
    public class SlackBot
    {
        private readonly ILogger _logger;

        public SlackBot(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SlackBot>();
        }

        [Function("DiarySummury")]
        public void DiarySummury([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
