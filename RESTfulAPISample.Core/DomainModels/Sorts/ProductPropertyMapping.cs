﻿using System;
using System.Collections.Generic;
using RESTfulAPISample.Core.DTO;
using RESTfulAPISample.Core.Entity;

namespace RESTfulAPISample.Core.DomainModel
{
    public class ProductPropertyMapping : PropertyMapping<ProductDTO, Product>
    {
        public ProductPropertyMapping() :
            base(new Dictionary<string, List<MappedProperty>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(ProductDTO.FullName)] = new List<MappedProperty>
                {
                    new MappedProperty{ Name = nameof(Product.Name), Revert = false},
                    new MappedProperty{ Name = nameof(Product.Description), Revert = false}
                },
                [nameof(ProductDTO.Description)] = new List<MappedProperty>
                {
                    new MappedProperty{ Name = nameof(Product.Description), Revert = false}
                },
                [nameof(ProductDTO.CreateTime)] = new List<MappedProperty>
                {
                    new MappedProperty{ Name = nameof(Product.CreateTime), Revert = true}
                }
            })
        {
        }
    }
}