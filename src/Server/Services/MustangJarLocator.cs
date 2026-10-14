using System.IO.Compression;

namespace Docentric.EInvoice.Validator.RestServer.Services;

/// <summary>
/// Locates the Mustang CLI jar file and reads its version from the jar contents.
/// </summary>
public static class MustangJarLocator
{
    private const string JarSearchPattern = "Mustang-CLI-*.jar";
    private const string PomPropertiesEntry = "META-INF/maven/org.mustangproject/validator/pom.properties";
    private const string VersionKey = "version=";

    /// <summary>
    /// Finds the Mustang CLI jar file name in the given directory without opening the jar.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>
    /// The jar file name, or <see langword="null"/> when the directory or the jar file does not exist.
    /// When several jars match, the last one in ordinal file name order is returned.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="directory"/> is null or whitespace.</exception>
    public static string? FindJarFileName(string directory)
    {
        string? path = FindJarPath(directory);

        return path is null ? null : Path.GetFileName(path);
    }

    /// <summary>
    /// Reads the Mustang CLI version from the embedded Maven <c>pom.properties</c>
    /// of the <c>org.mustangproject:validator</c> artifact inside the jar.
    /// </summary>
    /// <param name="directory">The directory containing the jar.</param>
    /// <returns>
    /// The version, or <see langword="null"/> when the jar is missing, unreadable
    /// or does not contain a valid version.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="directory"/> is null or whitespace.</exception>
    public static string? ReadVersion(string directory)
    {
        string? path = FindJarPath(directory);

        if (path is null)
        {
            return null;
        }

        string? version = ReadVersionFromJar(path);

        return version is not null && Version.TryParse(version, out _) ? version : null;
    }

    private static string? FindJarPath(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        if (!Directory.Exists(directory))
        {
            return null;
        }

        return Directory.EnumerateFiles(directory, JarSearchPattern)
            .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static string? ReadVersionFromJar(string jarPath)
    {
        try
        {
            using ZipArchive archive = ZipFile.OpenRead(jarPath);
            ZipArchiveEntry? entry = archive.GetEntry(PomPropertiesEntry);

            if (entry is null)
            {
                return null;
            }

            using StreamReader reader = new(entry.Open());

            while (reader.ReadLine() is { } line)
            {
                if (line.StartsWith(VersionKey, StringComparison.Ordinal))
                {
                    return line[VersionKey.Length..].Trim();
                }
            }

            return null;
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
