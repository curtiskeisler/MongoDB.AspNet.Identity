using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    ///     Class UserStore.
    /// </summary>
    /// <typeparam name="TUser">The type of the t user.</typeparam>
    public class UserStore<TUser> : IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserStore<TUser>,
        IUserLockoutStore<TUser, string>,
        IUserTwoFactorStore<TUser, string>,
        IUserPhoneNumberStore<TUser>
        where TUser : IdentityUser
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
        private const string collectionName = "AspNetUsers";


        private MongoContext _mongoContext;

        #endregion

        #region Constructors
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserStore{TUser}" /> class. Uses DefaultConnection name if none was
        ///     specified.
        /// </summary>
        public UserStore() : this("DefaultConnection")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserStore{TUser}" /> class. Uses name from ConfigurationManager or a
        ///     mongodb:// Url.
        /// </summary>
        /// <param name="connectionNameOrUrl">The connection name or URL.</param>
        public UserStore(string connectionNameOrUrl)
        {
            _mongoContext = new MongoContext(connectionNameOrUrl);

            if (connectionNameOrUrl.ToLower().StartsWith("mongodb://"))
            {
                db = _mongoContext.GetDatabaseFromUrl(new MongoUrl(connectionNameOrUrl));
            }
            else
            {
                string connStringFromManager =
                    ConfigurationManager.ConnectionStrings[connectionNameOrUrl].ConnectionString;
                if (connStringFromManager.ToLower().StartsWith("mongodb://"))
                {
                    db = _mongoContext.GetDatabaseFromUrl(new MongoUrl(connStringFromManager));
                }
                else
                {
                    db = _mongoContext.GetDatabaseFromSqlStyle(connStringFromManager);
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserStore{TUser}" /> class. Uses name from ConfigurationManager or a
        ///     mongodb:// Url.
        ///     Database can be specified separately from connection server.
        /// </summary>
        /// <param name="connectionNameOrUrl">The connection name or URL.</param>
        /// <param name="dbName">Name of the database.</param>
        public UserStore(string connectionNameOrUrl, string dbName)
        {
            if (connectionNameOrUrl.ToLower().StartsWith("mongodb://"))
            {
                db = _mongoContext.GetDatabase(connectionNameOrUrl, dbName);
            }
            else
            {
                db = _mongoContext.GetDatabase(ConfigurationManager.ConnectionStrings[connectionNameOrUrl].ConnectionString, dbName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser}"/> class using a already initialized Mongo Database.
        /// </summary>
        /// <param name="mongoDatabase">The mongo database.</param>
        public UserStore(MongoDatabase mongoDatabase)
        {
            db = mongoDatabase;
        }


            /// <summary>
        ///     Initializes a new instance of the <see cref="UserStore{TUser}" /> class.
        /// </summary>
        /// <param name="connectionName">Name of the connection from ConfigurationManager.ConnectionStrings[].</param>
        /// <param name="useMongoUrlFormat">if set to <c>true</c> [use mongo URL format].</param>
        [Obsolete("Use UserStore(connectionNameOrUrl)")]
        public UserStore(string connectionName, bool useMongoUrlFormat)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            if (useMongoUrlFormat)
            {
                var url = new MongoUrl(connectionString);
                db = _mongoContext.GetDatabaseFromUrl(url);
            }
            else
            {
                db = _mongoContext.GetDatabaseFromSqlStyle(connectionString);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds the claim asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="claim">The claim.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task AddClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
            {
                user.Claims.Add(new IdentityUserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });
            }


            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets the claims asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{IList{Claim}}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult(result);
        }

        /// <summary>
        ///     Removes the claim asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="claim">The claim.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            return Task.FromResult(0);
        }


        /// <summary>
        ///     Creates the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task CreateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection<TUser>(collectionName).Insert(user);

            return Task.FromResult(user);
        }

        /// <summary>
        ///     Deletes the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task DeleteAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection(collectionName).Remove((Query.EQ("_id", ObjectId.Parse(user.Id))));
            return Task.FromResult(true);
        }

        /// <summary>
        ///     Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindByIdAsync(string userId)
        {
            ThrowIfDisposed();
            TUser user = db.GetCollection<TUser>(collectionName).FindOne((Query.EQ("_id", ObjectId.Parse(userId))));
            return Task.FromResult(user);
        }

        /// <summary>
        ///     Finds the by name asynchronous.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            
            TUser user = db.GetCollection<TUser>(collectionName).FindOne((Query.EQ("UserName", userName)));
            return Task.FromResult(user);
        }

        /// <summary>
        ///     Updates the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task UpdateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection<TUser>(collectionName)
                .Update(Query.EQ("_id", ObjectId.Parse(user.Id)), Update.Replace(user), UpdateFlags.Upsert);

            return Task.FromResult(user);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }

        /// <summary>
        ///     Adds the login asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="login">The login.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
            {
                user.Logins.Add(login);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        ///     Finds the user asynchronous.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            TUser user = null;
            user =
                db.GetCollection<TUser>(collectionName)
                    .FindOne(Query.And(Query.EQ("Logins.LoginProvider", login.LoginProvider),
                        Query.EQ("Logins.ProviderKey", login.ProviderKey)));

            return Task.FromResult(user);
        }

        /// <summary>
        ///     Gets the logins asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{IList{UserLoginInfo}}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Logins.ToIList());
        }

        /// <summary>
        ///     Removes the login asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="login">The login.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Logins.RemoveAll(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets the password hash asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.String}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<string> GetPasswordHashAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
        ///     Determines whether [has password asynchronous] [the specified user].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<bool> HasPasswordAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PasswordHash != null);
        }

        /// <summary>
        ///     Sets the password hash asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Adds to role asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="role">The role.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task AddToRoleAsync(TUser user, string role)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase))
                user.Roles.Add(role);

            return Task.FromResult(true);
        }

        /// <summary>
        ///     Gets the roles asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{IList{System.String}}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<IList<string>>(user.Roles);
        }

        /// <summary>
        ///     Determines whether [is in role asynchronous] [the specified user].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="role">The role.</param>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<bool> IsInRoleAsync(TUser user, string role)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase));
        }

        /// <summary>
        ///     Removes from role asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="role">The role.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task RemoveFromRoleAsync(TUser user, string role)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Roles.RemoveAll(r => String.Equals(r, role, StringComparison.InvariantCultureIgnoreCase));

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets the security stamp asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.String}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<string> GetSecurityStampAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.SecurityStamp);
        }

        /// <summary>
        ///     Sets the security stamp asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="stamp">The stamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Throws if disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"></exception>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        #endregion

