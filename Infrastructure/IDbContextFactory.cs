using Microsoft.EntityFrameworkCore;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Infrastructure
{
    public interface IDbContextFactory
    {
        WebsiteCmsBookingContext CreateDbContext();
    }
}
