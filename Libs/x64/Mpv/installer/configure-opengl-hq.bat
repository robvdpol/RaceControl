@echo OFF
cd /D %~dp0\..
set mpv_path=%cd%\mpv.exe
set config_dir=%cd%\mpv
set config_file=%cd%\mpv\mpv.conf
if not exist "%mpv_path%" call :die "mpv.exe not found"
echo.
echo.
echo ////////////////////////////////////////////////////////////////////////////
echo                                                                            /
echo  This script will configure mpv player to use opengl-hq video output for   /
echo  high-quality video rendering.                                             /
echo                                                                            /
echo  Opengl-hq will configure to use many useful video filters like debanding  / 
echo  filter,dithering..                                                        /
echo                                                                            /
echo  If the played video lagging a lot after using opengl-hq, delete the "mpv" /
echo  folder to use default mpv's video output (opengl) which suitable for      /
echo  slower computer.                                                          /
echo                                                                            /
echo                                                                            /
echo  Make sure you have write permission to create folder,                     /
echo  %config_dir%
echo                                                                            /
echo  For more info about mpv's settings, read doc\manual.pdf                   /
echo  or visit https://mpv.io/manual/master/                                    /
echo                                                                            /
echo ////////////////////////////////////////////////////////////////////////////
echo.
echo Press "Enter" to continue..
echo.
set /P enterKey={ENTER}

mkdir "%config_dir%"
echo # High quality video rendering for fast computer. > "%config_file%"
echo profile=gpu-hq >> "%config_file%"
echo deband=no >> "%config_file%"

:die
    if not [%1] == [] echo %~1
    pause
    exit 1