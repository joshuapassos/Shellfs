#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target //"

#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.initEnvironment ()

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "tests/**/bin"
    ++ "tests/**/obj"
    |> Shell.cleanDirs
)

Target.create "Build" (fun _ ->
    let runBuild =
        DotNet.build (fun p ->
            { p with
                Configuration = DotNet.BuildConfiguration.Release
            })
    !! "src/**/*.*proj"
    ++ "tests/**/*.*proj"
    |> Seq.iter runBuild
)

Target.create "NuGet" (fun _ ->

    let packConfiguration (defaults:DotNet.PackOptions) =
        { defaults with
            Configuration = DotNet.Release
        }
    DotNet.pack packConfiguration "src/Shellfs.fsproj"

    printfn "Sending to Nuget"
    !! "src/**/bin/Release/*.nupkg"
    |> Seq.iter (DotNet.nugetPush (fun opt ->
    opt.WithPushParams(
      { opt.PushParams with
          ApiKey = Some (Environment.environVarOrFail "NUGET_APIKEY")
          Source = Some "https://api.nuget.org/v3/index.json"
      })
  ))
)

Target.create "All" ignore
Target.create "Release" ignore

"Clean"
  ==> "Build"
  ==> "All"

"Build"
  ==> "NuGet"
  ==> "Release"

Target.runOrDefault "All"
