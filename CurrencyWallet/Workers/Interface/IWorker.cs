using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace CurrencyWallet
{
    public interface IWorker
    {
        public Task ExecuteJobAsync(IJobClient jobClient, IJob job);
    }
}
