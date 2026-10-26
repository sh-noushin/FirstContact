using FirstContact.Domain.Knowledge;

namespace FirstContact.Domain.Tests.Knowledge;

public class KnowledgeArticleTests
{
    [Fact]
    public void It_keeps_the_title_and_content()
    {
        KnowledgeArticle article = KnowledgeArticle.Create("  Dokumente exportieren  ", "  Waehlen Sie Export.  ");

        Assert.Equal("Dokumente exportieren", article.Title);
        Assert.Equal("Waehlen Sie Export.", article.Content);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void An_article_needs_a_title(string? title)
    {
        Assert.Throws<ArgumentException>(() => KnowledgeArticle.Create(title!, "Inhalt"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void An_article_needs_content(string? content)
    {
        Assert.Throws<ArgumentException>(() => KnowledgeArticle.Create("Titel", content!));
    }

    [Fact]
    public void Staff_can_improve_the_text()
    {
        KnowledgeArticle article = KnowledgeArticle.Create("Titel", "Alter Text");

        article.Update("Neuer Text");

        Assert.Equal("Neuer Text", article.Content);
        Assert.Equal("Titel", article.Title);
    }

    [Fact]
    public void An_article_cannot_be_emptied()
    {
        KnowledgeArticle article = KnowledgeArticle.Create("Titel", "Alter Text");

        Assert.Throws<ArgumentException>(() => article.Update("   "));
        Assert.Equal("Alter Text", article.Content);
    }
}
