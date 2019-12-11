@ECHO OFF
pushd "%~dp0"
ECHO.
ECHO.
ECHO.
ECHO This script deletes all temporary build files in their
ECHO corresponding BIN and OBJ Folder contained in the following projects
ECHO.
ECHO MLibTest
ECHO MLibTest_Components
ECHO.
ECHO MVVMTestApp
ECHO TestApp
ECHO WinFormsTestApp
ECHO.
ECHO Components\AvalonDock
ECHO Components\AvalonDock.Themes.Aero
ECHO Components\AvalonDock.Themes.Expression
ECHO Components\AvalonDock.Themes.VS2013
ECHO Components\AvalonDock.Themes.VS2010
ECHO Components\AvalonDock.Themes.Metro
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

ECHO Deleting BIN and OBJ Folders in MLibTest
ECHO.
RMDIR /S /Q MLibTest\MLibTest\bin
RMDIR /S /Q MLibTest\MLibTest\obj

ECHO Deleting BIN and OBJ Folders in MLibTest_Components
ECHO.
RMDIR /S /Q MLibTest\MLibTest_Components\ServiceLocator\bin
RMDIR /S /Q MLibTest\MLibTest_Components\ServiceLocator\obj

RMDIR /S /Q MLibTest\MLibTest_Components\Settings\Settings\bin
RMDIR /S /Q MLibTest\MLibTest_Components\Settings\Settings\obj

RMDIR /S /Q MLibTest\MLibTest_Components\Settings\SettingsModel\bin
RMDIR /S /Q MLibTest\MLibTest_Components\Settings\SettingsModel\obj

ECHO Deleting BIN and OBJ Folders in MVVMTestApp
ECHO.
RMDIR /S /Q MVVMTestApp\bin
RMDIR /S /Q MVVMTestApp\obj

ECHO Deleting BIN and OBJ Folders in TestApp
ECHO.
RMDIR /S /Q TestApp\bin
RMDIR /S /Q TestApp\obj

ECHO Deleting BIN and OBJ Folders in WinFormsTestApp
ECHO.
RMDIR /S /Q WinFormsTestApp\bin
RMDIR /S /Q WinFormsTestApp\obj

ECHO Deleting BIN and OBJ Folders in AvalonDock.Themes.Aero
ECHO.
RMDIR /S /Q Components\AvalonDock.Themes.Aero\bin
RMDIR /S /Q Components\AvalonDock.Themes.Aero\obj

ECHO Deleting BIN and OBJ Folders in AvalonDock
ECHO.
RMDIR /S /Q Components\AvalonDock\bin
RMDIR /S /Q Components\AvalonDock\obj

ECHO Deleting BIN and OBJ Folders in AvalonDock.Themes.Expression

ECHO.
RMDIR /S /Q Components\AvalonDock.Themes.Expression\bin
RMDIR /S /Q Components\AvalonDock.Themes.Expression\obj

ECHO Deleting BIN and OBJ Folders in AvalonDock.Themes.VS2010
ECHO.
RMDIR /S /Q Components\AvalonDock.Themes.VS2010\bin
RMDIR /S /Q Components\AvalonDock.Themes.VS2010\obj

ECHO Deleting BIN and OBJ Folders in AvalonDock.Themes.VS2013
ECHO.
RMDIR /S /Q Components\AvalonDock.Themes.VS2013\bin
RMDIR /S /Q Components\AvalonDock.Themes.VS2013\obj

ECHO Deleting BIN and OBJ Folders in AvalonDock.Themes.Metro
ECHO.
RMDIR /S /Q Components\AvalonDock.Themes.Metro\bin
RMDIR /S /Q Components\AvalonDock.Themes.Metro\obj



PAUSE

:EndOfBatch
