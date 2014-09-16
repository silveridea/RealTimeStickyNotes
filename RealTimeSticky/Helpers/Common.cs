using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;

namespace RealTimeSticky
{
    public static class Common
    {
        public static string GetConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings["RTSEntities"] != null &&
                !string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["RTSEntities"].ConnectionString))
            {
                return ConfigurationManager.ConnectionStrings["RTSEntities"].ConnectionString;
            }
            else
                return null;
        }
        public static string ConvertEFConnStringToSQLProviderConnString(string entityConnString)
        {
            var efBuilder = new EntityConnectionStringBuilder(entityConnString);
            var sqlBuilder = new SqlConnectionStringBuilder(efBuilder.ProviderConnectionString);
            sqlBuilder.ApplicationName = null;
            return sqlBuilder.ConnectionString;
        }

        public static string CreateEFConnectionString(bool trustedConnection, string serverName, string databaseName, string userName, string password, int timeout = 0)
        {
            string sqlProviderConnectionString = CreateSqlProviderConnectionString(trustedConnection, serverName, databaseName, userName, password, timeout);
            return CreateEFConnectionString(sqlProviderConnectionString);
        }
        public static string CreateEFConnectionString(string sqlProviderConnectionString)
        {
            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
            //Set the provider name.
            entityBuilder.Provider = "System.Data.SqlClient";
            //entityBuilder.Provider = "System.Data.EntityClient";
            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = sqlProviderConnectionString;

            // Set the Metadata location.
            entityBuilder.Metadata = @"res://*/RTSModel.csdl|res://*/RTSModel.ssdl|res://*/RTSModel.msl";
            string efConnectionString = entityBuilder.ConnectionString;
            return efConnectionString;
        }
        /// <summary>
        /// Create contents of connection strings used by the SqlConnection class
        /// </summary>
        /// <param name="trustedConnection">Avalue that indicates whether User ID and Password are specified in the connection (when false) or whether the current Windows account credentials are used for authentication (when true)</param>
        /// <param name="serverName">The name or network address of the instance of SQL Server to connect to</param>
        /// <param name="databaseName">The name of the database associated with the connection</param>
        /// <param name="userName">The user ID to be used when connecting to SQL Server</param>
        /// <param name="password">The password for the SQL Server account</param>
        /// <param name="timeout">The connection timeout</param>
        /// <returns>Connection string</returns>
        public static string CreateSqlProviderConnectionString(bool trustedConnection, string serverName, string databaseName, string userName, string password, int timeout = 0)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.IntegratedSecurity = trustedConnection;
            builder.DataSource = serverName;
            builder.InitialCatalog = databaseName;
            if (!trustedConnection)
            {
                builder.UserID = userName;
                builder.Password = password;
            }
            builder.PersistSecurityInfo = false;
            builder.MultipleActiveResultSets = true;
            if (timeout > 0)
            {
                builder.ConnectTimeout = timeout;
            }
            builder.ApplicationName = "EntityFramework";
            string sqlProviderConnectionString = builder.ConnectionString;
            return sqlProviderConnectionString;
        }

        private static AspNetHostingPermissionLevel? _trustLevel = null;
        /// <summary>
        /// Finds the trust level of the running application (http://blogs.msdn.com/dmitryr/archive/2007/01/23/finding-out-the-current-trust-level-in-asp-net.aspx)
        /// </summary>
        /// <returns>The current trust level.</returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if (!_trustLevel.HasValue)
            {
                //set minimum
                _trustLevel = AspNetHostingPermissionLevel.None;

                //determine maximum
                foreach (AspNetHostingPermissionLevel trustLevel in
                        new AspNetHostingPermissionLevel[] {
                                AspNetHostingPermissionLevel.Unrestricted,
                                AspNetHostingPermissionLevel.High,
                                AspNetHostingPermissionLevel.Medium,
                                AspNetHostingPermissionLevel.Low,
                                AspNetHostingPermissionLevel.Minimal 
                            })
                {
                    try
                    {
                        new AspNetHostingPermission(trustLevel).Demand();
                        _trustLevel = trustLevel;
                        break; //we've set the highest permission we can
                    }
                    catch (System.Security.SecurityException)
                    {
                        continue;
                    }
                }
            }
            return _trustLevel.Value;
        }
    }
}