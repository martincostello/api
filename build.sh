#!/bin/sh
dotnet restore
dotnet build src/API
dotnet test tests/API.Tests
dotnet publish src/API
