using System;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Restore;
using System.Threading;
using System.Diagnostics;
using Cake.Common.Tools.DotNet.Test;
using Cake.Common.Tools.DotNet.Build;

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
    public string Target { get; }
    public string BuildConfiguration { get; }
    public string SrcDirectoryPath { get; }
    public string NugetVersion { get; }
    public ProjectPaths ProjectPaths { get; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        Target = context.Argument("target", "Default");
        BuildConfiguration = context.Argument<string>("configuration", "Release");
        SrcDirectoryPath = context.Argument("srcDirectoryPath", $"C:/GitHub/Purple-Spike/ef-core-cosmos-db-index-configurator/EfCoreCosmosDbIndexConfigurationLib/src");
        NugetVersion = context.Argument("nugetVersion", "0.0.1-local");

        ProjectPaths = ProjectPaths.LoadFromContext(context, BuildConfiguration, SrcDirectoryPath);
    }
}

[TaskName(nameof(OutputParametersTask))]
public sealed class OutputParametersTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information($"INFO: Current Working Directory: {context.Environment.WorkingDirectory}");

        context.Log.Information($"INFO: {nameof(context.BuildConfiguration)}: {context.BuildConfiguration}");
        context.Log.Information($"INFO: {nameof(context.SrcDirectoryPath)}: {context.SrcDirectoryPath}");
        context.Log.Information($"INFO: {nameof(context.ProjectPaths)}.{nameof(context.ProjectPaths.ProjectName)}: {context.ProjectPaths.ProjectName}");
    }
}

[IsDependentOn(typeof(OutputParametersTask))]
[TaskName(nameof(BuildTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.CleanDirectory(context.ProjectPaths.OutDir);

        BuildDotnetApp(context, context.ProjectPaths.PathToSln);
        PublishDotnetApp(context, context.ProjectPaths.OutDir, context.ProjectPaths.CsprojFile);
    }

    private void BuildDotnetApp(BuildContext context, string pathToSln)
    {
        context.DotNetRestore(pathToSln, new DotNetRestoreSettings { });

        context.DotNetBuild(pathToSln, new DotNetBuildSettings
        {
            NoRestore = true,
            Configuration = context.BuildConfiguration
        });
    }

    private void PublishDotnetApp(BuildContext context, string outDir, string csprojFile)
    {
        context.DotNetPack(csprojFile, new Cake.Common.Tools.DotNet.Pack.DotNetPackSettings
        {
            IncludeSymbols = false,
            IncludeSource = true,
            NoBuild = true,
            Configuration = context.BuildConfiguration,
            OutputDirectory = outDir,
            ArgumentCustomization = (args) => args.Append($"-p:PackageVersion={context.NugetVersion}")
        });
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildTask))]
public class DefaultTask : FrostingTask
{
}
