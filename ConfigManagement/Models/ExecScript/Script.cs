using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigManagement.Models
{
    /// <summary>
    /// 脚本数据信息
    /// </summary>
    public class ScriptList
    {
        /// <summary>
        /// 脚本list
        /// </summary>
        public List<Script> Items { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int Total { get; set; }

    }

    /// <summary>
    /// 脚本信息
    /// </summary>
    [Table(Name = "Script")]
    public class Script
    {
        /// <summary>
        /// 编码
        /// </summary>
        [Column(IsIdentity = true, IsPrimary = true, Name = "Code")]
        public int Guid { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 脚本
        /// </summary>
        public string ScriptContent { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }

}
