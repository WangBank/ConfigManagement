using SqlSugar;

namespace ConfigManagement.Models
{
    public class BaseInfo : IInfo
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Guid { get; set; }
    }
}
