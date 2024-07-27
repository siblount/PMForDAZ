@echo off

rem Remove TestResults folders
if exist .\DAZ_Installer.DatabaseTests\TestResults rmdir /s /q .\DAZ_Installer.DatabaseTests\TestResults 2>nul
if exist .\DAZ_Installer.CoreTests\TestResults rmdir /s /q .\DAZ_Installer.CoreTests\TestResults 2>nul
if exist .\DAZ_Installer.IOTests\TestResults rmdir /s /q .\DAZ_Installer.IOTests\TestResults 2>nul
if exist .\TestResults rmdir /s /q .\TestResults 2>nul
rem Remove coveragereport directory
if exist .\coveragereport rmdir /s /q .\coveragereport 2>nul

rem Run tests and generate coverage report
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/*.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
