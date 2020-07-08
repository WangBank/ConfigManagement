using SqlSugar;

namespace ConfigManagement.Common
{
    public interface IDbFactory
    {
        SqlSugarClient GetDbContext(string name);
    }
}
