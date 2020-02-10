var target = Argument("target", "Default");

Task("Restore")
	.Does(() =>
{
	var solutions = GetFiles("./*.sln");
	// Restore all NuGet packages.
	foreach(var solution in solutions)
	{
		Information("Restoring {0}", solution);
		NuGetRestore(solution);
	}
});

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
{
  MSBuild("./AWS_KCL_DotNet.sln", new MSBuildSettings {
    Verbosity = Verbosity.Minimal,
    Configuration = "Release",
    PlatformTarget = PlatformTarget.x86
    });
});


Task("Bootstrap")
	.Does(()=>{
		CleanDirectories("./jars");
		var boostrap = MakeAbsolute(File("./Bootstrap/bin/Release/netcoreapp2.0/Bootstrap.dll"));
		Information(boostrap);
		StartProcess("dotnet ", new ProcessSettings {
				Arguments = new ProcessArgumentBuilder()					
					.Append(boostrap + " -p kcl.property")
				});
	});

Task("Nuget")
    .IsDependentOn("Bootstrap")
    .Does(() =>
{
	EnsureDirectoryExists("./.nuget");
	CleanDirectories("./.nuget");
	var nuGetPackSettings   = new NuGetPackSettings {
                                     BasePath                = "./ClientLibrary/",
                                     OutputDirectory         = "./.nuget"
                                 };
	NuGetPack("./ClientLibrary/ClientLibrary.nuspec", nuGetPackSettings);
});

Task("Default")
    .IsDependentOn("Nuget")
	.Does(() =>
{
	Information("All done!");
});

RunTarget(target);