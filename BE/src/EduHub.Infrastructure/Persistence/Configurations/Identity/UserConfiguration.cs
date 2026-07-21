using EduHub.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Ghi chú: UserConfiguration cấu hình mapping/constraint/index cho tài khoản người dùng đăng nhập.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Ghi chú: Configure map tài khoản người dùng đăng nhập sang table/column/index/constraint trong EF Core hoặc DI.
    /// </summary>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(user => user.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(320).IsRequired();
        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
        builder.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(256).IsRequired();
        builder.Property(user => user.ReferenceCode).HasColumnName("reference_code").HasMaxLength(64);
        builder.Property(user => user.PhoneNumber).HasColumnName("phone_number").HasMaxLength(32);
        builder.Property(user => user.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(user => user.SecurityStamp).HasColumnName("security_stamp").IsRequired();
        builder.Property(user => user.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(user => user.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("ux_users_normalized_email");

        builder.HasIndex(user => user.ReferenceCode)
            .HasDatabaseName("ix_users_reference_code");

        builder.HasMany(user => user.RefreshTokens)
            .WithOne(token => token.User)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
