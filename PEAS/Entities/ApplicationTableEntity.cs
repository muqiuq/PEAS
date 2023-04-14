using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEAS.Entities
{
    public class ApplicationTableEntity : Azure.Data.Tables.ITableEntity
    {
        public string SharedSecret { get; set; }

        public string AppToken { get; set; }

        public string Name { get; set; }

        public string RedirectUrl { get; set; }


        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

    }
}
