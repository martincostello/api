#!/bin/sh
dotnet restore --verbosity minimal || exit 1
dotnet build src/API || exit 1
dotnet test tests/API.Tests || exit 1
dotnet publish src/API || exit 1
