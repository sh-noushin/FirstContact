using FirstContact.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>How a handbook article is stored, including its embedding.</summary>
public class KnowledgeArticleConfiguration : IEntityTypeConfiguration<KnowledgeArticle>
{
    /// <summary>The column holding the embedding. Named here so the search can read it.</summary>
    public const string EmbeddingColumn = "Embedding";

    public void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(article => article.Id);

        builder.Property(article => article.Title).IsRequired().HasMaxLength(200);
        builder.Property(article => article.Content).IsRequired();

        builder.HasIndex(article => article.Title).IsUnique();

        // A shadow property: the embedding lives in the database but not on the domain
        // class, so the rules stay free of anything to do with vectors.
        builder.Property<byte[]>(EmbeddingColumn);
    }
}
