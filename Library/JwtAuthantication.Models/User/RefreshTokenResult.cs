using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Models.User {
    public class RefreshTokenResult {
        public bool IsValid { get; set; }
        public string UserName { get; set; }
        public Catalog.User.User User { get; set; }
    }
}
