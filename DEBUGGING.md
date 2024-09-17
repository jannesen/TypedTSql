# Debugging

## MSBuild
To debug the MSBuild TASK set te following in project: Jannesen.VisualStudioExtension.TypedTSql.Build
```
Program: C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Current\Bin\amd64\MSBuild.exe
Arguments: <path-to-.ttsqlproj> /property:Configuration=Release /property:Platform=AnyCPU /T:Rebuild
```

## Visual Studio Extension

To debug the Visual Studio Extension set the follosing in project: Jannesen.VisualStudioExtension.TypedTSql
```
Program: C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe
Arguments: /rootsuffix Exp <path-to-.sln>
```