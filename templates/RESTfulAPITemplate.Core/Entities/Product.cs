﻿using System;
#if (DISTRIBUTEDCACHE)
using MessagePack;
using RESTfulAPITemplate.Core.SeedWork;
#endif

namespace RESTfulAPITemplate.Core.Entity
{
#if (DISTRIBUTEDCACHE)

    [MessagePackObject(keyAsPropertyName: true)]

#endif



    public class Product : BaseEntity<Guid>
    {
        // public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsOnSale { get; set; }
        public DateTime CreateTime { get; private set; } = DateTime.Now;
    }
}