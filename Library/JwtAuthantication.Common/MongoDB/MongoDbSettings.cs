using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Common.MongoDB {
    public class MongoDbSettings {
        public string Host { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }
        public int Port { get; init; }
        public string DatabaseName { get; init; }
        //public string ConnectionString => $"mongodb://{Username}:{Password}@{Host}:{Port}/?authSource=admin&readPreference=primary&directConnection=true&ssl=false";
        public string ConnectionString => $"mongodb://{Host}:{Port}/";

    }
}
