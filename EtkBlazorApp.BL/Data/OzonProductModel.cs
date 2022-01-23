using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class OzonProductListRootObjectModel
    {
        public OzonProductListModel result { get; set; }
    }

    public class OzonProductListModel
    {
        public OzonProductModel[] items { get; set; }
        public int total { get; set; }
    }

    public class OzonProductModel
    {
        public int product_id { get; set; }
        public string offer_id { get; set; }
    }
}
