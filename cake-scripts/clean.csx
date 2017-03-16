Task("Clean")
    .Description("Calls git clean -dfx")
    .Does(() =>
    {
        if(StartProcess("git", @"clean -d -f -x -e tools/") != 0)
        {
            Error("Unable to clean directories");
        }
    });