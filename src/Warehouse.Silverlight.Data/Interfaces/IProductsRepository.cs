﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Warehouse.Silverlight.Infrastructure;
using Warehouse.Silverlight.Models;

namespace Warehouse.Silverlight.Data.Interfaces
{
    public interface IProductsRepository
    {
        Task<AsyncResult<Product[]>> GetAsync();
        Task<AsyncResult<Product>> GetAsync(string id);
        Task<AsyncResult<string>> SaveAsync(Product product);
        Task<AsyncResult> UpdatePrice(ProductPriceUpdate[] prices);
        Task<AsyncResult> Delete(List<string> ids);
        Task<AsyncResult> AttachFile(string productId, string fileId);
        Task<AsyncResult<FileInfo[]>> GetFiles(string productId);
    }
}
