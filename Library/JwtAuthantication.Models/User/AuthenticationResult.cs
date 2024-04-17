using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Models.User {
    public class AuthenticationResult {
        public string Token { get; set; }
        public Guid RefreshToken { get; set; }
        public DateTime Expiry { get; set; }
    }
}
