using EduHub.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Ghi chú: RefreshTokenConfiguration cấu hình mapping/constraint/index cho xoay vòng refresh token và cấp token mới.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// Ghi chú: Configure map xoay vòng refresh token và cấp token mới sang table/column/index/constraint trong EF Core hoặc DI.
    /// </summary>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(token => token.Id);
        builder.Property(token => token.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(token => token.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(token => token.TokenHash).HasColumnName("token_hash").HasMaxLength(512).IsRequired();
        builder.Property(token => token.FamilyId).HasColumnName("family_id").IsRequired();
        builder.Property(token => token.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
        builder.Property(token => token.RevokedAtUtc).HasColumnName("revoked_at_utc");
        builder.Property(token => token.ReplacedByTokenId).HasColumnName("replaced_by_token_id");
        builder.Property(token => token.DeviceId).HasColumnName("device_id").HasMaxLength(256);
        builder.Property(token => token.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(token => token.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(token => token.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_refresh_tokens_token_hash");

        builder.HasIndex(token => new { token.UserId, token.FamilyId })
            .HasDatabaseName("ix_refresh_tokens_user_id_family_id");
    }
}
