
set target=%~dp0..\
set source=%~dp0bin\Jannesen.VisualStudioExtension.TypedTSql.vsix

if not exist "%target%" mkdir "%target%"
copy "%source%" "%target%\TypedTSql.vsix"