using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace CurrencyWallet.Workers
{
    public class CheckClientUniqueness : IWorker
    {
        private readonly ILogger<CheckClientUniqueness> _logger;
        private readonly IZeebeClient _zeebeClient;

        public CheckClientUniqueness(ILogger<CheckClientUniqueness> logger, IZeebeClient zeebeClient)
        {
            _logger = logger;
            _zeebeClient = zeebeClient;
        }

        public async Task ExecuteJobAsync(IJobClient jobClient, IJob job)
        {
            _logger.LogInformation($"Executing process {job.ProcessInstanceKey} job {job.Key}");
            
            try
            {
                var variables = JObject.Parse(job.Variables);
                //тут будет проверка на уникальность процесса
                var updateVariables = new
                {
                    IsUniqueness = true
                };
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
