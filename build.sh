#!/bin/sh
dotnet restore
dotnet build src/API
dotnet publish src/API
