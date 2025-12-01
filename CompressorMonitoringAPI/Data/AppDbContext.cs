using Microsoft.EntityFrameworkCore;
using CompressorMonitoringAPI.Models;

namespace CompressorMonitoringAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<EquipmentSpecification> EquipmentSpecifications { get; set; }
        public DbSet<MonitoringReport> MonitoringReports { get; set; }
        public DbSet<ReportParameter> ReportParameters { get; set; }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Equipment -> EquipmentSpecifications
            modelBuilder.Entity<Equipment>()
                .HasMany(e => e.Specifications)
                .WithOne(es => es.Equipment)
                .HasForeignKey(es => es.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Equipment -> MonitoringReports
            modelBuilder.Entity<Equipment>()
                .HasMany(e => e.Reports)
                .WithOne(mr => mr.Equipment)
                .HasForeignKey(mr => mr.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // MonitoringReports -> ReportParameters
            modelBuilder.Entity<MonitoringReport>()
                .HasMany(mr => mr.Parameters)
                .WithOne(rp => rp.Report)
                .HasForeignKey(rp => rp.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Индексы
            modelBuilder.Entity<MonitoringReport>()
                .HasIndex(mr => new { mr.EquipmentId, mr.ReportDate });
        }
    }
}