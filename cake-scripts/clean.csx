Task("clean")
    .Description("Calls git clean -dfx")
    .Does(() =>
    {
        if(StartProcess("git", @"clean -d -f -x -e tools/") != 0)
        {
            Error("Unable to clean directories");
        }
    });

Task("clean-solution")
	.Description("Deletes the contents of the obj and bin folders")
	.Does(() =>
	{
		var subdirs = new string[] {@"\obj", @"\bin"};
		var dir = System.IO.Directory.GetCurrentDirectory();

		Information("Cleaning files from {0}", dir);
		DeleteDirectories(dir, subdirs);
	});

private void DeleteDirectories(string dir, string[] subdirs)
{
    System.IO.Directory.EnumerateDirectories(dir)
        .ToList()
        .ForEach(currentDir =>
        {
            if(subdirs.Any(currentDir.EndsWith))
            {
                Information("Cleaning {0}", currentDir);
                System.IO.Directory.Delete(currentDir, true);
            }
            else
            {
                DeleteDirectories(currentDir, subdirs);
            }
        });
}