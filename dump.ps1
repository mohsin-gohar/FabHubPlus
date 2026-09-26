$r = Invoke-WebRequest -Uri 'http://localhost:5124/' -UseBasicParsing
[IO.File]::WriteAllText('F:\.net\FanHubPlus\FanHubPlus\home_rendered.html', $r.Content)
$d = Invoke-WebRequest -Uri 'http://localhost:5124/Explore/Details/1' -UseBasicParsing
[IO.File]::WriteAllText('F:\.net\FanHubPlus\FanHubPlus\details_rendered.html', $d.Content)
Write-Output "Files saved."
