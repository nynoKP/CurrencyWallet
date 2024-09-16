using CurrencyWallet.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace CurrencyWallet.Workers
{
    public class ParseAndCheckData : IWorker
    {
        private readonly ILogger<ParseAndCheckData> _logger;

        public ParseAndCheckData(ILogger<ParseAndCheckData> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteJobAsync(IJobClient jobClient, IJob job)
        {
            _logger.LogInformation($"Executing process {job.ProcessInstanceKey} job {job.Key}");

            try
            {
                var variables = JObject.Parse(job.Variables);
                var messageContent = JsonConvert.DeserializeObject<MessageContentDTO>(variables["MessageContent"].ToString());
                dynamic updateVariables = null;
                if (messageContent != null && messageContent.IsValid())
                {
                    updateVariables = new
                    {
                        IsParsed = true,
                        Pinfl = messageContent.MessageContent.client_info.pinfl,
                        Method = messageContent.Method,
                        Currency = messageContent.MessageContent.products.currency,
                        NciClientId = messageContent.MessageContent.client_info.nci_number,
                        UniqueId = messageContent.MessageContent.system.unique_id
                    };
                }
                else
                {
                    updateVariables = new
                    {
                        IsParsed = false
                    };
                }

                await jobClient.NewCompleteJobCommand(job.Key)
                    .Variables((string)JsonConvert.SerializeObject(updateVariables))
                    .Send();
            }
            catch (Exception ex)
            {
                var error = $"Executing process {job.ProcessInstanceKey} job {job.Key} Error: " + ex.ToString();

                _logger.LogError(error);

                await jobClient.NewThrowErrorCommand(job.Key)
                    .ErrorCode(error)
                    .Send();
            }
        }
    }
}
