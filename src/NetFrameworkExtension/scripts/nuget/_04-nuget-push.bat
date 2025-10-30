@echo off

:: Set the project directory (modify as needed)
cd /d "%~dp0"
cd ../..

set package=MapIdeaHub.BirSign.NetFrameworkExtension.1.0.5.nupkg
set apiKey=oy2jijluovipjvh6iwravtvs4xmj7jje7esqqxnyedwsqm
set source=https://api.nuget.org/v3/index.json

 nuget push %package% %apiKey% -Source %source%