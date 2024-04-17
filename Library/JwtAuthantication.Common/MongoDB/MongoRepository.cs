using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Common.MongoDB {
    public class MongoRepository<T>:IRepository<T> where T : IEntity {
        private readonly IMongoCollection<T> dbCollection;
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;
        private readonly IMongoDatabase _database;

        public MongoRepository(IMongoDatabase database,string collectionName) {
            _database = database;
            dbCollection = database.GetCollection<T>(collectionName);
        }

        public IQueryable<T> GetQueryable() {
            return dbCollection.AsQueryable();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync() {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T,bool>> filter) {
            return await dbCollection.Find(filter).ToListAsync();
        }

        public async Task<T> GetAsync(Guid id) {
            FilterDefinition<T> filter = filterBuilder.Eq(entity => entity.Id,id);
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T,bool>> filter) {
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(T entity) {
            if(entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            await dbCollection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity) {
            if(entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            FilterDefinition<T> filter = filterBuilder.Eq(existingEntity => existingEntity.Id,entity.Id);
            await dbCollection.ReplaceOneAsync(filter,entity);
        }

        public async Task RemoveAsync(Guid id) {
            FilterDefinition<T> filter = filterBuilder.Eq(entity => entity.Id,id);
            await dbCollection.DeleteOneAsync(filter);
        }

        public async Task CreateRangeAsync(T[] entities) {
            if(entities == null)
                throw new ArgumentNullException(nameof(entities));

            await dbCollection.InsertManyAsync(entities);
        }
    }
}
