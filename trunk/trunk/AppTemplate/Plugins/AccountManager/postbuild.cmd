set _localAppData=%LOCALAPPDATA%\AppTemplate\Plugins
set _targetName=%1%
set _targetPath=%2%
set _pluginPath=%_localAppData%\%_targetName%
set _debug=0

if "1"=="%_debug%" goto debug
goto makepath

:debug
echo #------------------------------------------------------------
echo #--debug-----------------------------------------------------
echo #------------------------------------------------------------
echo # _localAppData=%_localAppData%
echo # _targetname=%_targetname%
echo # _targetPath=%_targetPath%
echo # _pluginPath=%_pluginPath%
echo #------------------------------------------------------------
echo #------------------------------------------------------------
echo #------------------------------------------------------------

:makepath
if not exist "%_pluginPath%" mkdir "%_pluginPath%"
goto copy_plugin

:copy_plugin
copy /y %_targetPath%%_targetName%.dll "%_localAppData%"
goto copy_dependencies

:copy_dependencies
copy /y %_targetPath%*.dll "%_pluginPath%"
goto delete_plugin_from_dependency_path

:delete_plugin_from_dependency_path
if exist "%_localAppData%\%_targetName%\%_targetName%.dll" del /f /q "%_localAppData%\%_targetName%\%_targetName%.dll"

:end
