﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Common {
    public interface IEntity {
        [BsonId]
        Guid Id { get; set; }
    }
}
