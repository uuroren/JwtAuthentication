using JwtAuthentication.Common;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Catalog.User {
    public class User:IEntity {
        [BsonId]
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string HashPassword { get; set; }
        public string Phone { get; set; }
        public string SmsCode { get; set; }
        public DateTime LastSmsSent { get; set; }
        public string AccessToken { get; set; }
        public Guid RefreshToken { get; set; }
        public DateTime TokenExpiry { get; set; }
        public List<string> Roles { get; set; }
        public bool BlockedUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
