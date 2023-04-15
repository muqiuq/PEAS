using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEAS.Pocos
{
    public class LoginRequestPoco
    {
        
        public string AppToken { get; set; }
        public string EMail { get; set; }

        public string OTP { get; set; }
        public string ReqId { get; set; }
    }
}
