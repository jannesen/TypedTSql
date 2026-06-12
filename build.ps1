$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version Latest

$msbuild='C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\amd64\MSBuild.exe'
$vsixinstaller='C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\VSIXInstaller.exe'

if (-not (Test-Path -Path $msbuild  -PathType Leaf)) { throw "Can't find msbuild"  }

function Cmd-Cleanup() {
    Write-Host '# cleanup'

    foreach ($proj in (Get-ChildItem -Path . -Recurse -File -Filter "*.csproj")) {
        foreach ($dir in @("obj", "bin")) {
            $path = Join-Path $proj.DirectoryName $dir

            if (Test-Path $path -PathType Container) {
                Write-Output "    delete ${path}"
                Remove-Item -Path $path -Recurse -Force
            }
        }
    }

    if (Test-Path -Path 'output' -PathType Container) {
        Write-Host '    clean output'
        Remove-Item -Path 'output' -Recurse -Force
        New-Item -Path 'output' -ItemType Directory | Out-Null
    }
}

function Cmd-Publish() {
    Cmd-Cleanup

    Write-Host '# msbuild Jannesen.TypedTSql.slnx'
    & $msbuild Jannesen.TypedTSql.slnx /nologo /verbosity:minimal /p:Configuration=Release /r /T:Rebuild
}

function Cmd-Install() {
    $vsix="$(Get-Location)\output\Jannesen.TypedTSql.vsix" 
    
    if (-not (Test-Path $vsix -PathType Leaf)) {
        Cmd-Publish
    }
        
    & $vsixinstaller "$vsix"
}

if ($args.Length -eq 0) {
    Write-Host "SYNTAX: builder publish|install|cleanup"
    Exit 1
}

$argn = 0

Set-Location $PSScriptRoot

Write-Host "# Work in: $(Get-Location)"

while ($argn -lt $args.Length) {
    switch($args[$argn]) {
    "publish" {
            Cmd-Publish
            $argn = $argn + 1
        }

    "install" {
            Cmd-Install
            $argn = $argn + 1
        }

    "cleanup" {
            Cmd-Cleanup
            $argn = $argn + 1
        }        

    default {
		    Write-Host "Unknown cmd " $args[$argn]
		    Exit 1
	    }
    }
}
