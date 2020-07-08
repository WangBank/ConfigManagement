using ConfigManagement.Models;
using Renci.SshNet;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConfigManagement.Common
{
    public class SftpHelp
    {
        /// <summary>
        /// Sftp下载文件
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localPath">本地文件路径</param>

        public static async Task<ResultData> DownLoad(string host,int port,string username,string password,string remotePath,string localPath)
        {
            try
            {
                using var client = new SftpClient(host, port, username, password);
                client.Connect();
                byte[] buffer = client.ReadAllBytes(remotePath);
                using FileStream fs = new FileStream(localPath, FileMode.Create);
                await fs.WriteAsync(buffer, 0, buffer.Length);
                return ResultData.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", ex.Message, null);
            }
        }

        /// <summary>
        /// Sftp上传文件
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localPath">本地文件路径</param>
        public static async Task<ResultData> UpLoad(string host, int port, string username, string password,string remotePath, string localPath)
        {
            try
            {
                using var client = new SftpClient(host, port, username, password);
                client.Connect();
                using FileStream fileStream = new FileStream(localPath, FileMode.Open);
                using MemoryStream fs = new MemoryStream();
                await fileStream.CopyToAsync(fs);
                fs.Seek(0, SeekOrigin.Begin);
                client.BufferSize = 4 * 1024 * 1024;
                client.UploadFile(fs, remotePath);
                return ResultData.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", ex.Message, null);
            }
        }
    }
}
