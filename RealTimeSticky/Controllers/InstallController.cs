using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using System.Data.SqlClient;
using System.Security.AccessControl;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using System.Web.Configuration;
using System.Configuration;
using System.Text;
using System.Web.Security;
using RealTimeSticky.Models;
 
namespace RealTimeSticky.Controllers
{
    public class InstallController : BootstrapBaseController
    {
        #region Utilities
        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="sqlProviderConnectionString">Connection string</param>
        /// <returns>Returns true if the database exists.</returns>
        private bool SqlServerDatabaseExists(string sqlProviderConnectionString)
        {
            bool ret = false;
            SqlConnection conn = new SqlConnection(sqlProviderConnectionString);
            try
            {
                //just try to connect
                conn.Open();
                ret = true;
                conn.Close();
            }
            catch
            {
                ret = false;
            }
            finally
            {
                conn.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// Creates a database on the server.
        /// </summary>
        /// <param name="sqlProviderConnectionString">Connection string</param>
        /// <returns>Error</returns>
        private string CreateDatabase(string sqlProviderConnectionString)
        {
            try
            {
                //parse database name
                var builder = new SqlConnectionStringBuilder(sqlProviderConnectionString);
                var databaseName = builder.InitialCatalog;
                //now create connection string to 'master' dabatase. It always exists.
                builder.InitialCatalog = "master";
                var masterCatalogConnectionString = builder.ToString();
                string query = string.Format("CREATE DATABASE [{0}] COLLATE SQL_Latin1_General_CP1_CI_AS", databaseName);

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
        /// Will return true if file is read only or non existent
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckIsReadOnly(string path)
        {
            if (System.IO.File.Exists(path))
            {
                FileInfo fInfo = new FileInfo(path);
                return fInfo.IsReadOnly;
            }
            return true;
        }

        /// <summary>
        /// Check permissions
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="checkRead">Check read</param>
        /// <param name="checkWrite">Check write</param>
        /// <param name="checkModify">Check modify</param>
        /// <param name="checkDelete">Check delete</param>
        /// <returns>Resulr</returns>
        private bool CheckPermissions(string path, bool checkRead, bool checkWrite, bool checkModify, bool checkDelete)
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            bool flag8 = false;
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            System.Security.AccessControl.AuthorizationRuleCollection rules = null;
            try
            {
                rules = Directory.GetAccessControl(path).GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            catch
            {
                return true;
            }
            try
            {
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (!current.User.Equals(rule.IdentityReference))
                    {
                        continue;
                    }
                    if (AccessControlType.Deny.Equals(rule.AccessControlType))
                    {
                        if ((FileSystemRights.Delete & rule.FileSystemRights) == FileSystemRights.Delete)
                            flag4 = true;
                        if ((FileSystemRights.Modify & rule.FileSystemRights) == FileSystemRights.Modify)
                            flag3 = true;

                        if ((FileSystemRights.Read & rule.FileSystemRights) == FileSystemRights.Read)
                            flag = true;

                        if ((FileSystemRights.Write & rule.FileSystemRights) == FileSystemRights.Write)
                            flag2 = true;

                        continue;
                    }
                    if (AccessControlType.Allow.Equals(rule.AccessControlType))
                    {
                        if ((FileSystemRights.Delete & rule.FileSystemRights) == FileSystemRights.Delete)
                        {
                            flag8 = true;
                        }
                        if ((FileSystemRights.Modify & rule.FileSystemRights) == FileSystemRights.Modify)
                        {
                            flag7 = true;
                        }
                        if ((FileSystemRights.Read & rule.FileSystemRights) == FileSystemRights.Read)
                        {
                            flag5 = true;
                        }
                        if ((FileSystemRights.Write & rule.FileSystemRights) == FileSystemRights.Write)
                        {
                            flag6 = true;
                        }
                    }
                }
                foreach (IdentityReference reference in current.Groups)
                {
                    foreach (FileSystemAccessRule rule2 in rules)
                    {
                        if (!reference.Equals(rule2.IdentityReference))
                        {
                            continue;
                        }
                        if (AccessControlType.Deny.Equals(rule2.AccessControlType))
                        {
                            if ((FileSystemRights.Delete & rule2.FileSystemRights) == FileSystemRights.Delete)
                                flag4 = true;
                            if ((FileSystemRights.Modify & rule2.FileSystemRights) == FileSystemRights.Modify)
                                flag3 = true;
                            if ((FileSystemRights.Read & rule2.FileSystemRights) == FileSystemRights.Read)
                                flag = true;
                            if ((FileSystemRights.Write & rule2.FileSystemRights) == FileSystemRights.Write)
                                flag2 = true;
                            continue;
                        }
                        if (AccessControlType.Allow.Equals(rule2.AccessControlType))
                        {
                            if ((FileSystemRights.Delete & rule2.FileSystemRights) == FileSystemRights.Delete)
                                flag8 = true;
                            if ((FileSystemRights.Modify & rule2.FileSystemRights) == FileSystemRights.Modify)
                                flag7 = true;
                            if ((FileSystemRights.Read & rule2.FileSystemRights) == FileSystemRights.Read)
                                flag5 = true;
                            if ((FileSystemRights.Write & rule2.FileSystemRights) == FileSystemRights.Write)
                                flag6 = true;
                        }
                    }
                }
                bool flag9 = !flag4 && flag8;
                bool flag10 = !flag3 && flag7;
                bool flag11 = !flag && flag5;
                bool flag12 = !flag2 && flag6;
                bool flag13 = true;
                if (checkRead)
                {
                    flag13 = flag13 && flag11;
                }
                if (checkWrite)
                {
                    flag13 = flag13 && flag12;
                }
                if (checkModify)
                {
                    flag13 = flag13 && flag10;
                }
                if (checkDelete)
                {
                    flag13 = flag13 && flag9;
                }
                return flag13;
            }
            catch (IOException)
            {
            }
            return false;
        }

        /// <summary>
        /// Sets or adds the specified connection string in the ConnectionStrings section
        /// </summary>
        /// <param name="name">ConnectionString name</param>
        /// <param name="efConnectionString">Connection string</param>
        private bool SaveConnectionString(string name, string efConnectionString)
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration("~");

                if (config.ConnectionStrings.ConnectionStrings[name] != null)
                {
                    config.ConnectionStrings.ConnectionStrings[name].ConnectionString = efConnectionString;
                    config.ConnectionStrings.ConnectionStrings[name].ProviderName = "System.Data.EntityClient";
                }
                else
                {
                    config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(name, efConnectionString, "System.Data.EntityClient"));
                }
                config.Save(ConfigurationSaveMode.Modified);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string RunSQLScripts(string pathToScriptFile, string sqlProviderConnectionString)
        {
            List<string> statements = new List<string>();

            using (Stream stream = System.IO.File.OpenRead(pathToScriptFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                string statement = string.Empty;
                while ((statement = ReadNextStatementFromStream(reader)) != null)
                {
                    statements.Add(statement);
                }
            }
            try
            {
                foreach (string stmt in statements)
                {
                    using (SqlConnection conn = new SqlConnection(sqlProviderConnectionString))
                    {
                        conn.Open();
                        SqlCommand command = new SqlCommand(stmt, conn);
                        command.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }
        
        private string ReadNextStatementFromStream(StreamReader reader)
        {
            StringBuilder sb = new StringBuilder();

            string lineOfText;

            while (true)
            {
                lineOfText = reader.ReadLine();
                if (lineOfText == null)
                {
                    if (sb.Length > 0)
                        return sb.ToString();
                    else
                        return null;
                }

                if (lineOfText.Trim().ToUpper() == "GO")
                    break;

                sb.Append(lineOfText + Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion

        #region Public
        public ActionResult Index()
        {
            if (!string.IsNullOrEmpty(Common.GetConnectionString()))
                return RedirectToAction("Index", "Home");

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            var model = new InstallModel()
            {
                SqlAuthenticationType = "sqlauthentication",
                SqlServerCreateDatabase = true
            };

            if (!TryWriteWebConfig())
            {
                ModelState.AddModelError("", "The file web.config is read only. Please make it writable.");
            }
            return View(model);
        }


        [HttpPost]
        public ActionResult Index(InstallModel model)
        {
            if (!string.IsNullOrEmpty(Common.GetConnectionString()))
                return RedirectToAction("Index", "Home");

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;
        
            //values
            if (string.IsNullOrEmpty(model.SqlServerName))
                ModelState.AddModelError("", "SQL Server name is required");
            if (string.IsNullOrEmpty(model.SqlDatabaseName))
                ModelState.AddModelError("", "Database name is required");

            //authentication type
            if (model.SqlAuthenticationType.Equals("sqlauthentication", StringComparison.InvariantCultureIgnoreCase))
            {
                //SQL authentication
                if (string.IsNullOrEmpty(model.SqlServerUsername))
                    ModelState.AddModelError("", "SQL username is required");
                if (string.IsNullOrEmpty(model.SqlServerPassword))
                    ModelState.AddModelError("", "SQL password is required");
                if (string.IsNullOrEmpty(model.SqlServerConfirmPassword))
                    ModelState.AddModelError("", "SQL password confirm is required");

                if (!string.IsNullOrEmpty(model.SqlServerPassword) &&
                    !string.IsNullOrEmpty(model.SqlServerConfirmPassword) &&
                    model.SqlServerPassword != model.SqlServerConfirmPassword)
                {
                    ModelState.AddModelError("", "SQL passwords do not match");
                }
            }

            //Consider granting access rights to the resource to the ASP.NET request identity. 
            //ASP.NET has a base process identity 
            //(typically {MACHINE}\ASPNET on IIS 5 or Network Service on IIS 6 and IIS 7, 
            //and the configured application pool identity on IIS 7.5) that is used if the application is not impersonating.
            //If the application is impersonating via <identity impersonate="true"/>, 
            //the identity will be the anonymous user (typically IUSR_MACHINENAME) or the authenticated request user.

            //validate permissions
            string rootDir = Server.MapPath("~/");
            var dirsToCheck = new List<string>();
            dirsToCheck.Add(rootDir + "Uploads");
            dirsToCheck.Add(rootDir + "App_Data");
            foreach (string dir in dirsToCheck)
            {
                if (!CheckPermissions(dir, false, true, true, true))
                {
                    ModelState.AddModelError("", string.Format("The '{0}' account is not granted with Modify permission on folder '{1}'. Please configure these permissions.", WindowsIdentity.GetCurrent().Name, dir));
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string sqlProviderConnectionString = Common.CreateSqlProviderConnectionString(model.SqlAuthenticationType == "windowsauthentication",
                            model.SqlServerName, model.SqlDatabaseName,
                            model.SqlServerUsername, model.SqlServerPassword);
                    

                    if (model.SqlServerCreateDatabase)
                    {
                        if (!SqlServerDatabaseExists(sqlProviderConnectionString))
                        {
                            //create database
                            var errorCreatingDatabase = CreateDatabase(sqlProviderConnectionString);
                            if (!String.IsNullOrEmpty(errorCreatingDatabase))
                                throw new Exception(errorCreatingDatabase);
                            else
                            {
                                //Database cannot be created sometimes. Weird! Seems to be Entity Framework issue
                                //that's just wait 10 seconds
                                Thread.Sleep(10000);
                            }
                        }
                    }
                    else
                    {
                        //check whether database exists
                        if (!SqlServerDatabaseExists(sqlProviderConnectionString))
                            throw new Exception("Database does not exist or you don't have permissions to connect to it. ");
                    }  

                    //run scripts
                    string result = RunSQLScripts(Server.MapPath("~/Install/CreateDB.sql"), sqlProviderConnectionString);
                    string efConnectionString = Common.CreateEFConnectionString(sqlProviderConnectionString);
                    if (string.IsNullOrEmpty(result))
                    {
                        //save user data
                        //RTSEntities ctx = new DAL.RTSEntities(efConnectionString);
                        Thread.Sleep(4000);                       
                    }
                    else
                    {
                        throw new Exception(result);
                    }

                    //save connection string
                    if (!SaveConnectionString("RTSEntities", efConnectionString))
                    {
                        throw new Exception(HttpUtility.HtmlDecode("The installer couldn't update the web.config file on your server. This may be caused by limited file system permissions. Please open your web.config file manually in Notepad and add the following: &lt;connectionStrings&gt;&lt;add name=\"EntaskerEntities\" connectionString=\"" + sqlProviderConnectionString + "\"/&gt;&lt;/connectionStrings&gt;"));
                    }

                    //restart application so it will pick new connection string
                    //RestartAppDomain();

                    //Redirect to install successful page
                    return RedirectToAction("InstallSuccess", "Install");
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError("", exception.Message);
                }
            }
            return View(model);
        }

        public ViewResult InstallSuccess()
        {
            return View();
        }

        /// <summary>
        /// Restart application domain
        /// </summary>
        public virtual void RestartAppDomain()
        {
            if (Common.GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
            {
                //full trust
                HttpRuntime.UnloadAppDomain();

                TryWriteGlobalAsax();
            }
            else
            {
                //medium trust
                bool success = TryWriteWebConfig();
                if (!success)
                {
                    throw new Exception("mobx.mobi needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                        "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                        "- run the application in a full trust environment, or" + Environment.NewLine +
                        "- give the application write access to the 'web.config' file.");
                }

                success = TryWriteGlobalAsax();
                if (!success)
                {
                    throw new Exception("mobx.mobi needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                        "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                        "- run the application in a full trust environment, or" + Environment.NewLine +
                        "- give the application write access to the 'Global.asax' file.");
                }
            }
        }
        #endregion

        #region Private
        private bool TryWriteWebConfig()
        {
            try
            {
                // In medium trust, "UnloadAppDomain" is not supported. Touch web.config
                // to force an AppDomain restart.
                System.IO.File.SetLastWriteTimeUtc(Server.MapPath("~/web.config"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryWriteGlobalAsax()
        {
            try
            {
                System.IO.File.SetLastWriteTimeUtc(Server.MapPath("~/global.asax"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
