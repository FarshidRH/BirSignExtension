@echo off

:: Set the project directory (modify as needed)
cd /d "%~dp0"
cd ../..

set srcPackageSourcePath="."
set destPackageSourcePath="D:\local-nugets"

nuget init %srcPackageSourcePath% %destPackageSourcePath%