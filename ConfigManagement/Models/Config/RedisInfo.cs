using System.Collections.Generic;

namespace ConfigManagement.Models.Config
{
    /// <summary>
    /// Redis.json对应的实体类
    /// </summary>
    public class RedisInfo
    {
        public string Mode { get; set; }

        /// <summary>
        /// 服务名
        /// </summary>
        public string ServiceName { get; set; }
        public List<Server> Servers { get; set; }
        public string Address { get; set; }
        public string InstanceName { get; set; }
        public string Password { get; set; }

        public class Server
        {
            public string IP { get; set; }
            public int Port { get; set; }
        }

    }
}
