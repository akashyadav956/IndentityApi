using IdentityAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI.Infrastructure
{
    public class IdentityDBContext: DbContext
    {
        public IdentityDBContext(DbContextOptions<IdentityDBContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
