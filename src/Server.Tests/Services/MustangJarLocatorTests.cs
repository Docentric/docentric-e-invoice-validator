using System.IO.Compression;

using Docentric.EInvoice.Validator.RestServer.Services;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

/// <summary>
/// Tests how MustangJarLocator finds the Mustang CLI jar and reads its version.
/// </summary>
public class MustangJarLocatorTests
{
    private const string PomPath = "META-INF/maven/org.mustangproject/validator/pom.properties";

    private static string CreateDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), "mustang-jar-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void CreateJar(string directory, string fileName, string? entryPath, string entryContent)
    {
        using ZipArchive archive = ZipFile.Open(Path.Combine(directory, fileName), ZipArchiveMode.Create);

        if (entryPath is null)
        {
            archive.CreateEntry("placeholder.txt");
            return;
        }

        ZipArchiveEntry entry = archive.CreateEntry(entryPath);
        using StreamWriter writer = new(entry.Open());
        writer.Write(entryContent);
    }

    private static string Pom(string version) => $"artifactId=validator\r\ngroupId=org.mustangproject\r\nversion={version}\r\n";

    /// <summary>
    /// Verifies that the file name of an existing jar is returned.
    /// </summary>
    [Fact]
    public void WhenJarExistsThenFindJarFileNameReturnsFileName()
    {
        string directory = CreateDirectory();
        CreateJar(directory, "Mustang-CLI-2.26.0.jar", PomPath, Pom("2.26.0"));

        Assert.Equal("Mustang-CLI-2.26.0.jar", MustangJarLocator.FindJarFileName(directory));
    }

    /// <summary>
    /// Verifies that the jar file name is returned even when the file is not a valid archive.
    /// </summary>
    [Fact]
    public void WhenJarIsNotAValidArchiveThenFindJarFileNameStillReturnsFileName()
    {
        string directory = CreateDirectory();
        File.WriteAllText(Path.Combine(directory, "Mustang-CLI-2.26.0.jar"), "not a zip");

        Assert.Equal("Mustang-CLI-2.26.0.jar", MustangJarLocator.FindJarFileName(directory));
    }

    /// <summary>
    /// Verifies that the last jar by ordinal file name is returned when several jars exist.
    /// </summary>
    [Fact]
    public void WhenMultipleJarsExistThenFindJarFileNameReturnsLastByOrdinalName()
    {
        string directory = CreateDirectory();
        CreateJar(directory, "Mustang-CLI-a.jar", PomPath, Pom("2.9.0"));
        CreateJar(directory, "Mustang-CLI-b.jar", PomPath, Pom("2.26.0"));

        Assert.Equal("Mustang-CLI-b.jar", MustangJarLocator.FindJarFileName(directory));
    }

    /// <summary>
    /// Verifies that null is returned when the directory contains no jar.
    /// </summary>
    [Fact]
    public void WhenNoJarExistsThenFindJarFileNameReturnsNull()
    {
        string directory = CreateDirectory();
        CreateJar(directory, "other.jar", PomPath, Pom("2.26.0"));

        Assert.Null(MustangJarLocator.FindJarFileName(directory));
    }

    /// <summary>
    /// Verifies that null is returned when the directory does not exist.
    /// </summary>
    [Fact]
    public void WhenDirectoryDoesNotExistThenFindJarFileNameReturnsNull()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Assert.Null(MustangJarLocator.FindJarFileName(directory));
    }

    /// <summary>
    /// Verifies that a whitespace directory is rejected when finding the jar.
    /// </summary>
    [Fact]
    public void WhenDirectoryIsWhitespaceThenFindJarFileNameThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MustangJarLocator.FindJarFileName(" "));
    }

    /// <summary>
    /// Verifies that the version is read from pom.properties inside the jar.
    /// </summary>
    [Fact]
    public void WhenJarContainsPomPropertiesThenReadVersionReturnsJarVersion()
    {
        string directory = CreateDirectory();
        CreateJar(directory, "Mustang-CLI-9.9.9.jar", PomPath, Pom("2.26.0"));

        Assert.Equal("2.26.0", MustangJarLocator.ReadVersion(directory));
    }

    /// <summary>
    /// Verifies that null is returned when the jar has no pom.properties.
    /// </summary>
    [Fact]
    public void WhenJarHasNoPomPropertiesThenReadVersionReturnsNull()
    {
        string directory = CreateDirectory();
        CreateJar(directory, "Mustang-CLI-2.26.0.jar", null, string.Empty);

        Assert.Null(MustangJarLocator.ReadVersion(directory));
    }

    /// <summary>
    /// Verifies that null is returned when pom.properties has no valid version.
    /// </summary>
    [Fact]
    public void WhenPomPropertiesHasNoValidVersionThenReadVersionReturnsNull()
    {
        string directory = CreateDirectory();
        CreateJar(directory, "Mustang-CLI-2.26.0.jar", PomPath, Pom("latest"));

        Assert.Null(MustangJarLocator.ReadVersion(directory));
    }

    /// <summary>
    /// Verifies that null is returned when the jar is not a valid archive.
    /// </summary>
    [Fact]
    public void WhenJarIsNotAValidArchiveThenReadVersionReturnsNull()
    {
        string directory = CreateDirectory();
        File.WriteAllText(Path.Combine(directory, "Mustang-CLI-2.26.0.jar"), "not a zip");

        Assert.Null(MustangJarLocator.ReadVersion(directory));
    }

    /// <summary>
    /// Verifies that null is returned when the directory contains no jar.
    /// </summary>
    [Fact]
    public void WhenNoJarExistsThenReadVersionReturnsNull()
    {
        string directory = CreateDirectory();

        Assert.Null(MustangJarLocator.ReadVersion(directory));
    }

    /// <summary>
    /// Verifies that a whitespace directory is rejected when reading the version.
    /// </summary>
    [Fact]
    public void WhenDirectoryIsWhitespaceThenReadVersionThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MustangJarLocator.ReadVersion(" "));
    }
}
