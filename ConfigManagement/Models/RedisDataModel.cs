using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigManagement.Models
{
    public class RedisDataModel
    {
        public string RedisAddr { get; set; }
        public int RedisDB { get; set; }
        public string Type { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public int TTL { get; set; }
        public DateTime? TTLTime { get; set; }
    }
}
