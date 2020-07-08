using FreeSql.DataAnnotations;
using System.Collections.Generic;

namespace ConfigManagement.Models
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TasksList
    {
        /// <summary>
        /// 任务list
        /// </summary>
        public List<Tasks> Items { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int Total { get; set; }

    }

    /// <summary>
    /// 任务
    /// </summary>
    [Table(Name = "Tasks")]
    public class Tasks
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
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }

}
