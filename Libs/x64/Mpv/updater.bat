@echo OFF
:: This batch file exists to run updater.ps1 without hassle
pushd %~dp0
if exist "%~dp0\installer\updater.ps1" (
    set updater_script="%~dp0\installer\updater.ps1"
) else (
    set updater_script="%~dp0\updater.ps1"
)
powershell -noprofile -nologo -executionpolicy bypass -File %updater_script%

:: After update, updater.ps1 should not in same folder as mpv.exe
if exist "%~dp0\installer\updater.ps1" if exist "%~dp0\updater.ps1" (
    del "%~dp0\updater.ps1"
)
timeout 5
