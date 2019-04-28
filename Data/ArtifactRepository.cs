using openrmf_read_api.Models;
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

namespace openrmf_read_api.Data {
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


        private ObjectId GetInternalId(string id)
        {
            ObjectId internalId;
            if (!ObjectId.TryParse(id, out internalId))
                internalId = ObjectId.Empty;

            return internalId;
        }

        // query after Id or InternalId (BSonId value)
        //
        public async Task<Artifact> GetArtifact(string id)
        {
            try
            {
                return await _context.Artifacts.Find(artifact => artifact.InternalId == GetInternalId(id)).FirstOrDefaultAsync();
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

        public async Task<IEnumerable<object>> GetCountByType(string system)
        {
            try
            {
                // show them all by type
                if (string.IsNullOrEmpty(system)) {
                    var groupArtifactItemsByType = _context.Artifacts.Aggregate()
                            .Group(s => s.stigType,
                            g => new ArtifactCount {stigType = g.Key, count = g.Count()}).ToListAsync();
                    return await groupArtifactItemsByType;
                }
                else {
                    var groupArtifactItemsByType = _context.Artifacts.Aggregate().Match(artifact => artifact.system == system)
                            .Group(s => s.stigType,
                            g => new ArtifactCount {stigType = g.Key, count = g.Count()}).ToListAsync();
                    return await groupArtifactItemsByType;
                }
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    
    
        #region Systems
        public async Task<List<ChecklistSystem>> GetAllSystems() 
        {
            try
            {
                List<ChecklistSystem> systems = new List<ChecklistSystem>();
                var match = new BsonDocument();
                var group = new BsonDocument{
                    {"_id", "$system"},
                    {"checklistCount", new BsonDocument {{"$sum", 1}}}
                };
                var results = await _context.Artifacts.Aggregate().Match(match).Group(group).ToListAsync();
                if (results != null) {
                   foreach (BsonDocument item in results)
                    {                    
                        systems.Add(new ChecklistSystem() { 
                            system = item.Elements.ElementAtOrDefault(0).Value.AsString, 
                            checklistCount = item.Elements.ElementAtOrDefault(1).Value.AsInt32 }); 
                    }
                }
                // return it in alpha order
                return systems.OrderBy(x => x.system).ToList();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<Artifact>> GetSystemArtifacts(string system)
        {
            try
            {
                var query = await _context.Artifacts.FindAsync(artifact => artifact.system == system);
                return query.ToList().OrderBy(x => x.title);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
        #endregion
    }
}