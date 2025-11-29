@echo off

:: Set the project directory (modify as needed)
cd /d "%~dp0"
cd ../..

set srcPackageSourcePath="bin\Release\*.nupkg"
set destPackageSourcePath="D:\local-nugets"

dotnet nuget push %srcPackageSourcePath% --source %destPackageSourcePath%