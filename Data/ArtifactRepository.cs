using openstig_read_api.Models;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Translators;
using Microsoft.Extensions.Options;

namespace openstig_read_api.Data {
    public class ArtifactRepository : IArtifactRepository
    {
        private readonly ArtifactContext _context = null;

        public ArtifactRepository(IOptions<Settings> settings)
        {
            _context = new ArtifactContext(settings);
        }

        public async Task<IEnumerable<Artifact>> GetAllArtifacts()
        {
            try
            {
                return await _context.Artifacts
                        .Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        // query after Id or InternalId (BSonId value)
        //
        public async Task<Artifact> GetArtifact(string id)
        {
            try
            {
                return await _context.Artifacts.Find(artifact => artifact.id == new Guid(id)).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        // query after body text, updated time, and header image size
        //
        public async Task<IEnumerable<Artifact>> GetArtifact(string bodyText, DateTime updatedFrom, long headerSizeLimit)
        {
            try
            {
                var query = _context.Artifacts.Find(artifact => artifact.title.Contains(bodyText) &&
                                    artifact.updatedOn >= updatedFrom);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
        
        public async Task AddArtifact(Artifact item)
        {
            try
            {
                await _context.Artifacts.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> RemoveArtifact(string id)
        {
            try
            {
                DeleteResult actionResult 
                    = await _context.Artifacts.DeleteOneAsync(
                        Builders<Artifact>.Filter.Eq("Id", id));

                return actionResult.IsAcknowledged 
                    && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> UpdateArtifact(string id, Artifact body)
        {
            var filter = Builders<Artifact>.Filter.Eq(s => s.id.ToString(), id);
            var update = Builders<Artifact>.Update
                            .Set(s => s, body)
                            .CurrentDate(s => s.updatedOn);

            try
            {
                UpdateResult actionResult 
                    = await _context.Artifacts.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged
                    && actionResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> RemoveAllArtifacts()
        {
            try
            {
                DeleteResult actionResult 
                    = await _context.Artifacts.DeleteManyAsync(new BsonDocument());

                return actionResult.IsAcknowledged
                    && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
        public async Task<long> CountChecklists(){
            try {
                long result = await _context.Artifacts.CountDocumentsAsync(Builders<Artifact>.Filter.Empty);
                return result;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<Artifact>> GetLatestArtifacts(int number)
        {
            try
            {
                return await _context.Artifacts.Find(_ => true).SortByDescending(y => y.updatedOn).Limit(number).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<object>> GetCountByType()
        {
            try
            {
                var groupArtifactItemsByType = _context.Artifacts.Aggregate()
                        .Group(s => s.type,
                        g => new ArtifactCount {type = g.Key, count = g.Count()}).ToListAsync();

                return await groupArtifactItemsByType;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}