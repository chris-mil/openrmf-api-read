using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace openrmf_read_api.Models
{
    [Serializable]
    public class Artifact
    {
        public Artifact () {
            CHECKLIST = new CHECKLIST();
        }

        public DateTime created { get; set; }
        public CHECKLIST CHECKLIST { get; set; }
        public string rawChecklist { get; set; }
        
        // if this is part of a system, list that system.
        // if empty this is just a standalone checklist
        public string system { get; set; }
        public string hostName { get; set;}
        public string stigType { get; set; }
        public string stigRelease { get; set; }
        public string title { get {
            return hostName.Trim() + "-" + stigType.Trim() + "-" + stigRelease.Trim();
        }}
        
        [BsonId]
        // standard BSonId generated by MongoDb
        public ObjectId InternalId { get; set; }

        [BsonDateTimeOptions]
        // attribute to gain control on datetime serialization
        public DateTime? updatedOn { get; set; }

        public Guid createdBy { get; set; }
        public Guid? updatedBy { get; set; }
    }

}