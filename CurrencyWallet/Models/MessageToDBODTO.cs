using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyWallet.Models
{
    public class MessageContent
    {
        [JsonProperty("system")]
        public System system { get; set; }
        public class System
        {
            [JsonProperty("unique_id")]
            public string unique_id { get; set; }

            [JsonProperty("claim_id")]
            public object claim_id { get; set; }

            [JsonProperty("application_status")]
            public int application_status { get; set; }

            [JsonProperty("process_name")]
            public string process_name { get; set; }

            [JsonProperty("product_id")]
            public object product_id { get; set; }

            [JsonProperty("approved_amount")]
            public object approved_amount { get; set; }

            [JsonProperty("approved_term")]
            public object approved_term { get; set; }
        }
    }
    public class MessageToDBODTO
    {
        [JsonProperty("TransactionID")]
        public long TransactionID { get; set; }

        [JsonProperty("InstanceID")]
        public long InstanceID { get; set; }

        [JsonProperty("MessageType")]
        public string MessageType { get; set; } = "DataRequest";

        [JsonProperty("Action")]
        public string Action { get; set; } = "Dbo";

        [JsonProperty("Method")]
        public string Method { get; set; } = "LoanProposal";

        [JsonProperty("MessageContent")]
        public MessageContent MessageContent { get; set; }
      
    }

}
