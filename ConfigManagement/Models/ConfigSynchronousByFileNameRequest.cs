using System.Collections.Generic;

namespace ConfigManagement.Models
{
    public class ConfigSynchronousByFileNameRequest
    {
        /// <summary>
        /// 配置文件信息
        /// </summary>
        public List<ConfigInfo> ConfigInfos { get; set; }

        public string FileName { get; set; }
    }
}
