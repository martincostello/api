@echo off
dotnet restore src/API/project.json tests/API.Tests/project.json --verbosity minimal
dotnet build src/API
dotnet test tests/API.Tests
dotnet publish src/API
