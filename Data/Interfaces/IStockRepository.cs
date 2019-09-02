using CoreBot.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CoreBot.Data.Interfaces
{
    public interface IStockRepository
    {
        Task<List<Stock>> FindStock();
        Task<List<Stock>> FindStockByFilters(string color, string garment);
        Task<Stock> FindStockBySkuCode(string skuCode);
    }
}
