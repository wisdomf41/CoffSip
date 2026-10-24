
using CoffSip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoffSip.Infrastructure.Identity;

namespace CoffSip.Infrastructure.Data.Configurations;

// Maps MenuItem to PostgreSQL with validation constraints.
public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems", table =>
            table.HasCheckConstraint(
                "CK_MenuItems_Price_Positive",
                "\"Price\" > 0"));

        builder.HasKey(x => x.Id);

        // this will establish the optional operator-to-menu-item relationship.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(item => item.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.OwnerId);

        builder.Property(x => x.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Category)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(x => x.IsAvailable)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.IsArchived)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
