using Azure;
using Azure.Data.Tables;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEAS.Entities
{
    public class UserTableEntity : Azure.Data.Tables.ITableEntity
    {
        public bool Enabled { get; set; }
        public string Domain { get; set; }
        
        public bool IsProtected { get; set;  }
        
        public string Name { get;set; }

        public string HashedEmail { get; set; }

        public bool CanCreateApplication { get; set; }
       
        public string Group { get; set; }

        public string OTP { get; set; }

        public DateTime? LastLoginAttempt { get; set; }

        public DateTime? LastSuccessFullLogin { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
