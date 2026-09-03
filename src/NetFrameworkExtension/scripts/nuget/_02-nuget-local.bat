@echo off
setlocal enabledelayedexpansion

:: Set the project directory (modify as needed)
cd /d "%~dp0"
cd ../..

set packageId=MapIdeaHub.BirSign.NetFrameworkExtension
set feed=D:\Nugets
set srcPackageSourcePath="bin\Release\*.nupkg"
set destPackageSourcePath="%feed%"

:: A local folder feed refuses to overwrite a version it already holds. Remove
:: this exact version first so that rebuilding without bumping the version really
:: does replace it; otherwise consuming projects keep resolving the older build.
:: The version is read from the file name, so it never has to be set here.
for %%f in ("bin\Release\*.nupkg") do (
    set "packageName=%%~nf"
    set "packageVersion=!packageName:%packageId%.=!"
    if exist "%feed%\%packageId%\!packageVersion!" (
        echo Replacing %packageId% !packageVersion! in %feed% ...
        rmdir /s /q "%feed%\%packageId%\!packageVersion!"
    )
)

dotnet nuget push %srcPackageSourcePath% --source %destPackageSourcePath%

endlocal
