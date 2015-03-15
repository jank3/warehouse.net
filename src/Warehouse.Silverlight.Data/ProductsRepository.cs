﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Warehouse.Silverlight.Auth;
using Warehouse.Silverlight.Data.Http;
using Warehouse.Silverlight.Data.Interfaces;
using Warehouse.Silverlight.Infrastructure;
using Warehouse.Silverlight.Models;

namespace Warehouse.Silverlight.Data
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly IAuthStore authStore;

        public ProductsRepository(IAuthStore authStore)
        {
            this.authStore = authStore;
        }

        public async Task<AsyncResult<Product[]>> GetAsync()
        {
            var token = authStore.LoadToken();
            using (var client = new BearerHttpClient(token.AccessToken))
            {
                var str = await client.GetStringAsync(new Uri("api/products", UriKind.Relative));
                var res = JsonConvert.DeserializeObject<Product[]>(str);
                return new AsyncResult<Product[]> { Result = res, Succeed = true };
            }
        }

        public async Task<AsyncResult<Product>> GetAsync(string id)
        {
            var token = authStore.LoadToken();
            using (var client = new BearerHttpClient(token.AccessToken))
            {
                var uri = new Uri(string.Concat("api/products/", id), UriKind.Relative);
                var str = await client.GetStringAsync(uri);
                var res = JsonConvert.DeserializeObject<Product>(str);
                return new AsyncResult<Product> { Result = res, Succeed = true };
            }
        }

        public async Task<AsyncResult> UpdatePrice(ProductPriceUpdate[] prices)
        {
            var succeed = false;
            var token = authStore.LoadToken();
            using (var client = new BearerHttpClient(token.AccessToken))
            {
                var data = JsonConvert.SerializeObject(prices);
                using (var content = new StringContent(data, Encoding.UTF8, "application/json"))
                {
                    var uri = new Uri("api/products/updatePrice", UriKind.Relative);
                    var resp = await client.PutAsync(uri, content);
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        succeed = true;
                    }
                }
            }
            return new AsyncResult { Succeed = succeed };
        }

        public async Task<AsyncResult> Delete(List<string> ids)
        {
            var succeed = false;
            var token = authStore.LoadToken();
            using (var client = new BearerHttpClient(token.AccessToken))
            {
                var q = string.Join(",", ids);
                var uriString = string.Concat("api/products?ids=", q);
                var uri = new Uri(uriString, UriKind.Relative);
                var resp = await client.DeleteAsync(uri);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    succeed = true;
                }
            }
            return new AsyncResult { Succeed = succeed };
        }
    }
}
