
$DelPaths = @(
".\UDPBD-for-XEB+-CLI\bin"
".\UDPBD-for-XEB+-CLI\obj"
".\UDPBD-for-XEB+-GUI\bin"
".\UDPBD-for-XEB+-GUI\obj"
".\udpbd-vexfat"
".\udpbd-server"
)


foreach ($delPath in $DelPaths)
{
    if (Test-Path $delPath)
    {
        Remove-Item -Path $delPath -Recurse
    }
}

