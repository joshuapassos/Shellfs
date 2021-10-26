open System.Text.RegularExpressions
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


let (|ValidVersion|_|) input =
    if Regex.IsMatch (input, "^([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+)?$") then
        Some input
    else
        None

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
    let version =
        match (Environment.environVarOrFail "VERSION") with
        | ValidVersion v -> v
        | _ -> failwith "Invalid version format x.x.x"

    let properties = [
        ("Version", version)
        ("Authors", "Joshua Passos")
        ("PackageProjectUrl", "https://github.com/joshuapassos/Shellfs")
        ("RepositoryUrl", "https://github.com/joshuapassos/Shellfs")
        ("PackageLicenseExpression", "MIT")
    ]

    let packConfiguration (defaults:DotNet.PackOptions) =
        { defaults with
            Configuration = DotNet.Release
            MSBuildParams = { defaults.MSBuildParams with Properties = properties }
        }
    DotNet.pack packConfiguration "src/Shellfs.fsproj"

    printfn "Sending to Nuget"
    !! "src/**/bin/Release/*.nupkg"
    |> Seq.iter (DotNet.nugetPush (fun opt ->
    opt.WithPushParams(
      { opt.PushParams with
          ApiKey = Some (Environment.environVarOrFail "NUGET_TOKEN")
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
