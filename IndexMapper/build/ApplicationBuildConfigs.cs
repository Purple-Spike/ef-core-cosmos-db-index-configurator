using Cake.Common;
using Cake.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record ProjectPaths(
    string ProjectName,
    string PathToSln,
    string FunctionProjectFolder,
    string CsprojFile,
    string OutDir,
    string UnitTestDirectory,
    string UnitTestProj,
    string CoverletOutDir)

{
    public static ProjectPaths LoadFromContext(ICakeContext context, string buildConfiguration, string srcDirectory)
    {
        var projectName = "IndexMapper";
        var pathToSln = srcDirectory + $"/{projectName}.sln";
        var functionProjectDir = srcDirectory + $"/{projectName}";
        var functionCsprojFile = functionProjectDir + $"/{projectName}.csproj";
        var outDir = functionProjectDir + $"/bin/{buildConfiguration}/cake-build-output";
        var unitTestDirectory = srcDirectory + $"/UnitTests";
        var unitTestProj = unitTestDirectory + $"/UnitTests.csproj";
        var coverletOutDir = unitTestDirectory + $"/coverlet-coverage-results/";

        return new ProjectPaths(
            projectName,
            pathToSln,
            functionProjectDir,
            functionCsprojFile,
            outDir,
            unitTestDirectory,
            unitTestProj,
            coverletOutDir);
    }
};
