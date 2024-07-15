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
using IO = System.IO;
using Cake.Core.IO;
using Build;
using System.Text;
using System.IO;
using Cake.Common.Build.GitHubActions;

#nullable enable

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public const string WINDOWS_PROJECT_FILENAME = "DAZ_Installer.Windows.csproj";
    public const string INSTALLER_ISS_FILENAME = "Windows64.iss";
    public const string VERSION_FILENAME = "VERSION";
    public string BuildConfiguration { get; set; } = "Release";
    public string Platform { get; set; } = "x64";
    public string Version { get; set; } = string.Empty;
    public bool VersionOverriden { get; set; } = false;
    public string VersionSuffix { get; set; } = "Pre-alpha";
    public string BaseSearchPath { get; set; } = string.Empty;
    public new IFileSystem FileSystem { get; set; } = new FileSystem();
    public IGitHubActionsProvider GithubActions { get; set; }
    public IPathFinder PathFinder { get; set; }

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

        context.Environment.WorkingDirectory = context.Environment.WorkingDirectory.GetParent();
        context.Log.Information("Current Working Directory: {0}", context.Environment.WorkingDirectory.FullPath);
        BaseSearchPath = context.Environment.WorkingDirectory.FullPath;
        GithubActions = new CustomGithubActionsProvider(this);
        PathFinder = new PathFinder(BaseSearchPath);
    }
}

[TaskName("UpdateVersion")]
public sealed class UpdateVersionTask : AsyncFrostingTask<BuildContext>
{
    public const string WINDOWS_PROJECT_PATH = "src/DAZ_Installer.Windows/DAZ_Installer.Windows.csproj";
    public const string INSTALLER_ISS_PATH = "src/DAZ_Installer.Installer/Windows64.iss";
    public const string VERSION_FILE_PATH = "src/DAZ_Installer.Windows/VERSION";
    public override async Task RunAsync(BuildContext context)
    {
        if (!context.GithubActions.IsRunningOnGitHubActions)
            throw new System.InvalidOperationException("This task SHOULD be ran in a Github Action ONLY!");

        context.Log.Information("Updating version...");
        var projectPath = context.PathFinder.FindPath(WINDOWS_PROJECT_PATH);
        var installerPath = context.PathFinder.FindPath(INSTALLER_ISS_PATH);
        var versionPath = context.PathFinder.FindPath(VERSION_FILE_PATH);
        
        var projectFileInfo = context.FileSystem.GetFile(projectPath);
        var installerFileInfo = context.FileSystem.GetFile(installerPath);
        var versionFileInfo = context.FileSystem.GetFile(versionPath);

        if (!projectFileInfo.Exists)
            throw new IO.FileNotFoundException("Could not find project file", projectPath);
        if (!installerFileInfo.Exists)
            throw new IO.FileNotFoundException("Could not find installer file", installerPath);
        if (!versionFileInfo.Exists)
            throw new IO.FileNotFoundException("Could not find version file", versionPath);
        if (projectFileInfo.Length > CSProjVersionUpdater.MAX_FILE_SIZE)
            throw new IO.FileLoadException("Project file is too large", projectPath);
        if (installerFileInfo.Length > ISSVersionUpdater.MAX_FILE_SIZE)
            throw new IO.FileLoadException("Installer file is too large", installerPath);
        if (versionFileInfo.Length > VersionVersionUpdater.MAX_FILE_SIZE)
            throw new IO.FileLoadException("Version file is too large", versionPath);

        context.Log.Information("Reading files...");
        var projectContent = ReadAllLines(projectFileInfo);
        var installerContent = ReadAllLines(installerFileInfo);
        var versionContent = ReadAllLines(versionFileInfo);

        context.Log.Information("Getting version...");
        GetVersion(context, versionContent);
        IncrementVersion(context);

        context.Log.Information("Updating related files...");
        var updatedContent = CSProjVersionUpdater.UpdateVersion(projectContent, context.Version, context.VersionSuffix);
        var updatedInstallerContent = ISSVersionUpdater.UpdateVersion(installerContent, context.Version, context.VersionSuffix);
        var updatedVersionContent = VersionVersionUpdater.UpdateVersion(context.Version, context.VersionSuffix);
        
        context.Log.Information("Saving related files...");
        await projectFileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write).WriteAsync(Encoding.UTF8.GetBytes(updatedContent));
        await installerFileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write).WriteAsync(Encoding.UTF8.GetBytes(updatedInstallerContent));
        await versionFileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write).WriteAsync(Encoding.UTF8.GetBytes(updatedVersionContent));

        context.Log.Information("Version updated.");
    }

    private static void GetVersion(BuildContext context, string versionContent)
    {
        var lines = versionContent.Split('\n');
        if (context.VersionOverriden) return;
        if (lines.Length < 2)
            throw new IO.InvalidDataException("Version file is invalid");
        if (string.IsNullOrWhiteSpace(lines[0]) && string.IsNullOrWhiteSpace(lines[1]))
            throw new IO.InvalidDataException("Version file is invalid");
        context.Version = lines[0];
        context.VersionSuffix = lines[1];
    }

    private static void IncrementVersion(BuildContext context)
    {
        if (context.VersionOverriden) return;
        var version = context.Version.Split('.');
        if (version.Length != 3)
            throw new IO.InvalidDataException("Version is invalid");
        if (!int.TryParse(version[2], out int patch))
            throw new IO.InvalidDataException("Version is invalid");
        patch++;
        context.Version = $"{version[0]}.{version[1]}.{patch}";
        context.Log.Information("Incremented version to {0}", context.Version);
    }

    private static string ReadAllLines(IFile file)
    {
        using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

[TaskName("Hello")]
public sealed class HelloTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information("Hello");
        context.GitHubActions().Commands.UploadArtifact("", "");
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