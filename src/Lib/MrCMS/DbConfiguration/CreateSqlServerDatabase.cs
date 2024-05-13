using System;
using System.Data.SqlClient;
using System.Threading;
using Microsoft.Data.SqlClient;
using MrCMS.Apps;
using MrCMS.Installation.Models;

namespace MrCMS.DbConfiguration
{
    public abstract class CreateSqlServerDatabase
    {
        public IDatabaseProvider CreateDatabase(InstallModel model)
        {
            string connectionString = GetConnectionString(model);

            if (model.SqlServerCreateDatabase)
            {
                if (!SqlServerDatabaseExists(connectionString))
                {
                    //create database
                    string errorCreatingDatabase = CreateSqlDatabase(connectionString);
                    if (!String.IsNullOrEmpty(errorCreatingDatabase))
                        throw new Exception(errorCreatingDatabase);

                    int tries = 0;
                    while (tries < 50)
                    {
                        if (NewDbIsReady(connectionString))
                            break;
                        tries++;
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                //check whether database exists
                if (!SqlServerDatabaseExists(connectionString))
                    throw new Exception("Database does not exist or you don't have permissions to connect to it");
            }

            return GetProvider(connectionString);
        }

        private bool NewDbIsReady(string connectionString)
        {
            try
            {
                var provider = GetProvider(connectionString);
                new NHibernateConfigurator(provider, new MrCMSAppContext()).CreateSessionFactory();
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected abstract IDatabaseProvider GetProvider(string connectionString);

        public string GetConnectionString(InstallModel model)
        {
            if (model.SqlConnectionInfo == SqlConnectionInfo.Raw)
            {
                //raw connection string
                return model.DatabaseConnectionString;
            }

            return CreateConnectionString(model);
        }

        /// <summary>
        ///     Creates a database on the server.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Error</returns>
        private string CreateSqlDatabase(string connectionString)
        {
            try
            {
                //parse database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                string databaseName = builder.InitialCatalog;
                //now create connection string to 'master' dabatase. It always exists.
                builder.InitialCatalog = "master";
                string masterCatalogConnectionString = builder.ToString();
                string query = string.Format("CREATE DATABASE [{0}] COLLATE SQL_Latin1_General_CP1_CI_AS",
                    databaseName);

                using (var conn = new SqlConnection(masterCatalogConnectionString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(query, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Format("An error occured when creating database: {0}", ex.Message);
            }
        }

        /// <summary>
        ///     Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Returns true if the database exists.</returns>
        private bool SqlServerDatabaseExists(string connectionString)
        {
            try
            {
                //just try to connect
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string CreateConnectionString(InstallModel model)
        {
            var builder = new SqlConnectionStringBuilder();
            bool trustedConnection = model.SqlAuthenticationType == SqlAuthenticationType.Windows;
            builder.IntegratedSecurity = trustedConnection;
            builder.DataSource = model.SqlServerName;
            builder.InitialCatalog = model.SqlDatabaseName;
            if (!trustedConnection)
            {
                builder.UserID = model.SqlServerUsername;
                builder.Password = model.SqlServerPassword;
            }

            builder.PersistSecurityInfo = false;
            builder.MultipleActiveResultSets = true;
            builder.TrustServerCertificate = true;
            return builder.ConnectionString;
        }
    }
}