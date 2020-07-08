using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigManagement.Common.Page
{
    public static class PageHelper
    {

        public static List<T> GetPageData<T>(List<T> data, int page, int limit)
        {
            if (page > 0 && limit > 0)
            {
                data = data.Skip((page - 1) * limit).Take(limit).ToList();
            }
            return data;
        }
    }
}
