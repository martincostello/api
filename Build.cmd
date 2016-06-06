@echo off
dotnet restore --verbosity minimal
dotnet build src/API
dotnet test tests/API.Tests
dotnet publish src/API
