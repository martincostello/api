#!/bin/sh
export artifacts=$(dirname "$(readlink -f "$0")")/artifacts
export configuration=Release

dotnet restore --verbosity minimal || exit 1
dotnet build --output $artifacts --configuration $configuration || exit 1
dotnet test tests/API.Tests/API.Tests.csproj --output $artifacts --configuration $configuration || exit 1
dotnet publish src/API/API.csproj --output $artifacts --configuration $configuration || exit 1
