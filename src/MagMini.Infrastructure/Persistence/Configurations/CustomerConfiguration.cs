using MagMini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagMini.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(250);
        builder.Property(c => c.Nip).HasMaxLength(20);
        builder.Property(c => c.PostalCode).HasMaxLength(10);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(100);
    }
}