using DeviceArchiving.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DeviceArchiving.Data.Contexts;
public class DeviceArchivingContext : DbContext
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<OperationType> OperationsTypes { get; set; }
    public DbSet<User> Users { get; set; }

    public DeviceArchivingContext(DbContextOptions<DeviceArchivingContext> options) : base(options){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Operation>()
                   .HasOne(o => o.User)
                   .WithMany()
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
        modelBuilder.Entity<Device>().HasIndex(u => u.SerialNumber).IsUnique();
        modelBuilder.Entity<Device>().HasIndex(u => u.LaptopName).IsUnique();
    }
}



