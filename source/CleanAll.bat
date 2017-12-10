@ECHO OFF
pushd "%~dp0"
ECHO.
ECHO.
ECHO.
ECHO This script deletes all temporary build files in their
ECHO corresponding BIN and OBJ Folder contained in the following projects
ECHO.
ECHO AvalonDock.MVVMTestApp
ECHO AvalonDock.TestApp
ECHO AvalonDock.WinFormsTestApp
ECHO.
ECHO Components\Xceed.Wpf.AvalonDock
ECHO Components\Xceed.Wpf.AvalonDock.Themes.Expression
ECHO Components\Xceed.Wpf.AvalonDock.Themes.VS2013
ECHO Components\Xceed.Wpf.AvalonDock.Themes.Metro
ECHO.
ECHO.
REM Ask the user if hes really sure to continue beyond this point XXXXXXXX
set /p choice=Are you sure to continue (Y/N)?
if not '%choice%'=='Y' Goto EndOfBatch
REM Script does not continue unless user types 'Y' in upper case letter
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO Removing vs settings folder with *.sou file
ECHO.
RMDIR /S /Q .vs

ECHO Deleting BIN and OBJ Folders in AvalonDock.MVVMTestApp
ECHO.
RMDIR /S /Q AvalonDock.MVVMTestApp\bin
RMDIR /S /Q AvalonDock.MVVMTestApp\obj

ECHO Deleting BIN and OBJ Folders in AvalonDock.TestApp
ECHO.
RMDIR /S /Q AvalonDock.TestApp\bin
RMDIR /S /Q AvalonDock.TestApp\obj

ECHO Deleting BIN and OBJ Folders in AvalonDock.WinFormsTestApp
ECHO.
RMDIR /S /Q AvalonDock.WinFormsTestApp\bin
RMDIR /S /Q AvalonDock.WinFormsTestApp\obj


ECHO Deleting BIN and OBJ Folders in Xceed.Wpf.AvalonDock
ECHO.
RMDIR /S /Q Components\Xceed.Wpf.AvalonDock\bin
RMDIR /S /Q Components\Xceed.Wpf.AvalonDock\obj

ECHO Deleting BIN and OBJ Folders in Xceed.Wpf.AvalonDock.Themes.Expression

ECHO.
RMDIR /S /Q Components\Xceed.Wpf.AvalonDock.Themes.Expression\bin
RMDIR /S /Q Components\Xceed.Wpf.AvalonDock.Themes.Expression\obj

ECHO Deleting BIN and OBJ Folders in Xceed.Wpf.AvalonDock.Themes.VS2013
ECHO.
RMDIR /S /Q Components\Xceed.Wpf.AvalonDock.Themes.VS2013\bin
RMDIR /S /Q Components\Xceed.Wpf.AvalonDock.Themes.VS2013\obj

ECHO Deleting BIN and OBJ Folders in Xceed.Wpf.AvalonDock.Themes.Metro
ECHO.
RMDIR /S /Q Components\Xceed.Wpf.AvalonDock.Themes.Metro\bin
RMDIR /S /Q Components\Xceed.Wpf.AvalonDock.Themes.Metro\obj



PAUSE

:EndOfBatch
