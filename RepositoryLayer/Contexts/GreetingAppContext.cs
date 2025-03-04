using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Entity;
using Microsoft.EntityFrameworkCore;


namespace RepositoryLayer.Contexts
{
    public class GreetingAppContext : DbContext

    {

        public GreetingAppContext(DbContextOptions<GreetingAppContext> options) : base(options) { }

        public virtual DbSet<GreetEntity> Greet { get; set; }
    }

}