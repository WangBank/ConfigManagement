using FreeSql.DataAnnotations;
using System.Collections.Generic;
using System.Data.Common;
using FreeSql;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;

namespace ConfigManagement.Models
{
    /// <summary>
    /// 账套信息列表
    /// </summary>
    public class OrganizationList
    {
        /// <summary>
        /// 账套信息列表
        /// </summary>
        public List<Organization> Items { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Total { get; set; }

    }

    /// <summary>
    /// 账套信息dto
    /// </summary>
    [Table(Name = "Organization")]
    public class Organization
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(IsIdentity = true, IsPrimary = true,Name ="Code")]
        public int Guid { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据库类型
        /// 0 oracle,1 sqlserver,2 mysql
        /// </summary>
        public string DbType { get; set; }

        /// <summary>
        /// 数据库地址
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 登录名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DataBaseName { get; set; }

        /// <summary>
        /// 数据库链接
        /// </summary>
        public string ConnectingString { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        public (string connString, FreeSql.DataType dataType,DbConnection dbConnection,string testSql) GetConnString(Organization organization)
        {
            DataType dataType=DataType.Oracle;
            string testSql = string.Empty;
            string connString = string.Empty;
            DbConnection dbConnection = null;
            string serverName = string.Empty;
            switch (organization.DbType)
            {
                case "0":
                    dataType = DataType.Oracle;
                    //user id=user1;password=123456; data source=//127.0.0.1:1521/XE;Pooling=true;Min Pool Size=1
                    serverName = organization.ServerName;
                    if (!organization.ServerName.Contains(':'))
                    {
                        serverName = organization.ServerName + ":1521";
                    }
                    connString = $"user id={organization.UserName};password={organization.Password}; data source=//{serverName}/{organization.DataBaseName};Pooling=true;Min Pool Size=1";
                    dbConnection = new OracleConnection(connString);
                    testSql = "select * from v$version";
                    break;
                case "1":
                    //Data Source=RomensMSystem;Initial Catalog=RomensManage; uid =sa; pwd =romens@ddoa;Pooling=true;Min Pool Size=1
                    dataType = DataType.SqlServer;

                    connString = $"Data Source={serverName.Replace(';',',')};Initial Catalog={organization.DataBaseName}; uid ={organization.UserName}; pwd ={organization.Password};Pooling=true;Min Pool Size=1";
                    dbConnection = new SqlConnection(connString);
                    testSql = "select 1";
                    break;
                case "2":
                    dataType = DataType.MySql;
                    //Data Source=127.0.0.1;Port=3306;User ID=root;Password=root; Initial Catalog=cccddd;Charset=utf8; SslMode=none;Min pool size=1
                    string name = organization.ServerName;
                    if (!organization.ServerName.Contains(':'))
                    {
                        name = organization.ServerName + ":3306";
                    }
                    string[] mysqlIp = name.Split(':');
                    connString = $"server={mysqlIp[0]};port={mysqlIp[1]};database={organization.DataBaseName};uid={organization.UserName};pwd={organization.Password};CharSet=utf8";
                    dbConnection = new MySqlConnection(connString);
                    testSql = "select 1";
                    break;
                default:
                    break;
            }
            return (connString,dataType, dbConnection,testSql);
        }
    }

}
