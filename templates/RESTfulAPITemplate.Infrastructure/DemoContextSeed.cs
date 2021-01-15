﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RESTfulAPITemplate.Core.Entity;

namespace RESTfulAPITemplate.Infrastructure
{
    public class DemoContextSeed
    {
        private ILogger<DemoContextSeed> _logger;
        private readonly ProductContext _context;

        public DemoContextSeed(ILogger<DemoContextSeed> logger, ProductContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task SeedAsync(int retry = 0)
        {
            int retryForAvailability = retry;
            try
            {
                if (!_context.Products.Any())
                {
                    var now = DateTime.Now;
                    _context.Products.AddRange(
                        new Product
                        {
                            Name = "A Learning ASP.NET Core",
                            Description = "C best-selling book covering the fundamentals of ASP.NET Core",
                            IsOnSale = true,
                        },
                        new Product
                        {
                            Name = "D Learning EF Core 2",
                            Description = "A best-selling book covering the fundamentals of C#",
                            IsOnSale = true,
                        },
                        new Product
                        {
                            Name = "D Learning EF Core 3",
                            Description = "B best-selling book covering the fundamentals of .NET Standard",
                        },
                        new Product
                        {
                            Name = "C Learning .NET Core",
                            Description = "D best-selling book covering the fundamentals of .NET Core",
                        },
                        new Product
                        {
                            Name = "Learning C#",
                            Description = "A best-selling book covering the fundamentals of C#",
                        });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Seed data created.");
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability < 10)
                {
                    retryForAvailability++;
                    _logger.LogError(ex.Message);
                    await SeedAsync(retryForAvailability);
                }
            }
        }
    }

}