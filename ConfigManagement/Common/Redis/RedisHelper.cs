using ConfigManagement.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigManagement.Common
{
    public class RedisHelper
    {

        private string redisConnenctionString;

        public volatile ConnectionMultiplexer redisConnection;

        private object redisConnectionLock = new object();

        public RedisHelper(string conn)
        {
            string redisConfiguration = conn;//获取连接字符串

            if (string.IsNullOrWhiteSpace(redisConfiguration))
            {
                throw new ArgumentException("redis config is empty", nameof(redisConfiguration));
            }
            this.redisConnenctionString = redisConfiguration;
            this.redisConnection = GetRedisConnection();
        }

        /// <summary>
        /// 核心代码，获取连接实例
        /// 通过双if 夹lock的方式，实现单例模式
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetRedisConnection()
        {
            //如果已经连接实例，直接返回
            if (this.redisConnection != null && this.redisConnection.IsConnected)
            {
                this.redisConnection.Close();
                this.redisConnection.Dispose();
            }

            //加锁，防止异步编程中，出现单例无效的问题
            lock (redisConnectionLock)
            {
                if (this.redisConnection != null)
                {
                    //释放redis连接
                    this.redisConnection.Dispose();
                }
                try
                {
                    this.redisConnection = ConnectionMultiplexer.Connect(redisConnenctionString);
                }
                catch (Exception)
                {
                    //throw new Exception("Redis服务未启用，请开启该服务，并且请注意端口号，本项目使用的的6319，而且我的是没有设置密码。");
                }
            }
            return this.redisConnection;
        }
        /// <summary>
        /// 清除
        /// </summary>
        public void ClearAll()
        {
            foreach (var endPoint in this.GetRedisConnection().GetEndPoints())
            {
                var server = this.GetRedisConnection().GetServer(endPoint);
                foreach (var key in server.Keys())
                {
                    redisConnection.GetDatabase().KeyDelete(key);
                }
            }
        }

        /// <summary>
        /// 获取全部的键
        /// </summary>
        /// <returns></returns>
        //public List<RedisDataModel> GetAllKeys()
        //{
        //    List<RedisKey> redisKeys = new List<RedisKey>();
        //    foreach (var endPoint in this.GetRedisConnection().GetEndPoints())
        //    {
        //        var server = this.GetRedisConnection().GetServer(endPoint);
        //        if (!server.IsReplica)
        //        {
        //            redisKeys.AddRange(server.Keys());
        //        }
        //    }
        //    List<RedisDataModel> models = new List<RedisDataModel>();
        //    foreach (var key in redisKeys)
        //    {
        //        RedisDataModel model = new RedisDataModel();
        //        model.Key = key;
        //        model.Type = this.GetType(key).ToString();
        //        model.TTL = this.GetExpireTime(key);
        //        models.Add(model);
        //    }
        //    return models;
        //}

        /// <summary>
        /// 获取查询的键（模糊查询）
        /// </summary>
        /// <returns></returns>
        public List<RedisDataModel> GetKeys(int db = -1, string pattern = "*")
        {
            List<RedisKey> redisKeys = new List<RedisKey>();
            foreach (var endPoint in this.GetRedisConnection().GetEndPoints())
            {
                var server = this.GetRedisConnection().GetServer(endPoint);
                if (!server.IsReplica)
                {
                    redisKeys.AddRange(server.Keys(db, pattern));
                }
            }
            List<RedisDataModel> models = new List<RedisDataModel>();
            foreach (var key in redisKeys)
            {
                RedisDataModel model = new RedisDataModel();
                model.Key = key;
                model.Type = this.GetType(key, db).ToString();
                model.TTL = this.GetTTL(key, db);
                models.Add(model);
            }
            return models;
        }

        /// <summary>
        /// 清理模糊查询出来的键
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<bool, string>> ClearKeys(int db = -1, string pattern = "*")
        {
            try
            {
                List<RedisKey> redisKeys = new List<RedisKey>();
                foreach (var endPoint in this.GetRedisConnection().GetEndPoints())
                {
                    var server = this.GetRedisConnection().GetServer(endPoint);
                    if (!server.IsReplica)
                    {
                        redisKeys.AddRange(server.Keys(db, pattern));
                    }
                }
                var _db = this.GetRedisConnection().GetDatabase(db);
                foreach (var key in redisKeys)
                {
                    if (this.Get(key, db))
                    {
                        await _db.KeyDeleteAsync(key);
                    }
                }
                return new Tuple<bool, string>(true, "");
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Get(string key, int db = -1)
        {
            return redisConnection.GetDatabase(db).KeyExists(key);
        }

        /// <summary>
        /// 根据键，查询值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key, int db = -1)
        {
            return redisConnection.GetDatabase(db).StringGet(key);
        }

        /// <summary>
        /// 根据键，查询类型
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedisType GetType(string key, int db = -1)
        {
            return redisConnection.GetDatabase(db).KeyType(key);
        }

        /// <summary>
        /// 根据键，查询过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTTL(string key, int db = -1)
        {
            TimeSpan? timeSpan = GetExpireTime(key);
            if (timeSpan != null)
            {
                return (int)(((TimeSpan)timeSpan).TotalSeconds);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 根据键，查询过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TimeSpan? GetExpireTime(string key, int db = -1)
        {
            TimeSpan? timeSpan = redisConnection.GetDatabase(db).KeyTimeToLive(key);
            return timeSpan;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TEntity Get<TEntity>(string key, int db = -1)
        {
            var value = redisConnection.GetDatabase(db).StringGet(key);
            if (value.HasValue)
            {
                //需要用的反序列化，将Redis存储的Byte[]，进行反序列化
                return SerializeHelper.Deserialize<TEntity>(value);
            }
            else
            {
                return default(TEntity);
            }
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key, int db = -1)
        {
            redisConnection.GetDatabase(db).KeyDelete(key);
        }
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheTime"></param>
        public async Task<Tuple<bool, string>> Set(string Type, string key, object value, TimeSpan? cacheTime, int db = -1)
        {
            try
            {
                if (value != null)
                {
                    //switch(RedisType.String)
                    //序列化，将object值生成RedisValue
                    return new Tuple<bool, string>(await redisConnection.GetDatabase(db).StringSetAsync(key, value.ToString(), cacheTime), "");
                }
                else
                {
                    return new Tuple<bool, string>(false, "值不能为空");
                }
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        /// <summary>
        /// 增加/修改
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValue(string key, byte[] value, int db = -1)
        {
            return redisConnection.GetDatabase(db).StringSet(key, value, TimeSpan.FromSeconds(120));
        }

    }
}
