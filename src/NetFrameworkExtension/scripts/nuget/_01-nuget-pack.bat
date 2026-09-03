@echo off

:: Set the project directory (modify as needed)
cd /d "%~dp0"
cd ../..

:: Drop older packages so the local and push scripts only ever see the
:: version that was just built.
if exist "bin\Release\*.nupkg" del /q "bin\Release\*.nupkg"

:: Builds and packs from NetFrameworkExtension.nuspec. The version comes from
:: <Version> in Directory.Build.props at the repository root.
::
:: Do NOT go back to "nuget pack -Build": that makes nuget.exe locate an
:: MSBuild by itself and it can pick the wrong one (for example the copy
:: shipped inside SQL Server Management Studio), which fails the build.
dotnet msbuild NetFrameworkExtension.csproj -t:PackNuspec -p:Configuration=Release
