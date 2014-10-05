﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Warehouse.Silverlight.DataService.Infrastructure;
using Warehouse.Silverlight.Models;

namespace Warehouse.Silverlight.DataService
{
    public class DataService : IDataService
    {
        public async Task<AsyncResult<Product[]>> GetProductsAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = System.Windows.Browser.HtmlPage.Document.DocumentUri;
                var str = await client.GetStringAsync(new Uri("api/products", UriKind.Relative));
                var res = JsonConvert.DeserializeObject<Product[]>(str);
                return new AsyncResult<Product[]> { Result = res, Success = true };
            }
        }
    }
}
