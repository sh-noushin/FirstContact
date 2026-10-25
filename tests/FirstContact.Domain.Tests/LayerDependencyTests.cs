using System.Xml.Linq;

namespace FirstContact.Domain.Tests;

/// <summary>
/// Checks that references point inward only. Reading the .csproj files means an
/// accidental "dotnet add reference" fails here, even before any code uses it.
/// </summary>
public class LayerDependencyTests
{
    [Fact]
    public void Domain_references_no_other_project()
    {
        Assert.Empty(ReferencesOf("FirstContact.Domain"));
    }

    [Fact]
    public void Application_references_only_the_domain()
    {
        Assert.Equal(["FirstContact.Domain"], ReferencesOf("FirstContact.Application"));
    }

    [Fact]
    public void Infrastructure_does_not_reference_the_api()
    {
        Assert.DoesNotContain("FirstContact.Api", ReferencesOf("FirstContact.Infrastructure"));
    }

    /// <summary>The projects one layer declares a reference to.</summary>
    private static List<string> ReferencesOf(string project)
    {
        string file = Path.Combine(SolutionRoot(), "src", project, project + ".csproj");
        XDocument document = XDocument.Load(file);

        return document.Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .OfType<string>()
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .ToList();
    }

    private static string SolutionRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find the solution root.");
    }
}
