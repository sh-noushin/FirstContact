using FirstContact.Application.Knowledge;
using FirstContact.Application.Tests.Fakes;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Tests.Knowledge;

/// <summary>
/// Writing an article is what closes the loop from a knowledge gap back to an answer,
/// so adding and improving must both work without creating competing copies.
/// </summary>
public class WriteArticleTests
{
    private readonly InMemoryArticleRepository _articles = new();

    [Fact]
    public async Task A_new_article_is_added_and_embedded()
    {
        KnowledgeArticle article = await Write("SAP-Schnittstelle", "Es gibt eine REST-Schnittstelle.");

        Assert.Equal("SAP-Schnittstelle", article.Title);
        Assert.Single(_articles.All);
        Assert.Equal(1, _articles.Embeddings);
    }

    [Fact]
    public async Task Writing_the_same_title_again_improves_the_article_instead_of_copying_it()
    {
        await Write("SAP-Schnittstelle", "Alter Text");

        KnowledgeArticle article = await Write("SAP-Schnittstelle", "Besserer Text");

        Assert.Single(_articles.All);
        Assert.Equal("Besserer Text", article.Content);
    }

    [Fact]
    public async Task An_improved_article_is_embedded_again_so_search_sees_the_new_text()
    {
        await Write("SAP-Schnittstelle", "Alter Text");
        await Write("SAP-Schnittstelle", "Besserer Text");

        Assert.Equal(2, _articles.Embeddings);
    }

    [Fact]
    public async Task The_title_is_matched_ignoring_case_and_spacing()
    {
        await Write("SAP-Schnittstelle", "Alter Text");

        await Write("  sap-schnittstelle  ", "Besserer Text");

        Assert.Single(_articles.All);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task An_article_without_a_title_is_rejected(string? title)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Write(title!, "Inhalt"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task An_article_without_content_is_rejected(string? content)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Write("Titel", content!));
    }

    [Fact]
    public async Task Articles_are_listed_by_title()
    {
        await Write("Zertifikat", "Inhalt");
        await Write("Aufbewahrung", "Inhalt");

        IReadOnlyList<KnowledgeArticle> listed = await _articles.ListAsync(CancellationToken.None);

        Assert.Equal(["Aufbewahrung", "Zertifikat"], listed.Select(article => article.Title));
    }

    private Task<KnowledgeArticle> Write(string title, string content) =>
        new WriteArticle(_articles).HandleAsync(title, content, CancellationToken.None);
}
