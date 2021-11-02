using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class WebsiteCurrencyStatusEntity
    {
        public DateTime date_modified { get; init; }
        public string code { get; init; }
        public decimal value_official { get; init; }
    }
}
