#tool "nuget:?package=xunit.runner.console&version=2.1.0"
#tool "nuget:?package=OpenCover"
#tool "nuget:?package=ReportGenerator"

using System.Collections.Generic;

private XUnit2Settings xuSettings = new XUnit2Settings {ShadowCopy = false};
private OpenCoverSettings ocSettings = new OpenCoverSettings().WithFilter("+[*]* -[*.Tests*]* -[*]*.*Config -[xunit*]* -[*]Grapevine.Core.*");

Task("unit-test-coverage")
.Does(() =>
{
    System.Console.WriteLine(ReportPaths.Unit.OutputFile);
    OpenCover(tool => { tool.XUnit2("./**/Grapevine.Tests.Unit.dll", xuSettings); },
        new FilePath(ReportPaths.Unit.OutputFile),
        ocSettings);
});

Task("integration-test-coverage")
.Does(() =>
{
    OpenCover(tool => { tool.XUnit2("./**/Grapevine.Tests.Integration.dll", xuSettings); },
        new FilePath(ReportPaths.Integration.OutputFile),
        ocSettings);
});

Task("all-coverage-report")
.IsDependentOn("unit-test-coverage")
.IsDependentOn("integration-test-coverage")
.Does(()=>
{
    ReportGenerator(ReportPaths.OutputFiles, new DirectoryPath(ReportPaths.Combined.OutputFolder));
    LaunchReport(ReportPaths.Combined.ReportIndex);
});

Task("unit-test-coverage-report")
.IsDependentOn("unit-test-coverage")
.Does(()=>
{
    ReportGenerator(new FilePath(ReportPaths.Unit.OutputFile), new DirectoryPath(ReportPaths.Unit.OutputFolder));
    LaunchReport(ReportPaths.Unit.ReportIndex);
});

Task("integration-test-coverage-report")
.IsDependentOn("integration-test-coverage")
.Does(()=>
{
    ReportGenerator(new FilePath(ReportPaths.Integration.OutputFile), new DirectoryPath(ReportPaths.Integration.OutputFolder));
    LaunchReport(ReportPaths.Integration.ReportIndex);
});

private void LaunchReport(string reportPath)
{
    if (IsRunningOnWindows())
    {
        StartProcess("explorer.exe", reportPath);
    }
    else
    {
        Information("Unable to launch report on non-Windows OS.");
        Information("Coverage report can be found at: " + reportPath);
    }
}

internal static class ReportPaths
{
    private const string OutputFolder = "code-coverage";
    public static string OutputDirectory { get; set; }
    public static IList<FilePath> OutputFiles { get; set;  }
    public static IoPaths Unit { get; set;  }
    public static IoPaths Integration { get; set;  }
    public static IoPaths Combined { get; set;  }

    static ReportPaths()
    {
        OutputDirectory = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), OutputFolder);
        System.IO.Directory.CreateDirectory(OutputDirectory);

        Unit = new IoPaths("unit");
        Integration = new IoPaths("integration");
        Combined = new IoPaths("combined");
        OutputFiles = new List<FilePath> {new FilePath(Unit.OutputFile), new FilePath(Integration.OutputFile)};
    }
}

internal class IoPaths
{
    public string OutputFile { get; set;  }
    public string OutputFolder { get; set;  }

    public string ReportIndex { get; set;  }

    public IoPaths(string type)
    {
        OutputFile = System.IO.Path.Combine(ReportPaths.OutputDirectory, type + "-test-coverage.xml");
        OutputFolder = System.IO.Path.Combine(ReportPaths.OutputDirectory, "reports", type);
        ReportIndex = System.IO.Path.Combine(OutputFolder, "index.htm");

        System.IO.Directory.CreateDirectory(OutputFolder);
    }
}
