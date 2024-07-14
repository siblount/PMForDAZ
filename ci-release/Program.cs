using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.Common.IO;
using Cake.Common.Build;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Test;
using Cake.Common;
using System.IO;
using Cake.Core.IO;
using Build;
using System.Text;

#nullable enable

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }

    public static string FindPath(string env, string path)
    {
        var components = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var result = FindPathRecursive(env, components, 0, 0);
        return result ?? throw new FileNotFoundException($"Could not find path: {path} in {env}");
    }

    private static string? FindPathRecursive(string currentPath, string[] components, int index, int depth)
    {
        if (depth > 6) // Limit recursion depth
        {
            return null;
        }

        if (index == components.Length - 1)
        {
            // Check if it's a file or directory
            var fullPath = Path.Combine(currentPath, components[index]);
            if (File.Exists(fullPath) || Directory.Exists(fullPath))
            {
                return fullPath;
            }
        }

        var searchPattern = $"*{components[index]}*";
        foreach (var dir in Directory.EnumerateDirectories(currentPath, searchPattern, SearchOption.TopDirectoryOnly))
        {
            if (index == components.Length - 1)
            {
                return dir;
            }

            var result = FindPathRecursive(dir, components, index + 1, depth + 1);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}

public class BuildContext : FrostingContext
{
    public const string WINDOWS_PROJECT_FILENAME = "DAZ_Installer.Windows.csproj";
    public const string INSTALLER_ISS_FILENAME = "Windows64.iss";
    public string BuildConfiguration { get; set; } = "Release";
    public string Platform { get; set; } = "x64";
    public string Version { get; set; } = string.Empty;
    public bool VersionOverriden { get; set; } = false;
    public string VersionSuffix { get; set; } = "Pre-alpha";
    public IFileSystem FileSystem { get; set; } = new FileSystem();

    public BuildContext(ICakeContext context): base(context)
    {
        if (context.Arguments.HasArgument("configuration"))
            BuildConfiguration = context.Arguments.GetArgument("configuration");
        if (context.Arguments.HasArgument("platform"))
            Platform = context.Arguments.GetArgument("platform");

        VersionOverriden = context.Arguments.HasArgument("version");
        if (VersionOverriden)
            Version = context.Arguments.GetArgument("version");

        if (context.Arguments.HasArgument("suffix"))
            VersionSuffix = context.Arguments.GetArgument("suffix");
    }
}

[TaskName("UpdateVersion")]
public sealed class UpdateVersionTask : AsyncFrostingTask<BuildContext>
{
    public const string WINDOWS_PROJECT_PATH = "src/DAZ_Installer.Windows/DAZ_Installer.Windows.csproj";
    public const string INSTALLER_ISS_PATH = "src/DAZ_Installer.Installer/Windows64.iss";
    public const string VERSION_FILE_PATH = "src/DAZ_Installer.Windows/VERSION";
    public string BaseSearchPath { get; set; } = string.Empty;
    public override async Task RunAsync(BuildContext context)
    {
        if (!context.GitHubActions().IsRunningOnGitHubActions)
            throw new System.InvalidOperationException("This task SHOULD be ran in a Github Action ONLY!");

        context.Log.Information("Updating version...");
        BaseSearchPath = context.Environment.WorkingDirectory.GetParent().FullPath;
        var projectPath = Program.FindPath(BaseSearchPath, WINDOWS_PROJECT_PATH);
        var installerPath = Program.FindPath(BaseSearchPath, INSTALLER_ISS_PATH);
        var versionPath = Program.FindPath(BaseSearchPath, VERSION_FILE_PATH);
        
        var projectFileInfo = context.FileSystem.GetFile(projectPath);
        var installerFileInfo = context.FileSystem.GetFile(installerPath);
        var versionFileInfo = context.FileSystem.GetFile(versionPath);

        if (!projectFileInfo.Exists)
            throw new FileNotFoundException("Could not find project file", projectPath);
        if (!installerFileInfo.Exists)
            throw new FileNotFoundException("Could not find installer file", installerPath);
        if (!versionFileInfo.Exists)
            throw new FileNotFoundException("Could not find version file", versionPath);
        if (projectFileInfo.Length > CSProjVersionUpdater.MAX_FILE_SIZE)
            throw new FileLoadException("Project file is too large", projectPath);
        if (installerFileInfo.Length > ISSVersionUpdater.MAX_FILE_SIZE)
            throw new FileLoadException("Installer file is too large", installerPath);
        if (versionFileInfo.Length > VersionVersionUpdater.MAX_FILE_SIZE)
            throw new FileLoadException("Version file is too large", versionPath);

        context.Log.Information("Reading files...");
        var projectContent = await File.ReadAllTextAsync(projectPath);
        var installerContent = await File.ReadAllTextAsync(installerPath);
        var versionContent = await File.ReadAllTextAsync(versionPath);

        context.Log.Information("Getting version...");
        GetVersion(context, versionContent);
        IncrementVersion(context);

        context.Log.Information("Updating related files...");
        var updatedContent = CSProjVersionUpdater.UpdateVersion(projectContent, context.Version, context.VersionSuffix);
        var updatedInstallerContent = ISSVersionUpdater.UpdateVersion(installerContent, context.Version, context.VersionSuffix);
        var updatedVersionContent = VersionVersionUpdater.UpdateVersion(context.Version, context.VersionSuffix);
        
        context.Log.Information("Saving related files...");
        await projectFileInfo.OpenWrite().WriteAsync(Encoding.UTF8.GetBytes(updatedContent));
        await installerFileInfo.OpenWrite().WriteAsync(Encoding.UTF8.GetBytes(updatedInstallerContent));
        await versionFileInfo.OpenWrite().WriteAsync(Encoding.UTF8.GetBytes(updatedVersionContent));

        context.Log.Information("Version updated.");
    }

    private static void GetVersion(BuildContext context, string versionContext)
    {
        var lines = versionContext.Split('\n');
        if (lines.Length < 2)
            throw new InvalidDataException("Version file is invalid");
        if (string.IsNullOrWhiteSpace(lines[0]) || string.IsNullOrWhiteSpace(lines[1]))
            throw new InvalidDataException("Version file is invalid");
        if (string.IsNullOrEmpty(context.Version))
            context.Version = lines[0];
        if (string.IsNullOrEmpty(context.VersionSuffix))
            context.VersionSuffix = lines[1];
    }

    private static void IncrementVersion(BuildContext context)
    {
        if (context.VersionOverriden) return;
        var version = context.Version.Split('.');
        if (version.Length != 3)
            throw new InvalidDataException("Version is invalid");
        if (!int.TryParse(version[2], out int patch))
            throw new InvalidDataException("Version is invalid");
        patch++;
        context.Version = $"{version[0]}.{version[1]}.{patch}";
        context.Log.Information("Incremented version to {0}", context.Version);
    }
}

[TaskName("Hello")]
public sealed class HelloTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information("Hello");
    }
}

//[TaskName("World")]
//[IsDependentOn(typeof(HelloTask))]
//public sealed class WorldTask : AsyncFrostingTask<BuildContext>
//{
//    // Tasks can be asynchronous
//    public override async Task RunAsync(BuildContext context)
//    {
//        if (context.Delay)
//        {
//            context.Log.Information("Waiting...");
//            await Task.Delay(1500);
//        }

//        context.Log.Information("World");
//    }
//}

//[TaskName("Default")]
//[IsDependentOn(typeof(WorldTask))]
//public class DefaultTask : FrostingTask
//{
//}