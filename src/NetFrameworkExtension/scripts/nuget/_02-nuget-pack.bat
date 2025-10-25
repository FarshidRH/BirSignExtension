@echo off

:: Set the project directory (modify as needed)
cd /d "%~dp0"
cd ../..

nuget pack -Build -Properties Configuration=Release