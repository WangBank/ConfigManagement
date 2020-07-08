using ConfigManagement.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManagement.Common
{
    public static class ApiUtils
    {
        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<ResultData> InsertData<T>(SqlSugarClient client,T data) where T : BaseInfo, new()
        {
            data.Guid = Guid.NewGuid().ToString("N");
            int result = await client.Insertable(data).ExecuteReturnIdentityAsync();
            if (result > 0)
            {
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "添加失败", null);
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<ResultData> UpdateData<T>(SqlSugarClient client, T data) where T : BaseInfo, new()
        {
            int result = await client.Updateable(data).ExecuteCommandAsync();
            if (result > 0)
            {
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "更新失败", null);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<ResultData> DeleteData<T>(SqlSugarClient client,List<T> data) where T : BaseInfo, new() 
        {
            if (data == null || data.Count == 0)
            {
                return ResultData.CreateResult("-1", "删除失败", null);
            }

            int result = await client.Deleteable<T>().In(data.Select(x => x.Guid).ToArray()).ExecuteCommandAsync();

            if (result > 0)
            {
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "删除失败", null);
            }
        }

        /// <summary>
        /// 把路径转换成当前系统的路径
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToSystemPath(this string source)
        {
            var os = Environment.OSVersion;
            if (os.Platform != PlatformID.Unix && os.Platform != PlatformID.MacOSX)
            {
                source = source.Replace("/", "\\");
            }
            else
            {
                source = source.Replace("\\", "/");
            }

            return source;
        }

        public static int ToInt32(this string source)
        {
            return string.IsNullOrEmpty(source) ? 0 : Convert.ToInt32(source);
        }

        public static string ToNullString(this object source)
        {
            return source == null ? string.Empty : source.ToString();
        }

        /// <summary>
        /// 生成指定字符串的MD5散列值，返回大写串。
        /// </summary>
        /// <param name="srcValue">源字符串</param>
        /// <param name="encodeType">type类型：16位还是32位</param>
        /// <returns>MD5值</returns>
        public static string MD5Encode(string srcValue, string encodeType)
        {
            var result = Encoding.Default.GetBytes(srcValue);
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var output = md5.ComputeHash(result);
            if (encodeType == "16")
                return BitConverter.ToString(output).Replace("-", "").ToUpper().Substring(8, 16);
            else
                return BitConverter.ToString(output).Replace("-", "").ToUpper();
        }

        public static T GetData<T>(this ISession session, string key)
        {
            string value = session.GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
        }

        public static void SetData(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static string GetLocalPath(SqlSugarClient db,string guid, out string errorMessage)
        {
            ConfigInfo config = db.Queryable<ConfigInfo>().Where(x => x.Guid == guid).First();
            if (config == null)
            {
                errorMessage = "本地文件不存在";
                return string.Empty;
            }

            string localPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Configs{config.LocalPath}{config.ConfigName}";
            var os = Environment.OSVersion;
            if (os.Platform != PlatformID.Unix && os.Platform != PlatformID.MacOSX)
            {
                localPath = localPath.Replace("/", "\\");
            }
            else
            {
                localPath = localPath.Replace("\\", "/");
            }

            if (!System.IO.File.Exists(localPath))
            {
                errorMessage = "本地文件不存在";
                return string.Empty;
            }

            errorMessage = "";
            return localPath;
        }

        public static List<string> GetLocalPathByFileName(SqlSugarClient db, string fileName, out string errorMessage)
        {
            List<ConfigInfo> configs = db.Queryable<ConfigInfo>().Where(x => x.ConfigName == fileName).ToList();
            if (configs == null)
            {
                errorMessage = "本地文件不存在";
                return null;
            }
            List<string> path = new List<string>();
            foreach (var config in configs)
            {
                string localPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Configs{config.LocalPath}{config.ConfigName}";
                var os = Environment.OSVersion;
                if (os.Platform != PlatformID.Unix && os.Platform != PlatformID.MacOSX)
                {
                    localPath = localPath.Replace("/", "\\");
                }
                else
                {
                    localPath = localPath.Replace("\\", "/");
                }

                if (!System.IO.File.Exists(localPath))
                {
                    errorMessage = "本地文件不存在";
                    return null;
                }

                path.Add(localPath);
            }

            errorMessage = "";
            return path;
        }

        public static string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 2,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 检测入参
        /// </summary>
        /// <param name="funcParams">接口入参</param>
        /// <param name="keys">要验证的值</param>
        /// <returns>检测结果<成功or失败,keyname></returns>
        public static KeyValuePair<bool, string> CheckFuncParams(JObject funcParams, params string[] keys)
        {
            if (keys == null || keys.Length <= 0)
            {
                return new KeyValuePair<bool, string>(true, null);
            }

            foreach (string key in keys)
            {
                // 判断是否存在该字段
                if (!funcParams.ContainsKey(key))
                {
                    return new KeyValuePair<bool, string>(false, key);
                }

                // 判断是否为null
                if (funcParams[key] == null)
                {
                    return new KeyValuePair<bool, string>(false, key);
                }

                if (string.IsNullOrEmpty(funcParams[key].ToString()))
                {
                    return new KeyValuePair<bool, string>(false, key);
                }
            }

            return new KeyValuePair<bool, string>(true, null);
        }

        /// <summary>
        /// 检测入参
        /// </summary>
        /// <param name="funcParams">接口入参</param>
        /// <param name="keys">需要验证的值</param>
        /// <returns>检测结果<成功or失败,keyname></returns>
        public static KeyValuePair<bool, string> CheckFuncParams(JEnumerable<JToken> funcParams, params string[] keys)
        {
            if (keys == null || keys.Length <= 0)
            {
                return new KeyValuePair<bool, string>(true, null);
            }

            foreach (JToken token in funcParams)
            {
                KeyValuePair<bool, string> keyValue = CheckFuncParams(token as JObject, keys);
                if (!keyValue.Key)
                {
                    return keyValue;
                }
            }

            return new KeyValuePair<bool, string>(true, null);
        }

        public static KeyValuePair<bool, string> CheckFuncParams<T>(T funcParams, params string[] keys)
        {
            if (keys == null || keys.Length <= 0)
            {
                return new KeyValuePair<bool, string>(true, null);
            }

            PropertyInfo[] propertys = funcParams.GetType().GetProperties(); // 获得此模型的公共属性
            foreach (PropertyInfo pi in propertys)
            {
                string name = pi.Name; //名称
                // 判断是否存在该字段
                if (!keys.Contains(name))
                {
                    continue;
                }
                object value = pi.GetValue(funcParams, null);  //值

                if (value == null)
                {
                    return new KeyValuePair<bool, string>(false, name);
                }

                if (string.IsNullOrEmpty(value.ToNullString()))
                {
                    return new KeyValuePair<bool, string>(false, name);
                }

            }
            return new KeyValuePair<bool, string>(true, null);
        }

    }
}
