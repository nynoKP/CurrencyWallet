using Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System.Reflection;
using Zeebe.Client;

namespace CurrencyWallet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.LoadConfiguration("nlog.config").GetCurrentClassLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                // Получение клиента и регистрация воркеров
                var zeebeClient = host.Services.GetRequiredService<IZeebeClient>();

                var topology = zeebeClient.TopologyRequest().Send().Result;
                logger.Info(topology.ToString());

                //деплой процесса
                var deploy = zeebeClient.NewDeployCommand()
                        .AddResourceFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Process.bpmn"))
                        .AddResourceFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Forms", "SupportForm.form"))
                        .Send().Result;

                //публикация воркеров
                var workerTypes = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => typeof(IWorker).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var workerType in workerTypes)
                {
                    var worker = (IWorker)host.Services.GetRequiredService(workerType);
                    var jobType = worker.GetType().Name;
                    var workerName = SD.ProcessId;
                    try
                    {
                        zeebeClient.NewWorker()
                            .JobType(jobType)
                            .Handler(worker.ExecuteJobAsync)
                            .MaxJobsActive(5)
                            .Name(workerName)
                            .AutoCompletion()
                            .PollInterval(TimeSpan.FromSeconds(1))
                            .PollingTimeout(TimeSpan.FromSeconds(5))
                            .Timeout(TimeSpan.FromSeconds(10))
                            .Open();

                        logger.Info($"Worker '{jobType}' registered successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.Info($"Error registering worker '{jobType}': {ex.Message}");
                    }
                }
                host.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application stopped due to an exception");
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) => services.AddMainConfigureServices())
                .ConfigureServices((_, services) => new ApplicationServiceRegistration().ConfigureServices(services));
    }
}
