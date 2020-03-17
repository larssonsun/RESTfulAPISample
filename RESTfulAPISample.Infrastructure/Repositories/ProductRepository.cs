﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if (RESTFULAPIHELPER)
using Larsson.RESTfulAPIHelper.Interface;
using Larsson.RESTfulAPIHelper.SortAndQuery;
#endif
using Microsoft.EntityFrameworkCore;
using RESTfulAPISample.Core.DomainModel;
using RESTfulAPISample.Core.DTO;
using RESTfulAPISample.Core.Entity;
using RESTfulAPISample.Core.Interface;

namespace RESTfulAPISample.Infrastructure.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly DemoContext _context;

#if (RESTFULAPIHELPER)

        private readonly IPropertyMappingContainer _propertyMappingContainer;

#endif

#if (RESTFULAPIHELPER)

        public ProductRepository(DemoContext context, IPropertyMappingContainer propertyMappingContainer)
        {
            _context = context;
            _propertyMappingContainer = propertyMappingContainer;
#else

        public ProductRepository(DemoContext context)
        {
            _context = context;

#endif

        }

#if (!OBSOLETESQLSERVER)

        public IAsyncEnumerable<Product> GetProductsEachAsync() =>
            _context.Products.OrderBy(p => p.Name).AsNoTracking().AsAsyncEnumerable();
            
#endif
        public async Task<int> CountNameWithString(string s)
        {
            return await _context.Products.CountAsync(x => x.Name.Contains(s));
        }

        public async Task<(bool hasProduct, Product product)> TryGetProduct(Guid id)
        {
            var result = await _context.Products.AsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();

            return (result != null, result);
        }

        public void AddProduct(Product product)
        {
            product.CreateTime = DateTime.Now;
            _context.Products.Add(product);
        }

        public void DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
        }

        public void UpdateProduct(Product product)
        {
            _context.Update(product);
        }

        public async

#if (RESTFULAPIHELPER)

        Task<PagedListBase<Product>>

#else

        Task<IEnumerable<Product>> 

#endif
        GetProducts(ProductQuery parameters)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Name))
            {
                var name = parameters.Name.Trim().ToLowerInvariant();
                query = query.Where(x => x.Name.ToLowerInvariant() == name);
            }

            if (!string.IsNullOrEmpty(parameters.Description))
            {
                var description = parameters.Description.Trim().ToLowerInvariant();
                query = query.Where(x => x.Description.ToLowerInvariant().Contains(description));
            }

#if (RESTFULAPIHELPER)

            query = query.ApplySort(parameters.OrderBy, _propertyMappingContainer.Resolve<ProductDTO, Product>());

#endif

            var count = await query.CountAsync();
            var data = await query

#if (RESTFULAPIHELPER)

                .Skip(parameters.PageSize * parameters.PageIndex)
                .Take(parameters.PageSize)

#endif

                .AsNoTracking().ToListAsync();

#if (RESTFULAPIHELPER)

            return new PagedListBase<Product>(parameters.PageIndex, parameters.PageSize, count, data);

#else

            return data;

#endif
        }
    }
}