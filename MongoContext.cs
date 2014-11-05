using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MongoDB.AspNet.Identity
{
    internal class MongoContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoContext" /> class. Uses DefaultConnection name if none was
        ///     specified.
        /// </summary>
        public MongoContext()
            : this("DefaultConnection")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoContext" /> class. Uses supplied connectionStringOrUrl
        /// </summary>
        /// <param name="connectionStringOrUrl">Connection string parameter</param>
        public MongoContext(string connectionStringOrUrl)
        {

        }

        /// <summary>
        ///     Gets the database from connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>MongoDatabase.</returns>
        /// <exception cref="System.Exception">No database name specified in connection string</exception>
        public MongoDatabase GetDatabaseFromSqlStyle(string connectionString)
        {
            var conString = new MongoConnectionStringBuilder(connectionString);
            MongoClientSettings settings = MongoClientSettings.FromConnectionStringBuilder(conString);
            MongoServer server = new MongoClient(settings).GetServer();
            if (conString.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return server.GetDatabase(conString.DatabaseName);
        }

        /// <summary>
        ///     Gets the database from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>MongoDatabase.</returns>
        public MongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            var client = new MongoClient(url);
            MongoServer server = client.GetServer();
            if (url.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return server.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        /// <summary>
        ///     Uses connectionString to connect to server and then uses databae name specified.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dbName">Name of the database.</param>
        /// <returns>MongoDatabase.</returns>
        public MongoDatabase GetDatabase(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            return server.GetDatabase(dbName);
        }
    }
}