using Microsoft.EntityFrameworkCore;
using Todo.Api.Models;

namespace Todo.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}
