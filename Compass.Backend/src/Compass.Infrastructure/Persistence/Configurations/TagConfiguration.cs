using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("chk_tag_name_length", "char_length(name) >= 2"));

        builder.Property(t => t.ColorHex).HasColumnName("color_hex").HasMaxLength(7).IsRequired().HasDefaultValue("#6366F1");
        builder.ToTable(t => t.HasCheckConstraint("chk_tag_color_hex", "color_hex ~* '^#[a-f0-9]{6}$'"));

        // Índice único composto: o usuário não pode ter duas tags com o mesmo nome
        builder.HasIndex(t => new { t.UserId, t.Name }).IsUnique();
    }
}