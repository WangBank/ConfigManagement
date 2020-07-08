using SqlSugar;

namespace ConfigManagement.Models
{
    [SugarTable("BusinessCategory")]
    public class BusinessCategory : BaseInfo
    {
        /// <summary>
        /// 类别名
        /// </summary>
        public string BusinessName { get; set; }
    }
}
