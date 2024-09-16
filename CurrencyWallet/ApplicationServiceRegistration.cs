using Infrastructure.Extensions.Queue;
using Infrastructure.Models;
using Infrastructure.Services.ServiceManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Reflection;
using Zeebe.Client;

namespace CurrencyWallet
{
    public class ApplicationServiceRegistration
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddNLog();
            });

            services.AddSingleton<IZeebeClient>(provider =>
            {
                return ZeebeClient.Builder()
                    
                    .UseLoggerFactory(new NLogLoggerFactory())
                    .UseGatewayAddress(SD.ZeebeUrl)
                    .UsePlainText() // Отключить SSL 
                    .Build();
            });

            //регаем http клиент
            services.AddHttpClient();
            //регаем менеджер сервисов
            services.AddScoped<IServiceManager, ServiceManager>();

            //сервис слушателя сообщений
            services.AddSingleton<QueueListenerService>();
            services.AddHostedService<QueueListenerService>();

            // Регистрация всех типов, реализующих IWorker
            var workerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(IWorker).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var workerType in workerTypes)
            {
                services.AddTransient(workerType);
            }
        }

        public void Configure(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(ConfigureServices);
        }
    }
}
