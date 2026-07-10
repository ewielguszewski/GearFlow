using GearFlow.Modules.Users.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearFlow.Modules.Users.Core.DAL.Configurations;

internal class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Customer)
            .WithOne(c => c.UserAccount)
            .HasForeignKey<UserAccount>(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(x => x.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
