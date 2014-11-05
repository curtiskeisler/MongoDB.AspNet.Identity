using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Identity Role Class for MongoDB ASP.NET Identity Implementation
    /// </summary>
    public class IdentityRole : IRole, IRole<string>
    {

        /// <summary>
        /// Uniqueue Identifier for UserRole is of type BsonType.ObjectId
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// User Role Name
        /// </summary>
        public string Name
        {
            get;
            set;
        }
    }
}