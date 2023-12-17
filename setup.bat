@echo off
setlocal

cd /D "%~dp0"
dotnet tool store
dotnet build
endlocal & exit /b %ERRORLEVEL%
