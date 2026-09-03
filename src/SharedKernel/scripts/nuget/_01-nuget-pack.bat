@echo off

:: Set the project directory (modify as needed)
cd /d "%~dp0"
cd ../..

:: Drop older packages so the local and push scripts only ever see the
:: version that was just built.
if exist "bin\Release\*.nupkg" del /q "bin\Release\*.nupkg"

:: The version comes from <Version> in Directory.Build.props at the
:: repository root, so it never has to be set here.
dotnet pack -c Release
