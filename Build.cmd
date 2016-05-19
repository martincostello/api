@echo off
cd .\src\API
dotnet restore
dotnet build
dotnet publish
