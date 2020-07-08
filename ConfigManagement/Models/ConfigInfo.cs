using SqlSugar;

namespace ConfigManagement.Models
{
    [SugarTable("ConfigInfo")]
    public class ConfigInfo : BaseInfo
    {
        /// <summary>
        /// 配置文件名
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public string ConfigPath { get; set; }

        /// <summary>
        /// 本地路径
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// 配置文件类别
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string CategoryType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string CreateDate { get; set; }

        /// <summary>
        /// 是否是系统文件 0否1是
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string IsSystem { get; set; }

        /// <summary>
        /// 更新页面地址
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string UpdateUrl { get; set; }
    }
}
