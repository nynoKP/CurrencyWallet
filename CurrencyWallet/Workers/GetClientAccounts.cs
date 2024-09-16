using CurrencyWallet.Models;
using Infrastructure.Models.AnorHub.Account.NciAccountGet;
using Infrastructure.Services.AnorHub;
using Infrastructure.Services.ServiceManager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace CurrencyWallet.Workers
{
    public class GetClientAccounts : IWorker
    {
        private readonly ILogger<GetClientAccounts> _logger;
        private readonly IAnorHubService _anorHubService;

        public GetClientAccounts(ILogger<GetClientAccounts> logger, IServiceManager serviceManager)
        {
            _logger = logger;
            _anorHubService = serviceManager.anorHubService;
        }

        public async Task ExecuteJobAsync(IJobClient jobClient, IJob job)
        {
            _logger.LogInformation($"Executing process {job.ProcessInstanceKey} job {job.Key}");

            // Логика выполнения задачи
           
            try
            {
                var variables = JObject.Parse(job.Variables);
                var accountGetResponse = await _anorHubService.NciAccountGet(new NciAccountGetRequestDTO(new NciAccountGetRequestDTO.Account()
                {
                    AccBal = "22616",
                    ClientId = variables["NciClientId"].ToString(),
                    Currency = GetCurrency(variables["Currency"].ToString())
                }));
                dynamic updateVariables = null;
                var accountGetModel = accountGetResponse.Model;
                if (accountGetResponse != null && accountGetResponse.Response.IsSuccessStatusCode && accountGetModel != null && accountGetModel.result.status == "SUCCESS")
                {
                    updateVariables = new
                    {
                        IsSuccess = true,
                        IsClientHasWallet = accountGetModel.result.data.Any(),
                        AccountGetObject = accountGetModel
                    };
                }
                else
                {
                    updateVariables = new
                    {
                        IsSuccess = false,
                        supportForm = new SupportFormDTO()
                        {
                            Error = NoEx(() => accountGetResponse.Exception.ToString()),
                            Request = NoEx(() => accountGetResponse.Response.RequestMessage.Content.ReadAsStringAsync().Result),
                            Response = NoEx(() => accountGetResponse.Response.Content.ReadAsStringAsync().Result)
                        }
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

        private string GetCurrency(string? currency)
        {
            if (currency == null) throw new ArgumentNullException("MessageContent.products.currency");

            if (currency == "UZS") return "000";
            if (currency == "USD") return "840";
            if (currency == "EUR") return "978";
            if (currency == "RUB") return "643";

            return string.Empty;
        }

        private T NoEx<T>(Func<T> func)
        {
            try
            {
                var res = func();
                return res;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
