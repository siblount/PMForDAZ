using System.Collections.Immutable;
using System.IO;

namespace DAZ_Installer.IO
{
    /// <summary>
    /// Represents a file scope settings class that demonstrates whether files and/or directories are allowed to be modified (deleting, modifying)
    /// and copying or deleting files. This class is used to ensure that the <see cref="MoveTo"/> and <see cref="CopyTo"/> operations are only allowed to approved directories. <para/>
    /// This class <b>DOES NOT</b> change ANY OS settings (such as ACLs). It is a software-level protection mechanism.
    /// </summary>
    public class DPFileScopeSettings : IDPFileScopeSettings
    {
        /// <summary>
        /// A file scope settings with no enforcement at all.
        /// </summary>
        public static readonly DPFileScopeSettings All = new(ImmutableList<string>.Empty, ImmutableList<string>.Empty, false, false, false, true);
        /// <summary>
        /// A file scope settings that does not accept anything.
        /// </summary>
        public static readonly DPFileScopeSettings None = CreateUltraStrict(ImmutableList<string>.Empty, ImmutableList<string>.Empty);
        public readonly ImmutableHashSet<string> WhitelistedDirectories;
        public readonly ImmutableHashSet<string> WhitelistedFilePaths;

        public bool ExplicitFilePaths { get; init; } = true;
        public bool ExplicitDirectoryPaths { get; init; } = true;
        public bool NoEnforcement { get; init; } = false;
        public bool ThrowOnPathTransversal { get; init; } = false;

