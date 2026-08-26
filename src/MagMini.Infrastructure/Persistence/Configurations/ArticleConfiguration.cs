using MagMini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagMini.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Ean).HasMaxLength(20);

        // Precyzja walutowa i magazynowa
        builder.Property(a => a.PurchasePriceNet).HasPrecision(18, 2);
        builder.Property(a => a.DefaultSalePriceNet).HasPrecision(18, 2);
        builder.Property(a => a.StockQuantity).HasPrecision(18, 4);
        builder.Property(a => a.MinStockQuantity).HasPrecision(18, 4);

        builder.HasOne(a => a.Category)
               .WithMany(c => c.Articles)
               .HasForeignKey(a => a.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}