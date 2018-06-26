@echo off

SET VERSION=0.0.0
IF NOT [%1]==[] (set VERSION=%1)

SET TAG=0.0.0
IF NOT [%2]==[] (set TAG=%2)
SET TAG=%TAG:tags/=%

curl -o nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -k
if %errorlevel% neq 0 exit /b %errorlevel%

.\\nuget.exe restore .\\src\\AggregateRepository.EventStore.Tests\\AggregateRepository.EventStore.Tests.csproj -PackagesDirectory .\\src\\packages -Verbosity detailed
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet test .\src\AggregateRepository.EventStore.Tests\AggregateRepository.EventStore.Tests.csproj
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet pack .\src\AggregateRepository.EventStore\AggregateRepository.EventStore.csproj -o ..\..\dist -p:Version="%VERSION%" -p:PackageVersion="%VERSION%" -p:Tag="%TAG%" -c Release
exit /b %errorlevel%