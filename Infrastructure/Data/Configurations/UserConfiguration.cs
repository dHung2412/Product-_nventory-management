// Infrastructure/Data/Configurations/UserConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name
            builder.ToTable("Users");

            // Primary key
            builder.HasKey(u => u.Id);

            // Properties configuration
            builder.Property(u => u.Id)
                .ValueGeneratedNever(); // Guid is generated in domain

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(u => u.Username)
                .IsUnique();
            
            builder.HasIndex(u => u.Email)
                .IsUnique();
            
            builder.HasIndex(u => u.Role);
            builder.HasIndex(u => u.IsActive);

            // Relationships
            builder.HasMany(u => u.StockTransactions)
                .WithOne(st => st.User)
                .HasForeignKey(st => st.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if transactions exist
        }
    }
}