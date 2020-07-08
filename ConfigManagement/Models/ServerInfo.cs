using SqlSugar;

namespace ConfigManagement.Models
{
    [SugarTable("ServerInfo")]
    public class ServerInfo : BaseInfo
    {
        /// <summary>
        /// 业务分类
        /// </summary>
        public string BusinessGuid { get; set; }

        /// <summary>
        /// 类别名
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string BusinessName { get; set; }

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
        public string Password { get; set; }
    }
}
