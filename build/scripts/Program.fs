module Program

open System
open System.IO
open Nullean.Runna.Fs
open Proc.Fs

/// The build's targets; `dotnet run --project build/scripts -- --help` lists them with these descriptions
type Target =
    /// clean known output locations
    | Clean
    /// Run build
    | Build
    /// Runs build then tests
    | Test
    /// Runs build, then creates and validates the packages and reports public API changes
    | Pack
    /// Runs tests, then creates and validates the packages and reports public API changes: exactly what CI runs
    | Ci
    /// Pack the NuGet packages into build/output
    | Generatepackages
    /// Validate the packed NuGet packages
    | Validatepackages
    /// Report public API changes against the previous release
    | Generateapichanges
    /// Build the documentation site and serve it locally
    | Docs

/// build/output, relative to the repository root
let private output = Paths.RootRelative Paths.Output.FullName

/// The packed .nupkg files, newest first
let private packageIds () =
    let version = Versions.current.Value
    Paths.Output.GetFiles("*.nupkg")
    |> Seq.sortByDescending (fun f -> f.CreationTimeUtc)
    |> Seq.map (fun f -> Path.GetFileNameWithoutExtension(f.Name).Replace("." + version, ""), Paths.RootRelative f.FullName)
    |> Seq.toList

/// Every project but this build itself: `dotnet clean` on the solution would delete the assemblies the running
/// build still loads lazily and fail the next target
let private projectsToClean () =
    ["src"; "tests"; "examples"]
    |> List.collect (fun d -> Directory.EnumerateFiles(d, "*.?sproj", SearchOption.AllDirectories) |> Seq.toList)

/// Unified artifacts layout (<UseArtifactsOutput>): `.artifacts/bin/<Project>/release/`, every package being single-TFM.
/// The packages are `Runna` and `Runna.Fs`; their projects keep the `Nullean.` prefix, as their namespaces do.
let private assembliesDir (packageId: string) = $".artifacts/bin/Nullean.{packageId}/release"

[<EntryPoint>]
let main argv =
    let app = RunnaApp<Target>("dotnet run --project build/scripts --", None)

    // `-s` skips what a target `requires`, never what it `composes`: build always cleans first
    app.Bind <| function
        | Clean -> target [] {
              if Paths.Output.Exists then Paths.Output.Delete true
              for project in projectsToClean () do
                  sh $"dotnet clean {project} -c Release -v q" }
        | Build -> command { composes [Clean] } |> executes { sh $"dotnet build -c Release" }
        // global.json selects Microsoft.Testing.Platform, so this runs the TUnit test executables directly; on GitHub
        // Actions TUnit writes its own summary of the run to GITHUB_STEP_SUMMARY
        | Test -> target [Build] { sh $"dotnet test -c Release" }
        | Pack -> command { requires [Build]; composes [Generatepackages; Validatepackages; Generateapichanges] }
        | Ci -> command { requires [Test]; composes [Generatepackages; Validatepackages; Generateapichanges] }
        | Generatepackages -> target [] {
              if Paths.Output.Exists then Paths.Output.Delete true
              sh $"dotnet pack -c Release -o {output}" }
        | Validatepackages -> target [] {
              let version = Versions.informational.Value
              let jenkinsOnWindows =
                  not (String.IsNullOrEmpty(Environment.GetEnvironmentVariable "JENKINS_URL")) && OperatingSystem.IsWindows()
              for _, package in packageIds () do
                  if jenkinsOnWindows then
                      sh $"dotnet nupkg-validator validate {package} -v {version} -k {Paths.SignKey} -t {output} -r"
                  else
                      sh $"dotnet nupkg-validator validate {package} -v {version} -k {Paths.SignKey} -t {output}" }
        | Generateapichanges -> target [] {
              let version = Versions.current.Value
              for id, _ in packageIds () do
                  let report = Path.Combine(output, $"breaking-changes-{id}.md")
                  // quoted: an unquoted | would pipe
                  sh $"dotnet assembly-differ diff 'previous-nuget|{id}|{version}|{Paths.MainTFM}' 'directory|{assembliesDir id}' -a --target Nullean.{id} -f github-comment --output {report}" }
        | Docs -> target [] {
              Documentation.build ()
              Documentation.serve 8080 }

    app.RunAsync(argv).GetAwaiter().GetResult()
