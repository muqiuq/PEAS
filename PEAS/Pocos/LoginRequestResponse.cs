using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEAS.Pocos
{
    public class LoginRequestResponse
    {

        public bool Success { get; set; }

        public string Message { get; set; }

        public string Token { get; set; }

        public string RedirectUrl { get; set; }
    }
}
