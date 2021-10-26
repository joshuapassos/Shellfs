# Shellfs

![Nuget](https://img.shields.io/nuget/v/Shellfs?style=flat-square)
![GitHub branch checks state](https://img.shields.io/github/checks-status/joshuapassos/Shellfs/main?style=flat-square)

Execulte simple shell scripts with F#

## Installing

You can install using dotnet package manager:

```
dotnet add package Shellfs
```

Another approach is importing in Fsharp Script (fsx):

```
#r "nuget: Shellfs, 0.0.2"
```

## Examples

```fsharp

open Shellfs

// Execute command with output on console
Shell.Exec "echo Hello" |> fun x -> x.Result

// Exec running asynchronously
// Ok { ExitCode = 0
//      StandardOutput = "Hello"
//      StandardError = "" }

Shell.Exec "invalidCommand" |> fun x -> x.Result

// Error "An error occurred trying to start process 'invalidCommand' with working directory"

// Execute command with slent mode (default is false)
Shell.Exec ("echo hello", silent=true)

// Execute command in specific directory (Default value is current directory)
Shell.Exec ("ls -l", dir=dir)

// It's possible exec command synchronously
Shell.ExecSync ("ls -l", dir=dir)

```

## Status
This project is in early stages.
- [x] Exec commands synchronously
- [x] Exec commands with tasks

## Future
- [ ] Add basic UNIX commands

