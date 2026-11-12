#!/usr/bin/env bash
dotnet run --project "$(dirname "$0")/build/Build.csproj" -- "$@"
