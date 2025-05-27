#!/bin/bash
set -e
version="0.0.0"
if [ -n "$1" ]; then version="$1"
fi

tag="0.0.0"
if [ -n "$2" ]; then tag="$2"
fi
tag=${tag/tags\//}

dotnet test .\\src\\AggregateRepository.Kurrent.Tests\\AggregateRepository.Kurrent.Tests.csproj
dotnet pack .\\src\\AggregateRepository.Kurrent\\AggregateRepository.Kurrent.csproj -o .\\dist -p:Version="$version" -p:PackageVersion="$version" -p:Tag="$tag" -c Release
