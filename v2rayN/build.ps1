param (
	[Parameter()]
	[ValidateNotNullOrEmpty()]
	[string]
	$OutputPath = '.\bin\v2rayN'
)

Write-Host 'Building'

dotnet publish `
	.\v2rayN.Desktop\v2rayN.Desktop.csproj `
	-c Release `
	-r win-x64 `
	--self-contained false `
	-p:PublishReadyToRun=false `
	-p:PublishSingleFile=true `
	-o "$OutputPath"


if ( -Not $? ) {
	exit $lastExitCode
	}

if ( Test-Path -Path .\bin\v2rayN ) {
    rm -Force "$OutputPath\*.pdb"
}

Write-Host 'Build done'

$SingBoxVersion = "sing-box-1.9.7-windows-amd64"

Invoke-WebRequest "https://github.com/SagerNet/sing-box/releases/latest/download/$SingBoxVersion.zip" -OutFile "sing_box.zip"

Expand-Archive -Path "sing_box.zip" -DestinationPath "."

mkdir "$OutputPath\bin\sing_box"

copy "$SingBoxVersion\sing-box.exe" "$OutputPath\bin\sing_box"

ls $OutputPath
7z a  v2rayN.zip $OutputPath
exit 0