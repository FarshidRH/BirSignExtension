@echo off

:: Set the project directory (modify as needed)
cd /d "%~dp0"
cd ../..

dotnet pack -c Release