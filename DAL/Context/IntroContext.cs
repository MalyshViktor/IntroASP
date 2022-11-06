using Microsoft.EntityFrameworkCore;
using System;

namespace Intro.DAL.Context
{
    public class IntroContext : DbContext
    {
        public DbSet<Entities.User> Users { get; set; }
        public DbSet<Entities.Topic> Topics { get; set; }
        public DbSet<Entities.Article> Articles { get; set; }

        public IntroContext(DbContextOptions options) 
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Этот метод вызывается когда создается модель - 
            //  БД из кода. Здесь можно задать начальные настройки
            modelBuilder.ApplyConfiguration(new UsersConfiguration());
            /*
            modelBuilder
                .Entity<Entities.Topic>()
                .HasMany(t => t.Articles)
                .WithOne(a => a.Topic);
            */
        }
    }
}
