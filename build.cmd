@echo off

SET VERSION=0.0.0
IF NOT [%1]==[] (SET VERSION=%1)

SET TAG=0.0.0
IF NOT [%2]==[] (SET TAG=%2)
SET TAG=%TAG:tags/=%

SET RUNTESTS=true
IF NOT [%3]==[] (SET RUNTESTS=%3)

dotnet restore .\src\AggregateRepository.KurrentDB.sln -PackagesDirectory .\src\packages -Verbosity detailed

dotnet format .\src\AggregateRepository.KurrentDB.sln --severity warn --verify-no-changes -v diag
IF %errorlevel% neq 0 EXIT /B %errorlevel%

IF /I "%RUNTESTS%"=="true" (
	dotnet test .\src\AggregateRepository.KurrentDB.Tests\AggregateRepository.KurrentDB.Tests.csproj
	IF %errorlevel% neq 0 EXIT /B %errorlevel%
) ELSE (
	ECHO Skipping tests because RUNTESTS is not set to "true".
)

dotnet pack .\src\AggregateRepository.KurrentDB\AggregateRepository.KurrentDB.csproj -o .\dist -p:Version="%VERSION%" -p:PackageVersion="%VERSION%" -p:Tag="%TAG%" -c Release