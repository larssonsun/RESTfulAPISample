﻿using System.Collections.Generic;
using RESTfulAPISample.Core.Interface;

namespace RESTfulAPISample.Core.SortAndQuery
{
    public abstract class PropertyMapping<TSource, TDestination> : IPropertyMapping where TDestination : IEntity
    {
        public Dictionary<string, List<MappedProperty>> MappingDictionary { get; }

        protected PropertyMapping(Dictionary<string, List<MappedProperty>> mappingDictionary)
        {
            MappingDictionary = mappingDictionary;
            MappingDictionary[nameof(IEntity.Id)] = new List<MappedProperty>
            {
                new MappedProperty { Name = nameof(IEntity.Id), Revert = false}
            };
        }
    }
}