using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JwtAuthentication.Common.MongoDB {
    public static class Extensions {
        public static IServiceCollection AddMongo(this IServiceCollection services) {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            services.AddSingleton(serviceProvider => {
                var configuration = serviceProvider.GetService<IConfiguration>()!;
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var mongoClient = new MongoClient(mongoDbSettings!.ConnectionString);
                return mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
            });
            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services,string collectionName) where T : IEntity {
            services.AddSingleton<IRepository<T>>(serviceProvider => {
                var database = serviceProvider.GetService<IMongoDatabase>()!;
                return new MongoRepository<T>(database,collectionName);
            });

            return services;
        }
    }
}
