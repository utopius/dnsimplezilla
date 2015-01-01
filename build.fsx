// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open Fake

// Properties
let currentVersion =
  if not isLocalBuild then buildVersion else
  "0.0.0.1"
let buildDir = "./build/"
let appDir = buildDir + "Release/"
let deployDir = buildDir + "deploy/"
let buildReferences = !! "src/app/**/*.csproj"
let setupReferences = !! "src/app/**/*.wixproj"
let setupBinaries = !! (buildDir + "/**/*.msi")

// Targets
Target "Clean" (fun _ ->
    CleanDirs [appDir; deployDir]
)

Target "Build" (fun _ ->
    buildReferences
      |> MSBuildRelease appDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Deploy" (fun _ ->
    setupReferences
      |> MSBuildReleaseExt deployDir ["Version", currentVersion] "Build"
      |> Log "SetupBuild-Output: "
)

// Dependencies
"Clean"
  ==> "Build"
  ==> "Deploy"

// start build
RunTargetOrDefault "Build"