using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Managers
{
    public class WebsiteUpdatedDataFormatter
    {
        public Dictionary<int, List<ProductUpdateData>> Info { get; }
    }
}