#region "IUserEmailStore Methods"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<TUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            TUser user = db.GetCollection<TUser>(collectionName).FindOne((Query.EQ("Email", email)));
            return Task.FromResult(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetEmailAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<string>(user.Email);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<bool>(user.EmailConfirmed);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task SetEmailAsync(TUser user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Email = email;
            return Task.FromResult(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        /// <summary>
        /// return the Access Failed Count for users.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<int>(user.AccessFailedCount);
        }

        /// <summary>
        /// return the User Account Lockout Status.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<bool>(user.LockoutEnabled);
        }

        /// <summary>
        /// returns the Lockout End Date
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<DateTimeOffset>(user.LockoutEnd);
        }

        /// <summary>
        /// Increments the Access failed count.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            user.AccessFailedCount = user.AccessFailedCount + 1;
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        /// <summary>
        /// Resets the access failed count.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task ResetAccessFailedCountAsync(TUser user)
        {
            user.AccessFailedCount = 0;
            return Task.FromResult<int>(0);
        }

        /// <summary>
        /// Enables the User Lockcout.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult<int>(0);
        }

        /// <summary>
        /// Sets the user lockout end date.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <returns></returns>
        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            user.LockoutEnd = lockoutEnd;
            return Task.FromResult<int>(0);
        }

        /// <summary>
        /// returns if the twofactor authentication is enabled for user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<bool>(user.TwoFactorEnabled);
        }

        /// <summary>
        /// Enables/Disables the Two Factor Authentication
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult<int>(0);
        }

        /// <summary>
        /// returns the User Phone Number
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<string>(user.PhoneNumber);
        }

        /// <summary>
        /// returns a boolean if the user phonenumber is confirmed or not.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<bool>(user.PhoneNumberConfirmed);
        }

        /// <summary>
        /// Sets the user phone number
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult<int>(0);
        }

        /// <summary>
        /// set's boolean value if the users phonenumber is confirmed or not
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult<int>(0);
        }

#endregion

    }
}
        