using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigManagement.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("RedisDBSetting")]
    public partial class RedisDBSetting
    {
        public RedisDBSetting()
        {


        }
        /// <summary>
        /// Desc_New:
        /// Default_New:
        /// Nullable_New:False
        /// </summary>           
        [SugarColumn(Length = 50, IsPrimaryKey = true)]
        public string Code { get; set; }

        /// <summary>
        /// Desc_New:
        /// Default_New:
        /// Nullable_New:True
        /// </summary> 
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Name { get; set; }

        /// <summary>
        /// Desc_New:
        /// Default_New:
        /// Nullable_New:True
        /// </summary>    
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ServerIp { get; set; }

        /// <summary>
        /// Desc_New:
        /// Default_New:
        /// Nullable_New:True
        /// </summary>       
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Password { get; set; }

    }
}
