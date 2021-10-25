module Shell

open System.Diagnostics
open System
open System.Threading.Tasks
open System.IO

type CommandResult =
  { ExitCode: int
    StandardOutput: string
    StandardError: string }

type ShellArgs =
  { Cmd: string
    Dir: string
    Silent: bool }

module private Internal =
  let call (args: ShellArgs) : Task<Result<CommandResult, string>> =
    task {

      if String.IsNullOrEmpty args.Cmd then
        return (Error "You must specify a program to run!")
      else

        let (cmd, parameters) =
          args.Cmd.Split(" ")
          |> fun x -> x |> Array.head, x |> Array.tail |> String.concat " "

        let psi = ProcessStartInfo(cmd)

        psi.WorkingDirectory <-
          if String.IsNullOrEmpty args.Dir then
            Directory.GetCurrentDirectory()
          else
            args.Dir

        psi.Arguments <- parameters
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        psi.CreateNoWindow <- true

        use p = new Process()
        p.StartInfo <- psi

        try
          p.Start() |> ignore

          let! out =
            Task.WhenAll(
              [ p.StandardOutput.ReadToEndAsync()
                p.StandardError.ReadToEndAsync() ]
            )

          if not args.Silent then
            do! stdout.WriteLineAsync out.[0]
            do! Console.Error.WriteLineAsync out.[1]
            ()

          do! p.WaitForExitAsync()

          return
            { ExitCode = p.ExitCode
              StandardOutput = out.[0]
              StandardError = out.[1] }
            |> Ok
        with
        | e -> return (Error $"Error: {e} {args.Cmd} ")
    }

let inline private await (task: Task<_>) = task.Result

type Shell =
  static member private GetParameters(cmd, ?dir, ?silent) =
    let dir =
      defaultArg dir (Directory.GetCurrentDirectory())

    let silent = defaultArg silent false

    { Cmd = cmd
      Dir = dir
      Silent = silent }

  static member Exec(cmd, ?dir, ?silent) =
    Internal.call (Shell.GetParameters(cmd, ?dir = dir, ?silent = silent))

  static member ExecSync(cmd, ?dir, ?silent) =
    await
    <| Internal.call (Shell.GetParameters(cmd, ?dir = dir, ?silent = silent))
