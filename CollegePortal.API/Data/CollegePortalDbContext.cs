using Microsoft.EntityFrameworkCore;
using CollegePortal.API.Models;

namespace CollegePortal.API.Data
{
    public class CollegePortalDbContext : DbContext
    {
        public CollegePortalDbContext(DbContextOptions<CollegePortalDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<FacultyPersonal> Faculty { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Marks> Marks { get; set; }
        public DbSet<FeeStructure> FeeStructures { get; set; }
        public DbSet<FeePayment> FeePayments { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Student ──
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.RollNumber).IsUnique();
            });

            // ── FacultyPersonal ──
            modelBuilder.Entity<FacultyPersonal>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // ── Admin ──
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // ── Subject ──
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasIndex(e => e.SubjectCode).IsUnique();
            });

            // ── Course ──
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasOne(c => c.Faculty)
                      .WithMany(f => f.Courses)
                      .HasForeignKey(c => c.FacultyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Subject)
                      .WithMany(s => s.Courses)
                      .HasForeignKey(c => c.SubjectId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Enrollment ──
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Enrollments)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Subject)
                      .WithMany(s => s.Enrollments)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Attendance ──
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasOne(a => a.Student)
                      .WithMany(s => s.Attendances)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Course)
                      .WithMany(c => c.Attendances)
                      .HasForeignKey(a => a.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.MarkedByFaculty)
                      .WithMany(f => f.MarkedAttendances)
                      .HasForeignKey(a => a.MarkedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Marks ──
            modelBuilder.Entity<Marks>(entity =>
            {
                entity.HasOne(m => m.Student)
                      .WithMany(s => s.Marks)
                      .HasForeignKey(m => m.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Subject)
                      .WithMany(s => s.Marks)
                      .HasForeignKey(m => m.SubjectId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Course)
                      .WithMany(c => c.Marks)
                      .HasForeignKey(m => m.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── FeePayment ──
            modelBuilder.Entity<FeePayment>(entity =>
            {
                entity.HasIndex(e => e.ReceiptNumber).IsUnique();

                entity.HasOne(fp => fp.Student)
                      .WithMany(s => s.FeePayments)
                      .HasForeignKey(fp => fp.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(fp => fp.FeeStructure)
                      .WithMany(fs => fs.FeePayments)
                      .HasForeignKey(fp => fp.FeeStructureId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Announcement ──
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasOne(a => a.Admin)
                      .WithMany(ad => ad.Announcements)
                      .HasForeignKey(a => a.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
