using SqlSugar;

namespace ConfigManagement.Models
{
    [SugarTable("DataBaseInfo")]
    public class DataBaseInfo : BaseInfo
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DataBaseType { get; set; }

        /// <summary>
        /// 业务类型 
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// 账套号
        /// </summary>
        public string AccountSetNumber { get; set; }

        /// <summary>
        /// 账套名称
        /// </summary>
        public string AccountSetName { get; set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string UserPassword { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DataBaseName { get; set; }

        /// <summary>
        /// 数据库密码
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string DataBasePassword { get; set; }

        /// <summary>
        /// 数据库链接串附加内容
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string ConnectionStringOther { get; set; }
    }
}
