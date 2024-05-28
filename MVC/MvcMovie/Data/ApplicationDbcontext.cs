using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;
namespace DemoMVC.Data
{
   // khai báo việc ánh xạ class Person vào trong database
 
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {}
        public DbSet<Person> Person { get; set;}
        public object Employee { get; internal set; }
        //public DbSet<DemoMVC.Models.Employee> Employee { get; set; } = default!;
    }
}