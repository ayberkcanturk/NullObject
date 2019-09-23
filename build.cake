#tool "nuget:?package=xunit.runner.console&version=2.3.1"
#addin "nuget:?package=NuGet.Core&version=2.14.0"
#addin nuget:?package=Cake.ArgumentHelpers
#addin "Cake.ExtendedNuGet"

using Cake.ExtendedNuGet;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var projectName = "NullObject";
var solutionPath = "./" + projectName + ".sln";

var branch = Argument("branch", EnvironmentVariable("APPVEYOR_REPO_BRANCH"));
var isRelease = EnvironmentVariable("APPVEYOR_REPO_TAG") == "true";

var nugetSource = "https://www.nuget.org/api/v2/package";
var nugetApiKey = EnvironmentVariable("nugetApiKey");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/NullObject/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionPath);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild(solutionPath, settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild(solutionPath, settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
	XUnit2("./test/**/bin/Release/*.Tests.dll",
     new XUnit2Settings {
        Parallelism = ParallelismOption.All,
        HtmlReport = true,
        NoAppDomain = true,
        OutputDirectory = "./build"
    });
});

Task("Pack")
	.WithCriteria(() => branch == "master" ) 
	.IsDependentOn("Tests")
	.Does(() =>
	{
		Information("Publish Libraries!");

		var code = 0;
		code = StartProcess("dotnet", "pack " + projectName + " -c Release -o ../artifacts");
		if(code != 0)
		{
			Error($"dotnet pack failed with code {code}");
		}
	});

Task("Deploy")
	.WithCriteria(() => branch == "master" )
	.IsDependentOn("Pack")
	.Does(() =>
	{
		Information("Deploy Packages!");
		var settings = new NuGetPushSettings {
			Source = nugetSource,
			ApiKey = nugetApiKey
		};

		var files = GetFiles("./artifacts/**/*.nupkg");
		foreach(var file in files)
		{
			Information("Push package: {0}", file);
			NuGetPush(file, settings);
		}
	});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
	.IsDependentOn("Tests")
    .IsDependentOn("Pack")
    .IsDependentOn("Deploy");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
