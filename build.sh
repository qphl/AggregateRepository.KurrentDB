#!/bin/bash
set -e
version="0.0.0"
if [ -n "$1" ]; then version="$1"
fi

tag="0.0.0"
if [ -n "$2" ]; then tag="$2"
fi
tag=${tag/tags\//}

curl -o nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
.\\nuget.exe restore .\\src\\AggregateRepository.EventStore.Tests\\AggregateRepository.EventStore.Tests.csproj -PackagesDirectory .\\src\\packages -Verbosity detailed

dotnet test .\\src\\AggregateRepository.EventStore.Tests\\AggregateRepository.EventStore.Tests.csproj
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet pack .\\src\\AggregateRepository.EventStore\\AggregateRepository.EventStore.csproj -o ..\\..\\dist -p:Version="$version" -p:PackageVersion="$version" -p:Tag="$tag" -c Release
