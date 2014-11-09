// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open Fake

// Properties
let currentVersion =
  if not isLocalBuild then buildVersion else
  "0.0.0.1"
let buildDir = "./build/Release"
let setupDir = "./setup/"
let buildReferences = !! "src/app/**/*.csproj"
let setupReferences = !! "src/app/**/*.wixproj"
let setupBinaries = !! (buildDir + "/**/*.msi")

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; setupDir]
)

Target "Build" (fun _ ->
    buildReferences
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Setup" (fun _ ->
    setupReferences
      |> MSBuildReleaseExt buildDir ["Version", currentVersion] "Build"
      |> Log "SetupBuild-Output: "
    // Copy all important files to the deploy directory
    setupBinaries
      |> Copy setupDir 
)

Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// Dependencies
"Clean"
  ==> "Build"
  ==> "Default"
  ==> "Setup"

// start build
RunTargetOrDefault "Default"