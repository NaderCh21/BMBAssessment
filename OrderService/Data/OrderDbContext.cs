using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
        { }

        public DbSet<Order> Orders => Set<Order>();
    }
}
