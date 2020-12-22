using EtkBlazorApp.BL.Data;
using EtkBlazorApp.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{

    public interface IDatabaseProductCorrelator
    {
        Task<List<ProductUpdateData>> GetCorrelationData(List<ProductEntity> products, List<PriceLine> priceLines);
    }


}
