using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Models.User {
    public class LoginModel {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string SmsCode { get; set; }
    }
}
