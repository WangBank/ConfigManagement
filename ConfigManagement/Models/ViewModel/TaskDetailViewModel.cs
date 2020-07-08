using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigManagement.Models.ViewModel
{
    
    public class TaskDetailViewModel
    {
        public int Guid { get; set; }

        /// <summary>
        /// 脚本code
        /// </summary>
        public int ScriptCode { get; set; }
        public string ScriptName { get; set; }


        /// <summary>
        /// 账套code
        /// </summary>
        public int OrgCode { get; set; }

        public string OrgName { get; set; }

        /// <summary>
        /// 任务Code
        /// </summary>
        public int TaskCode { get; set; }

        public string TaskName { get; set; }
    }
}
