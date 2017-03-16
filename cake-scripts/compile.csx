using System;

var slnPath = "./src/Grapevine.sln";
var toolVersion = MSBuildToolVersion.NET46;
var buildVersion = "version_num";

Task("compile-debug")
    .Does(() => {
        MSBuild(slnPath, settings =>
        {
            settings.SetConfiguration("Debug")
                .SetVerbosity(Verbosity.Minimal)
                .SetMaxCpuCount(1)
                .SetNodeReuse(false)
                .UseToolVersion(toolVersion);
        });
    });

Task("compile-release")
    .IsDependentOn("set-version")
    .Does(() => {
        MSBuild(slnPath, settings => {
            settings.SetConfiguration("Release")
                .SetVerbosity(Verbosity.Minimal)
                .SetMaxCpuCount(1)
                .SetNodeReuse(false)
                .UseToolVersion(toolVersion);
        });
    });

Task("set-version")
    .Does(() =>
    {
        foreach (var file in System.IO.Directory.GetFiles(@"src", "AssemblyInfo.cs", SearchOption.AllDirectories))
        {
            var newLines = System.IO.File.ReadLines(file)
            .Select(x =>
            {
                var value = System.Text.RegularExpressions.Regex.Replace(x, "^\\[assembly: AssemblyVersion\\(\"[0-9.]*\"\\)\\]", ("[assembly: AssemblyVersion(\"" + EnvironmentVariable(buildVersion) + "\")]"));
                value = System.Text.RegularExpressions.Regex.Replace(value, "^\\[assembly: AssemblyFileVersion\\(\"[0-9.]*\"\\)\\]", ("[assembly: AssemblyFileVersion(\"" + EnvironmentVariable(buildVersion) + "\")]"));
                return value;
            }).ToList();
            System.IO.File.WriteAllLines(file, newLines);
        }
    });