        /// <summary>
        /// Creates a file scope settings with no enforcement.
        /// </summary>
        public DPFileScopeSettings()
        {
            WhitelistedFilePaths = WhitelistedDirectories = ImmutableHashSet<string>.Empty;
            ExplicitDirectoryPaths = ExplicitFilePaths = false;
            NoEnforcement = true;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public DPFileScopeSettings(DPFileScopeSettings other)
        {
            WhitelistedFilePaths = other.WhitelistedFilePaths;
            WhitelistedDirectories = other.WhitelistedDirectories;
            ExplicitDirectoryPaths = other.ExplicitDirectoryPaths;
            ExplicitFilePaths = other.ExplicitFilePaths;
            NoEnforcement = other.NoEnforcement;
            ThrowOnPathTransversal = other.ThrowOnPathTransversal;
        }

        /// <summary>
        /// Creates a file scope settings with the specified whitelisted directories and files. Use this constructor when you already have setup the ImmutableHashSet to your liking.
        /// This is a faster constructor compared to <see cref="DPFileScopeSettings.DPFileScopeSettings(ImmutableHashSet{string}, ImmutableHashSet{string}, bool, bool)"/> because it
        /// needs to normalize all the paths before adding them to the ImmutableHashSet.
        /// </summary>
        /// <param name="filePaths">The file paths to whitelist (optional). All paths should be full and sanitized./>. </param>
        /// <param name="dirs">The directory paths to whitelist (optional).</param>
        /// <param name="strictDirectory">Determines whether to enforce operations must be in <paramref name="dirs"/>. If this is enabled with <paramref name="strictFile"/>, then 
        /// the file path must also be within <paramref name="dirs"/>.</param>
        /// <param name="strictFile">Determines whether to enforce operations must be explictly in <paramref name="filePaths"/>. If this is enabled with <paramref name="strictDirectory"/>,
        /// then the file path must also be within <paramref name="dirs"/>.</param>
        /// <param name="noEnforcement">Determines whether no enforcement should take place. This means that <paramref name="strictDirectory"/> and <paramref name="strictFile"/> will not be honored.</param>
        /// <param name="throwOnPathTransversal">Determines whether to throw a <see cref="PathTransversalException"/> during <see cref="IsDirectoryWhitelisted(string)"/> and <see cref="IsFilePathWhitelisted(string)"/> if a path transversal is detected. 
        /// Does not check in <paramref name="filePaths"/> or <paramref name="dirs"/>.</param>
        public DPFileScopeSettings(ImmutableHashSet<string> filePaths, ImmutableHashSet<string> dirs, bool strictDirectory = true, bool strictFile = false, bool throwOnPathTransversal = false, bool noEnforcement = false)
        {
            WhitelistedFilePaths = filePaths;
            WhitelistedDirectories = dirs;
            ExplicitDirectoryPaths = strictDirectory && !noEnforcement;
            ExplicitFilePaths = strictFile && !noEnforcement;
            NoEnforcement = noEnforcement;
            ThrowOnPathTransversal = throwOnPathTransversal;
        }
        /// <summary>
        /// Creates a file scope settings with the specified whitelisted directories and files and validates them if <paramref name="throwOnPathTransversal"/> is enabled.
        /// </summary>
        /// <param name="filePaths">The file paths to whitelist (optional). Paths will be sanitized.</param>
        /// <param name="dirs">The directory paths to whitelist (optional). Paths will be sanitized."/>.</param>
        /// <param name="strictDirectory">Determines whether to enforce operations must be in <paramref name="dirs"/>. If this is enabled with <paramref name="strictFile"/>, then 
        /// the file path must also be within <paramref name="dirs"/>.</param>
        /// <param name="strictFile">Determines whether to enforce operations must be explictly in <paramref name="filePaths"/>. If this is enabled with <paramref name="strictDirectory"/>,
        /// then the file path must also be within <paramref name="dirs"/>.</param>
        /// <param name="noEnforcement">Determines whether no enforcement should take place. This means that <paramref name="strictDirectory"/> and <paramref name="strictFile"/> will not be honored.</param>
        /// <param name="throwOnPathTransversal">Determines whether to throw a <see cref="PathTransversalException"/> if a path transversal is detected.</param>
        public DPFileScopeSettings(IEnumerable<string> filePaths, IEnumerable<string> dirs, bool strictDirectory = true, bool strictFile = false, bool throwOnPathTransversal = false, bool noEnforcement = false) : 
            this(setupHashset(filePaths), setupHashset(dirs), strictDirectory, strictFile, throwOnPathTransversal, noEnforcement) { }

        /// <summary>
        /// Creates an ultra strict file scope settings with the specified whitelisted directories and files. This is the strictest file scope settings possible.
        /// </summary>
        /// <param name="filePaths">The file paths to whitelist (optional). Paths will be normalized via <see cref="Path.GetFullPath(string)"/>.</param>
        /// <param name="dirs">The file paths to whitelist (optional). Paths will be normalized via <see cref="Path.GetFullPath(string)"/>.</param>
        /// <returns></returns>
        public static DPFileScopeSettings CreateUltraStrict(IEnumerable<string> filePaths, IEnumerable<string> dirs) => new(filePaths, dirs, true, true, true, false);

        /// <summary>
        /// Determines based on the current settings whether the specified directory is whitelisted.
        /// </summary>
        /// <param name="directoryPath">The path to check, will be normalized via <see cref="Path.GetFullPath(string)"/></param>
        /// <returns>Whether the <paramref name="directoryPath"/> is whitelisted or not.</returns>
        /// <exception cref="PathTransversalException">If <see cref="ThrowOnPathTransversal"/> is enabled and a path transversal is detected.</exception>
        /// <inheritdoc cref="Path.GetFullPath(string)"/>
        public bool IsDirectoryWhitelisted(string directoryPath)
        {
            if (ThrowOnPathTransversal) PathTransversalException.ThrowIfTransversalDetected(directoryPath);
            directoryPath = Path.GetFullPath(directoryPath);
            Console.WriteLine($"Directory Path: ${directoryPath}");
            if (NoEnforcement) return true;
            if (ExplicitDirectoryPaths)
                return WhitelistedDirectories.Contains(directoryPath);

            // When neither is enabled, then we just check if the directory is whitelisted.
            if (WhitelistedDirectories.Contains(directoryPath)) return true;
            foreach (var whitelistedDirectory in WhitelistedDirectories)
            {
                if (directoryPath.StartsWith(whitelistedDirectory, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines based on the curernt settings whether the specified file is whitelisted.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Whether the <paramref name="path"/> is whitelisted or not.</returns>
        /// <exception cref="PathTransversalException">If <see cref="ThrowOnPathTransversal"/> is enabled and a path transversal is detected.</exception>
        /// <inheritdoc cref="Path.GetFullPath(string)"/>
        public bool IsFilePathWhitelisted(string path)
        {
            if (ThrowOnPathTransversal) PathTransversalException.ThrowIfTransversalDetected(path);
            path = Path.GetFullPath(path);
            var dirPath = Path.GetDirectoryName(path) ?? string.Empty;
            Console.WriteLine($"Path: {path}, DirPath: {dirPath}");
            if (NoEnforcement) return true;
            if (ExplicitFilePaths && !ExplicitDirectoryPaths)
                return WhitelistedFilePaths.Contains(path);
            if (ExplicitDirectoryPaths && !ExplicitFilePaths)
                return WhitelistedDirectories.Contains(dirPath) || WhitelistedFilePaths.Contains(path);
            if (ExplicitDirectoryPaths && ExplicitDirectoryPaths)
                return WhitelistedFilePaths.Contains(path)
                       && WhitelistedDirectories.Contains(dirPath);

            // When none is enabled, then we check if the directory is whitelisted.
            if (WhitelistedFilePaths.Contains(path) || WhitelistedDirectories.Contains(dirPath)) return true;

            foreach (var whitelistedDirectory in WhitelistedDirectories)
            {
                if (path.StartsWith(whitelistedDirectory, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets up the <see cref="ImmutableHashSet"/> from the specified <see cref="IEnumerable{T}"/> and normalizes the paths via <see cref="Path.GetFullPath(string)"/>.
        /// </summary>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to gather paths from.</param>
        /// <returns>A newly-made <see cref="ImmutableHashSet"/> after processing <paramref name="enumerable"/>.</returns>
        private static ImmutableHashSet<string> setupHashset(IEnumerable<string> enumerable)
        {
            var builder = ImmutableHashSet.CreateBuilder<string>();
            foreach (var str in enumerable)
            {
                builder.Add(Path.GetFullPath(PathHelper.NormalizePath(str)));
            }
            return builder.ToImmutable();
        }
    }
}