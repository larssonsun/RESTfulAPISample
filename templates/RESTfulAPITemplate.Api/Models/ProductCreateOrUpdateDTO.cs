﻿namespace RESTfulAPITemplate.App.Model
{
    public class ProductCreateOrUpdateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsOnSale { get; set; }
    }
}