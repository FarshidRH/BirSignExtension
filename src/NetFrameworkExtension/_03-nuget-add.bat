@echo off

set srcPackageSourcePath="."
set destPackageSourcePath="D:\local-nugets"

nuget init %srcPackageSourcePath% %destPackageSourcePath%