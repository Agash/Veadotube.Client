# Contributing

Open an issue first for non-trivial changes so we can align on direction before code.

## Build and test

```pwsh
dotnet restore Veadotube.Client.slnx
dotnet build Veadotube.Client.slnx
dotnet test tests/Veadotube.Client.Tests/Veadotube.Client.Tests.csproj
```

## Releasing

Tag `vX.Y.Z` on `main`. Publishing a GitHub release triggers the workflow's NuGet push via OIDC trusted publishing. No API keys are stored in the repo.
