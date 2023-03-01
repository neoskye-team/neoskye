using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

// TODO: move the test build to somewhere else

class Build : NukeBuild {
  /// Support plugins are available for:
  ///   - JetBrains ReSharper        https://nuke.build/resharper
  ///   - JetBrains Rider            https://nuke.build/rider
  ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
  ///   - Microsoft VSCode           https://nuke.build/vscode

  public static int Main() => Execute<Build>(x => x.Compile);

  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [Solution] Solution Solution;

  Target Clean => _ => _
      .Before(Restore)
      .Executes(() => {
      });

  Target Restore => _ => _
      .Executes(() => {
        MSBuild($"{Solution} /target:Restore /p:Configuration={Configuration} /nr:false");
        if (IsLocalBuild)
          MSBuild($"{Directory.GetCurrentDirectory()}/test/test.csproj /target:Restore /p:Configuration={Configuration} /nr:false");
      });

  Target Compile => _ => _
      .DependsOn(Restore)
      .Executes(() => {
        MSBuild($"{Solution} /target:Build /p:Configuration={Configuration} /nr:false");
        if (IsLocalBuild)
          MSBuild($"{Directory.GetCurrentDirectory()}/test/test.csproj /target:Build /p:Configuration={Configuration} /nr:false");
      });

  Target RunTest => _ => _
    .DependsOn(Compile)
    .Executes(() => {
      var p = Process.Start($"{Directory.GetCurrentDirectory()}/test/bin/Debug/net7.0/test.exe");
      p.WaitForExit();
    });
}
