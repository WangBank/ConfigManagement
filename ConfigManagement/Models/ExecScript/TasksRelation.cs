using FreeSql.DataAnnotations;
using System.Collections.Generic;

namespace ConfigManagement.Models
{
    /// <summary>
    /// 任务明细
    /// </summary>
    public class TasksDetailList
    {
        /// <summary>
        /// 明细list
        /// </summary>
        public List<TasksDetail> Items { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int Total { get; set; }

    }

    /// <summary>
    /// 任务明细信息
    /// </summary>
    [Table(Name = "TasksDetail")]
    public class TasksDetail
    {
        /// <summary>
        /// 明细code
        /// </summary>
        [Column(IsIdentity = true, IsPrimary = true, Name = "Code")]
        public int Guid { get; set; }

        /// <summary>
        /// 脚本code
        /// </summary>
        public int ScriptCode { get; set; }

        /// <summary>
        /// 账套code
        /// </summary>
        public int OrgCode { get; set; }


        /// <summary>
        /// 任务Code
        /// </summary>
        public int TaskCode { get; set; }
    }

}
