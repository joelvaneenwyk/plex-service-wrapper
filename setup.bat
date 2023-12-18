@echo off
setlocal EnableExtensions
    cd /D "%~dp0"
    dotnet tool restore
    dotnet build
endlocal & exit /b %ERRORLEVEL%
