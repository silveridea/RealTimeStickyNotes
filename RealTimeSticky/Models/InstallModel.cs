using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using mvc=System.Web.Mvc;

namespace RealTimeSticky.Models
{
    public class InstallModel
    {
        #region SQL Server properties

        [mvc.AllowHtml]
        public string SqlServerName { get; set; }

        [mvc.AllowHtml]
        public string SqlDatabaseName { get; set; }

        [mvc.AllowHtml]
        public string SqlServerUsername { get; set; }

        [mvc.AllowHtml]
        [DataType(DataType.Password)]
        public string SqlServerPassword { get; set; }

        [mvc.AllowHtml]
        [DataType(DataType.Password)]
        public string SqlServerConfirmPassword { get; set; }
        
        public string SqlAuthenticationType { get; set; }
        
        [DisplayName("Create database if it doesn't exist?")]
        public bool SqlServerCreateDatabase { get; set; }
        #endregion
    }
}