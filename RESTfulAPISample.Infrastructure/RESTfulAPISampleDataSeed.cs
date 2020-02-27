using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RESTfulAPISample.Core.Entity;
#if (MSSQL)
using Microsoft.EntityFrameworkCore;
#endif

namespace RESTfulAPISample.Infrastructure
{
    public class RESTfulAPISampleContextSeed
    {

        private ILogger<RESTfulAPISampleContextSeed> _logger;
        private readonly RESTfulAPISampleContext _context;

        public RESTfulAPISampleContextSeed(ILogger<RESTfulAPISampleContextSeed> logger, RESTfulAPISampleContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task SeedAsync(int retry = 0)
        {
            int retryForAvailability = retry;
            try
            {

#if (MSSQL)

                _context.Database.Migrate();
#endif

                if (!_context.Products.Any())
                {
                    var now = DateTime.Now;
                    _context.Products.AddRange(
                        new Product
                        {
                            Name = "A Learning ASP.NET Core",
                            Description = "C best-selling book covering the fundamentals of ASP.NET Core",
                            IsOnSale = true,
                            CreateTime = now.AddDays(1),
                        },
                        new Product
                        {
                            Name = "D Learning EF Core 2",
                            Description = "A best-selling book covering the fundamentals of C#",
                            IsOnSale = true,
                            CreateTime = now,
                        },
                        new Product
                        {
                            Name = "D Learning EF Core 3",
                            Description = "B best-selling book covering the fundamentals of .NET Standard",
                            CreateTime = now.AddDays(2),
                        },
                        new Product
                        {
                            Name = "C Learning .NET Core",
                            Description = "D best-selling book covering the fundamentals of .NET Core",
                            CreateTime = now.AddDays(13),
                        },
                        new Product
                        {
                            Name = "Learning C#",
                            Description = "A best-selling book covering the fundamentals of C#",
                            CreateTime = now,
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