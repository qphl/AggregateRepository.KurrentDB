@echo off

SET VERSION=0.0.0
IF NOT [%1]==[] (set VERSION=%1)

SET TAG=0.0.0
IF NOT [%2]==[] (set TAG=%2)
SET TAG=%TAG:tags/=%

dotnet restore .\src\AggregateRepository.Kurrent.sln -PackagesDirectory .\src\packages -Verbosity detailed

dotnet format .\src\AggregateRepository.Kurrent.sln --severity warn --verify-no-changes -v diag
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet test .\src\AggregateRepository.Kurrent.Tests\AggregateRepository.Kurrent.Tests.csproj
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet pack .\src\AggregateRepository.Kurrent\AggregateRepository.Kurrent.csproj -o .\dist -p:Version="%VERSION%" -p:PackageVersion="%VERSION%" -p:Tag="%TAG%" -c Release