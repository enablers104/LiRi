using Dapper;
using CoreBot.Data.Interfaces;
using CoreBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CoreBot.Data
{
    public class StockRepository : BaseRepository, IStockRepository
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="StockRepository" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public StockRepository(IConfiguration configuration) : base(configuration["LiriSQLConnection"])
        {
        }

        /// <summary>
        /// Finds the stock.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Stock>> FindStock()
        {
            return await ExecWithConnAsync(async db => {
                var resultList = await db.QueryAsync<Stock>($"SELECT * FROM dbo.tStock", commandType: System.Data.CommandType.Text).ConfigureAwait(false);
                return resultList.ToList();
            });
        }

        public async Task<List<Stock>> FindStockByFilters(string color, string garment)
        {
            string sql = $"SELECT * FROM dbo.tStock WHERE Color = '{color}' AND Garment = '{garment}'";
            return await ExecWithConnAsync(async db => {
                var resultList = await db.QueryAsync<Stock>(sql, commandType: System.Data.CommandType.Text).ConfigureAwait(false);
                return resultList.ToList();
            });
        }
    }
}
