using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Tests.Support
{
    public class LoginRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }        

        public string Email { get; set; }

        public string ApiKey { get; set; }
    }
}
