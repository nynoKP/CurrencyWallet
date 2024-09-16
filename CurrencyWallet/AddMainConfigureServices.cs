using Infrastructure.Extensions.Queue;
using Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyWallet
{
    public static class MainConfigureServices
    {
        public static IServiceCollection AddMainConfigureServices(this IServiceCollection services)
        {
            var configuration_ = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                optional: true)
            .Build();

            SD.ProcessId = configuration_["ProcessId"];
            SD.ZeebeUrl = configuration_["ApiSettings:ZeebeUrl"];
            SD.RabbitMQSettings = configuration_.GetSection("RabbitMQ").Get<RabbitMQSettings>();
            SD.AnorHubAPI = configuration_.GetSection("ApiSettings:AnorHubAPI").Get<ApiSetting>();

            return services;
        }
    }
}
