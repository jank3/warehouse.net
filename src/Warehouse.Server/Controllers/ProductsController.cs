﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Warehouse.Server.Data;
using Warehouse.Server.Models;

namespace Warehouse.Server.Controllers
{
    public class ProductsController : ApiController
    {
        public IEnumerable<Product> Get()
        {
            var context = new MongoContext();
            var data = context.Products.FindAll();
            return data;
        }

        public HttpResponseMessage Get(string id)
        {
            var context = new MongoContext();
            var data = context.Products.FindOneById(new ObjectId(id));
            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        public HttpResponseMessage Put(string id, [FromBody] Product product)
        {
            var context = new MongoContext();
            var query = Query<Product>.EQ(p => p.Id, new ObjectId(id));
            var update = Update<Product>.Set(p => p.Name, product.Name);
            var res = context.Products.Update(query, update);
            var code = res.Ok ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            return Request.CreateResponse(code);
        }
    }
}
