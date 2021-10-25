#!/home/joshua/.dotnet/dotnet fsi
//

// #r "/usr/bin/Shell.dll"
#r "/home/joshua/Documents/Codigos/Shell.fs/bin/Debug/net6.0/linux-x64/Shell.dll"

// include Fake modules, see Fake modules section

open Shell
// Target.create "Deploy" (fun _ ->
//   Trace.log " --- Deploying app --- "
// )

// Target.runOrDefault "Deploy"

stdout.WriteLine "Running ExecSync in silent mode"
printfn "%A" (Shell.ExecSync ("ls", silent=true))
stdout.WriteLine "Running ExecSync in verbose mode"
printfn "%A" (Shell.ExecSync ("git sts"))


