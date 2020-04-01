﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RESTfulAPITemplate.Core.DomainModel;
using RESTfulAPITemplate.Core.Entity;

namespace RESTfulAPITemplate.Core.Interface
{
    public interface IProductRepository
    {
        void AddProduct(Product product);

#if (RESTFULAPIHELPER)

        Task<PagedListBase<Product>>

#else

        Task<IEnumerable<Product>>

#endif
        GetProducts(ProductQuery parm);

#if (!OBSOLETESQLSERVER)

        IAsyncEnumerable<Product> GetProductsEachAsync();

#endif

        Task<int> CountNameWithString(string s);
        Task<(bool hasProduct, Product product)> TryGetProduct(Guid id);
        void DeleteProduct(Product product);
        void UpdateProduct(Product product);

    }
}