using Microsoft.EntityFrameworkCore;
using todo.api.Models;

namespace todo.api.Data
{
    public class TodoContext : DbContext
    {
        public DbSet<Todo> Todos { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<TodoOwner> TodoOwners { get; set; }

      

        public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Todo>()
                .HasMany(p => p.Owners)
                .WithOne(c => c.Todo)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
