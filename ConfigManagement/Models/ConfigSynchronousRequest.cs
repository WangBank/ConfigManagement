using System.Collections.Generic;

namespace ConfigManagement.Models
{
    public class ConfigSynchronousRequest
    {
        /// <summary>
        /// 配置文件信息
        /// </summary>
        public List<ConfigInfo> ConfigInfos { get; set; }

        /// <summary>
        /// 服务器信息
        /// </summary>
        public List<ServerInfo> ServerInfos { get; set; }
    }
}
