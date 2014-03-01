using System.Web;
using System.Web.Mvc;

namespace Forms_Authentication_Rulz
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
