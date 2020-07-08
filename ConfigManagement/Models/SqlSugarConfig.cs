using Microsoft.Extensions.Options;
using SqlSugar;
using System.Collections.Generic;

namespace ConfigManagement.Models
{
    public class SqlSugarConfig : IOptions<SqlSugarConfig>
    {
        public SqlSugarConfig Value => this;

       // public Dictionary<string, ConnectionConfig> Configs { get; set; }

        public List<Connection> Connections { get; set; }
        public class Connection
        {
            public string ConnectionString { get; set; }
            public DbType DbType { get; set; }

            public string Name { get; set; }
        }
    }
}
