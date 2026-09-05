$ErrorActionPreference = 'Stop'
$installer = Join-Path $env:TEMP 'pixelrick-innosetup-6.7.3.exe'
Invoke-WebRequest 'https://github.com/jrsoftware/issrc/releases/download/is-6_7_3/innosetup-6.7.3.exe' -OutFile $installer
$expected = '9C73C3BAE7ED48D44112A0F48E66742C00090BDB5BEF71D9D3C056C66E97B732'
if ((Get-FileHash $installer -Algorithm SHA256).Hash -ne $expected) { throw 'Inno Setup checksum mismatch' }
$process = Start-Process -FilePath $installer -ArgumentList '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CURRENTUSER' -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode) { throw "Inno Setup install failed: $($process.ExitCode)" }
