using Microsoft.EntityFrameworkCore;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Services
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly DbContextOptions<WebsiteCmsBookingContext> _options;
        private WebsiteCmsBookingContext _dbContext;


        public DbContextFactory(DbContextOptions<WebsiteCmsBookingContext> options)
        {
            _options = options;
        }

        public WebsiteCmsBookingContext CreateDbContext()
        {
            _dbContext = new WebsiteCmsBookingContext(_options);
            return _dbContext;
        }
    }
}
