using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using MongoDB.Driver;

namespace MongoDB.AspNet.Identity
{

    /// <summary>
    /// ASP.NET Identity MongoDB UserRole Store.
    /// </summary>
    /// <typeparam name="TRole">UserRole Type should inherit from IdentityRole</typeparam>
    public class RoleStore<TRole> : IRoleStore<TRole, string>, IRoleStore <TRole>
         where TRole : IdentityRole
    {

        #region Private Methods & Variables

        /// <summary>
        ///     The database
        /// </summary>
        private readonly MongoDatabase db;

        /// <summary>
        ///     The _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The AspNetUsers collection name
        /// </summary>
        private const string collectionName = "AspNetRoles";


        private MongoContext _mongoContext;

        #endregion

        /// <summary>
        /// Creates a new User Role of Type TRole asynchronously
        /// </summary>
        /// <param name="role">New Role Name</param>
        /// <returns>return a Task</returns>
        public System.Threading.Tasks.Task CreateAsync(TRole role)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the Specified User role if exists.
        /// </summary>
        /// <param name="role">Input role</param>
        /// <returns>returns Task</returns>
        public System.Threading.Tasks.Task DeleteAsync(TRole role)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// find a user role by id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns>Returns an UserRole as TaskResult</returns>
        public System.Threading.Tasks.Task<TRole> FindByIdAsync(string roleId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds an UserRole by name
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns>Returns an UserRole as TaskResult</returns>
        public System.Threading.Tasks.Task<TRole> FindByNameAsync(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates as existing UserRole
        /// </summary>
        /// <param name="role">UserRole to be updated</param>
        /// <returns>Returns a Task object</returns>
        public System.Threading.Tasks.Task UpdateAsync(TRole role)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Disposes the RoleStore instance.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}