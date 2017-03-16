#load "./cake-scripts/compile.csx"
#load "./cake-scripts/xunit.csx"
#load "./cake-scripts/clean.csx"
#load "./cake-scripts/coverage.csx"

var target = Argument("target", "default");

Task("default")
    .IsDependentOn("cover");

Task("cover")
    .IsDependentOn("compile-debug")
    .IsDependentOn("unit-test-coverage-report");

RunTarget(target);
