using Fcg.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Users.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<UserPersistenceModel>
{
    public const int PasswordHashMaxLength = 100;
    public const int RoleMaxLength = 20;

    public void Configure(EntityTypeBuilder<UserPersistenceModel> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();

        builder.Property(user => user.Name).IsRequired().HasMaxLength(User.NameMaxLength);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(Email.MaxLength);
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(PasswordHashMaxLength);
        builder.Property(user => user.Role).IsRequired().HasConversion<string>().HasMaxLength(RoleMaxLength);
        builder.Property(user => user.Active).IsRequired();
        builder.Property(user => user.CreatedAt).IsRequired();
    }
}
