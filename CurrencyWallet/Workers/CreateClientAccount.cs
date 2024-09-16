using Camunda.Worker;
using CurrencyWallet.Models;
using Infrastructure.Models.AnorHub.Account.NciAccountCreate;
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
    public class CreateClientAccount : IWorker
    {
        private readonly ILogger<CreateClientAccount> _logger;
        private readonly IAnorHubService _anorHubService;

        public CreateClientAccount(ILogger<CreateClientAccount> logger, IServiceManager serviceManager)
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
                var nciAccountCreateResponse = await _anorHubService.NciAccountCreate(new NciAccountCreateRequestDTO(new NciAccountCreateRequestDTO.Account()
                {
                    AccBal = "22616",
                    ClientId = variables["NciClientId"].ToString(),
                    Currency = GetCurrency(variables["Currency"].ToString()),
                    IdOrder = "000",
                    OrderBetween = "001;199",
                    Name = string.Format("{0} {1}", GetCurrency(variables["Currency"].ToString()), "Электронный кошелек"),
                    SignOpen = "0"
                }));
                dynamic updateVariables = null;
                var nciAccountCreateModel = nciAccountCreateResponse.Model;
                if (nciAccountCreateResponse != null && nciAccountCreateModel != null && nciAccountCreateModel.Result.Status == "SUCCESS")
                {
                    updateVariables = new
                    {
                        AccountCreateObject = nciAccountCreateModel,
                        IsSuccess = true,
                        IsAccountCreated = true
                    };
                }
                else
                {
                    updateVariables = new
                    {
                        IsSuccess = false,
                        SupportForm = new SupportFormDTO()
                        {
                            Error = NoEx(() => nciAccountCreateResponse.Exception.ToString()),
                            Request = NoEx(() => nciAccountCreateResponse.Response.RequestMessage.Content.ReadAsStringAsync().Result),
                            Response = NoEx(() => nciAccountCreateResponse.Response.Content.ReadAsStringAsync().Result)
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
            
            // Например, здесь можно добавить логику обработки задачи

            // Завершение задачи
            await jobClient.NewCompleteJobCommand(job.Key)
                .Send();
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
