using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class DependencyConfiguration : IEntityTypeConfiguration<Dependency>
{
    public void Configure(EntityTypeBuilder<Dependency> builder)
    {
        builder.ToTable("dependencies");

        // Chave primária composta
        builder.HasKey(d => new { d.ParentCommitmentId, d.ChildCommitmentId });

        builder.Property(d => d.ParentCommitmentId).HasColumnName("parent_commitment_id");
        builder.Property(d => d.ChildCommitmentId).HasColumnName("child_commitment_id");

        // Relacionamentos blindados usando o tipo de referência <Commitment>
        builder.HasOne<Commitment>()
            .WithMany()
            .HasForeignKey(d => d.ParentCommitmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Commitment>()
            .WithMany()
            .HasForeignKey(d => d.ChildCommitmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Constraint de integridade
        builder.ToTable(t => t.HasCheckConstraint("chk_no_self_dependency", "parent_commitment_id != child_commitment_id"));

        // Índice para resolução rápida dos filhos no grafo
        builder.HasIndex(d => d.ChildCommitmentId).HasDatabaseName("idx_dependencies_child_lookup");
    }
}