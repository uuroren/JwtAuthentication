﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Models.Login {
    public class AuthenticationModel {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
    }
}
