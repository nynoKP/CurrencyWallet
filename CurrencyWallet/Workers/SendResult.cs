using CurrencyWallet.Models;
using Infrastructure.Extensions.Queue;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace CurrencyWallet.Workers
{
    public class SendResult : IWorker
    {
        private readonly ILogger<SendResult> _logger;
        private readonly QueueListenerService _queueListener;

        public SendResult(ILogger<SendResult> logger, QueueListenerService queueListener)
        {
            _logger = logger;
            _queueListener = queueListener;
        }

        public async Task ExecuteJobAsync(IJobClient jobClient, IJob job)
        {
            _logger.LogInformation($"Executing process {job.ProcessInstanceKey} job {job.Key}");

            try
            {
                var variables = JObject.Parse(job.Variables);
                bool isAccountCreated = variables["IsAccountCreated"] != null && variables["IsAccountCreated"].ToString().ToUpper() == "TRUE";
                var messageToDBO = new MessageToDBODTO()
                {
                    InstanceID = job.ProcessInstanceKey,
                    TransactionID = job.ProcessInstanceKey,
                    MessageContent = new MessageContent()
                    {
                        system = new MessageContent.System()
                        {
                            unique_id = variables["UniqueId"].ToString(),
                            application_status = isAccountCreated ? 4 : 3,
                            process_name = "CurrencyWallet"
                        }
                    }
                };
                dynamic updateVariables = new
                {
                    MessageToDBO = messageToDBO
                };
                _queueListener.SendMessage(JsonConvert.SerializeObject(messageToDBO));
                await jobClient.NewCompleteJobCommand(job.Key)
                    .Variables((string)JsonConvert.SerializeObject(updateVariables))
                    .Send();
            }
            catch (Exception ex)
            {

            }

            await jobClient.NewCompleteJobCommand(job.Key)
                    .Send();
        }
    }
}
