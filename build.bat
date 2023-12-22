@echo off
goto:$Main

:Command
setlocal EnableDelayedExpansion
    set "_command=%*"
    set "_command=!_command:   = !"
    set "_command=!_command:  = !"
    echo ##[cmd] !_command!
    call !_command!
exit /b

:$Main
setlocal EnableExtensions
    cd /D "%~dp0"
    call :Command git add .
    call :Command git clean -xfd
    call :Command dotnet tool restore
    call :Command dotnet build --runtime win-x64 --configuration Debug "PlexService/"
    call :Command dotnet build --runtime win-x64 --configuration Debug "PlexServiceInstaller/"
endlocal & exit /b %ERRORLEVEL%
