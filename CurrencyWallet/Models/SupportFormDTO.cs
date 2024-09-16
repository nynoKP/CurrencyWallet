using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyWallet.Models
{
    public class SupportFormDTO
    {
        public string Request {  get; set; }
        public string Response { get; set; }
        public string Error { get; set; }
        public string Action { get; set; }
    }
}
