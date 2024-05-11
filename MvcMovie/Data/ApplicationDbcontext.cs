using MvcMovie.Models;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Data
{
    public class ApplicationDbcontext : DbContext
     {
        public ApplicationDbcontext(DbContextOptions<ApplicationDbcontext> options) : base(options)
        {}
        //khai bao anh xa vao database
        public DbSet<Person> Person { get; set;}
     }
     }
