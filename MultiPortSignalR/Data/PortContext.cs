using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiPortSignalR.Data
{
    public class PortContext : DbContext
    {
        public PortContext(DbContextOptions<PortContext> options) : base(options) { }

        public DbSet<AppPort> AppPorts { get; set; }
    }

    [Table("appports")] // optional, helps clarity
    public class AppPort
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("port")]
        public int Port { get; set; }
    }
}
