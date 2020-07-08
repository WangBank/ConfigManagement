using ConfigManagement.Models;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ConfigManagement.Common
{
    public class DbFactory : IDbFactory
    {
        private readonly Dictionary<string, ConnectionConfig> _configs;
        //private readonly Dictionary<string, SqlSugarClient> _clients;

        public DbFactory(IOptions<SqlSugarConfig> options)
        {
            //this._clients = new Dictionary<string, SqlSugarClient>();
            _configs = new Dictionary<string, ConnectionConfig>();
            SqlSugarConfig sugarConfig = options.Value;
            foreach (var item in sugarConfig.Connections)
            {
                _configs.Add(item.Name, new ConnectionConfig()
                {
                    ConnectionString = item.ConnectionString.Replace("{programpath}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).ToSystemPath(),
                    DbType = item.DbType,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute,
                    IsShardSameThread = true,
                    //AopEvents = new AopEvents
                    //{
                    //    OnLogExecuting = (sql, p) =>
                    //    {
                    //        Console.WriteLine(sql);
                    //        Console.WriteLine(string.Join(",", p?.Select(it => it.ParameterName + ":" + it.Value)));
                    //    }
                    //}
                });
            }
        }

        public SqlSugarClient GetDbContext(string name)
        {

            
            if (!_configs.TryGetValue(name, out ConnectionConfig config))
            {
                return null;
            }

            //if (_clients.TryGetValue(name,out SqlSugarClient client))
            //{
            //    return client;
            //}
            return new SqlSugarClient(config);

            //client = new SqlSugarClient(config);
            //_clients.Add(name, client);
            //return client;
        }
    }
}
