#requires -Version 5.1
<#
Original vector brand illustrations, not screenshots. Uses Windows System.Drawing
and the installed Arial Bold font; no downloaded assets or dependencies.
Run from any directory. Outputs stay beside this script.
#>
param([switch]$AdaptiveOnly)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$script:Invariant = [Globalization.CultureInfo]::InvariantCulture

function N($value) { ([double]$value).ToString('0.###', $script:Invariant) }
function Color($hex, $alpha = 255) {
    $rgb = [Drawing.ColorTranslator]::FromHtml($hex)
    [Drawing.Color]::FromArgb([int]$alpha, $rgb.R, $rgb.G, $rgb.B)
}
function Begin-Art($width, $height, $name, $description, [bool]$transparent = $false) {
    $script:Name = $name
    $pixelFormat=if($transparent){[Drawing.Imaging.PixelFormat]::Format32bppArgb}else{[Drawing.Imaging.PixelFormat]::Format24bppRgb}
    $script:Bitmap = New-Object Drawing.Bitmap($width, $height, $pixelFormat)
    $script:G = [Drawing.Graphics]::FromImage($script:Bitmap)
    if($transparent){$script:G.Clear([Drawing.Color]::Transparent)}
    $script:G.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $script:G.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $script:Svg = New-Object 'Collections.Generic.List[string]'
    $script:Svg.Add('<svg xmlns="http://www.w3.org/2000/svg" width="'+$width+'" height="'+$height+'" viewBox="0 0 '+$width+' '+$height+'" role="img" aria-labelledby="title desc">')
    $script:Svg.Add('<title id="title">Edge Rush</title><desc id="desc">'+$description+'</desc>')
    $script:Gradient = 0
    $script:TX=0; $script:TY=0; $script:Scale=1
}
function Polygon($coords, $hex, $alpha = 255) {
    $points = @(); $xy = @()
    for ($i=0; $i -lt $coords.Count; $i+=2) {
        $px=$script:TX+$script:Scale*$coords[$i]; $py=$script:TY+$script:Scale*$coords[$i+1]
        $points += New-Object Drawing.PointF([single]$px, [single]$py)
        $xy += (N $px)+','+(N $py)
    }
    $brush = New-Object Drawing.SolidBrush((Color $hex $alpha))
    try { $script:G.FillPolygon($brush, [Drawing.PointF[]]$points) } finally { $brush.Dispose() }
    $script:Svg.Add('<polygon points="'+($xy -join ' ')+'" fill="'+$hex+'" fill-opacity="'+(N ($alpha/255))+'"/>')
}
function Line($coords, $hex, $width = 2, $alpha = 255) {
    $points = @(); $xy = @()
    for ($i=0; $i -lt $coords.Count; $i+=2) {
        $px=$script:TX+$script:Scale*$coords[$i]; $py=$script:TY+$script:Scale*$coords[$i+1]
        $points += New-Object Drawing.PointF([single]$px, [single]$py)
        $xy += (N $px)+','+(N $py)
    }
    $width*=$script:Scale
    $pen = New-Object Drawing.Pen((Color $hex $alpha), [single]$width)
    $pen.StartCap = $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin = [Drawing.Drawing2D.LineJoin]::Round
    try { $script:G.DrawLines($pen, [Drawing.PointF[]]$points) } finally { $pen.Dispose() }
    $script:Svg.Add('<polyline points="'+($xy -join ' ')+'" fill="none" stroke="'+$hex+'" stroke-width="'+(N $width)+'" stroke-opacity="'+(N ($alpha/255))+'" stroke-linecap="round" stroke-linejoin="round"/>')
}
function Ellipse($x, $y, $width, $height, $hex, $alpha = 255) {
    $x=$script:TX+$script:Scale*$x; $y=$script:TY+$script:Scale*$y
    $width*=$script:Scale; $height*=$script:Scale
    $brush = New-Object Drawing.SolidBrush((Color $hex $alpha))
    try { $script:G.FillEllipse($brush,[single]$x,[single]$y,[single]$width,[single]$height) } finally { $brush.Dispose() }
    $script:Svg.Add('<ellipse cx="'+(N ($x+$width/2))+'" cy="'+(N ($y+$height/2))+'" rx="'+(N ($width/2))+'" ry="'+(N ($height/2))+'" fill="'+$hex+'" fill-opacity="'+(N ($alpha/255))+'"/>')
}
function RoundRect([double]$x, [double]$y, [double]$width, [double]$height, [double]$radius, [string]$hex, [int]$alpha = 255) {
    # Explicit bevelled corners are shared by raster and SVG output. Numeric
    # parameters keep negative-coordinate arithmetic numeric in PowerShell 5.
    $r=[Math]::Min($radius,[Math]::Min($width/2,$height/2))
    Polygon @(
        ($x+$r),$y, ($x+$width-$r),$y,
        ($x+$width),($y+$r), ($x+$width),($y+$height-$r),
        ($x+$width-$r),($y+$height), ($x+$r),($y+$height),
        $x,($y+$height-$r), $x,($y+$r)
    ) $hex $alpha
}
function GradientBackground($width, $height, $top, $bottom) {
    $rect = New-Object Drawing.Rectangle(0,0,$width,$height)
    $brush = New-Object Drawing.Drawing2D.LinearGradientBrush($rect,(Color $top),(Color $bottom),90)
    try { $script:G.FillRectangle($brush,$rect) } finally { $brush.Dispose() }
    $script:Gradient++
    $id = 'gradient'+$script:Gradient
    $script:Svg.Add('<defs><linearGradient id="'+$id+'" x2="0" y2="1"><stop stop-color="'+$top+'"/><stop offset="1" stop-color="'+$bottom+'"/></linearGradient></defs><rect width="'+$width+'" height="'+$height+'" fill="url(#'+$id+')"/>')
}
function TextOutline($text, $x, $y, $size, $hex) {
    $path = New-Object Drawing.Drawing2D.GraphicsPath
    $family = New-Object Drawing.FontFamily('Arial')
    $format = [Drawing.StringFormat]::GenericTypographic
    $path.AddString($text,$family,[int][Drawing.FontStyle]::Bold,[single]$size,(New-Object Drawing.PointF($x,$y)),$format)
    $brush = New-Object Drawing.SolidBrush((Color $hex))
    try {
        $script:G.FillPath($brush,$path)
        $points=$path.PathPoints; $types=$path.PathTypes; $data=New-Object Text.StringBuilder
        for ($i=0; $i -lt $points.Count; $i++) {
            $kind=$types[$i] -band 7
            if ($kind -eq 0) { $null=$data.Append('M'+(N $points[$i].X)+' '+(N $points[$i].Y)+' ') }
            elseif ($kind -eq 1) { $null=$data.Append('L'+(N $points[$i].X)+' '+(N $points[$i].Y)+' ') }
            elseif ($kind -eq 3) {
                $null=$data.Append('C'+(N $points[$i].X)+' '+(N $points[$i].Y)+' '+(N $points[$i+1].X)+' '+(N $points[$i+1].Y)+' '+(N $points[$i+2].X)+' '+(N $points[$i+2].Y)+' ')
                $i+=2
            }
            if (($types[$i] -band 128) -ne 0) { $null=$data.Append('Z ') }
        }
        $script:Svg.Add('<path aria-label="'+$text+'" d="'+$data.ToString()+'" fill="'+$hex+'"/>')
    } finally { $brush.Dispose(); $path.Dispose(); $family.Dispose() }
}
function Car($cx, $cy, $scale = 1) {
    # All components share the same original top-down silhouette. Body is the
    # Cyan Interceptor's actual palette, but this larger drawing is brand art.
    $script:TX=$cx; $script:TY=$cy; $script:Scale=$scale
    Ellipse -94 -144 188 320 '#40E8FF' 8
    Ellipse -82 -130 164 296 '#40E8FF' 13
    RoundRect -73 -77 21 71 8 '#020611'
    RoundRect 52 -77 21 71 8 '#020611'
    RoundRect -76 54 25 78 8 '#020611'
    RoundRect 51 54 25 78 8 '#020611'
    Polygon @(-51,-127, -62,-107, -66,75, -57,135, -40,146, 40,146, 57,135, 66,75, 62,-107, 51,-127) '#26C4E8'
    Polygon @(-43,-118, -51,-98, -49,-64, 49,-64, 51,-98, 43,-118) '#78F4FF'
    Polygon @(-42,-53, -34,-84, 34,-84, 42,-53, 46,-18, -46,-18) '#071729'
    Polygon @(-37,-52, -30,-75, 30,-75, 37,-52) '#1E5263'
    Polygon @(-45,62, -37,106, 37,106, 45,62) '#08182B'
    Polygon @(-33,66, -27,93, 27,93, 33,66) '#184255'
    RoundRect -43 -7 86 61 12 '#32D6F1'
    Line @(-54,-105,-59,56,-50,118) '#B9FBFF' 3 160
    Line @(54,-105,59,56,50,118) '#0E728F' 4
    Line @(-46,-104,-26,-104) '#EDFFFF' 9
    Line @(26,-104,46,-104) '#EDFFFF' 9
    Line @(-45,126,45,126) '#FF5261' 7
    RoundRect -69 135 138 11 4 '#102234'
    Line @(-57,137,57,137) '#78F4FF' 3
    $script:TX=0; $script:TY=0; $script:Scale=1
}
function End-Art {
    $script:Svg.Add('</svg>')
    $png=Join-Path $PSScriptRoot ($script:Name+'.png')
    $svg=Join-Path $PSScriptRoot ($script:Name+'.svg')
    try {
        $script:Bitmap.Save($png,[Drawing.Imaging.ImageFormat]::Png)
        [IO.File]::WriteAllText($svg,($script:Svg -join "`n"),(New-Object Text.UTF8Encoding($false)))
    } finally { $script:G.Dispose(); $script:Bitmap.Dispose() }
    Write-Output $png
}

if (-not $AdaptiveOnly) {
Begin-Art 512 512 'icon-512' 'Original opaque brand icon: a cyan car on a dark three-lane neon road; no game interface.'
GradientBackground 512 512 '#070914' '#15102B'
Polygon @(82,0, 429,0, 478,512, 31,512) '#1B1E2A'
Line @(82,0,31,512) '#40E8FF' 4 160
Line @(429,0,478,512) '#FF48AA' 4 180
Line @(194,0,178,512) '#277F99' 3 210
Line @(318,0,334,512) '#277F99' 3 210
foreach($y in @(25,104,183,262,341,420)) { Line @(190,$y,188,($y+30)) '#66D5E8' 4 115; Line @(322,$y,324,($y+30)) '#66D5E8' 4 115 }
Line @(222,378,206,512) '#40E8FF' 11 28
Line @(290,378,306,512) '#40E8FF' 11 28
Car 256 253 1.36
End-Art

Begin-Art 1920 1080 'cover-1920x1080' 'Original Edge Rush brand cover with outlined title and cyan car on a three-lane neon road. Illustration, not a gameplay screenshot.'
GradientBackground 1920 1080 '#070914' '#17112B'
Ellipse 1020 83 720 720 '#402978' 23
Ellipse 1100 163 560 560 '#523696' 20
Ellipse 1180 243 400 400 '#6843A1' 20
foreach($building in @(@(990,214,48,385),@(1054,293,62,306),@(1131,187,41,412),@(1643,169,57,430),@(1716,246,43,353),@(1776,207,79,392))) {
    Polygon @($building[0],$building[1],($building[0]+$building[2]),$building[1],($building[0]+$building[2]),599,$building[0],599) '#0E1326'
    Line @($building[0],$building[1],($building[0]+$building[2]),$building[1]) '#FF48AA' 2 40
}
Line @(1000,597,1840,597) '#FF48AA' 3 110
Polygon @(1287,354,1497,354,1840,1080,930,1080) '#1B1E2A'
Line @(1280,354,916,1080) '#40E8FF' 8 35
Line @(1287,354,930,1080) '#40E8FF' 3 220
Line @(1504,354,1854,1080) '#FF48AA' 8 35
Line @(1497,354,1840,1080) '#FF48AA' 3 220
Line @(1357,355,1234,1080) '#297D96' 2 220
Line @(1427,355,1537,1080) '#297D96' 2 220
foreach($row in @(@(390,419),@(459,493),@(547,589),@(657,710),@(789,855),@(943,1024))) {
    foreach($lane in @(0,1)) {
        $x1=if($lane -eq 0){1357-123*($row[0]-355)/725}else{1427+110*($row[0]-355)/725}
        $x2=if($lane -eq 0){1357-123*($row[1]-355)/725}else{1427+110*($row[1]-355)/725}
        Line @($x1,$row[0],$x2,$row[1]) '#A7E9F4' 4 135
    }
}
Line @(1324,820,1280,1000) '#40E8FF' 12 30
Line @(1434,820,1478,1000) '#40E8FF' 12 30
Car 1387 731 1.44
TextOutline 'EDGE' 146 278 210 '#EEF6FF'
TextOutline 'RUSH' 139 477 234 '#40E8FF'
Line @(157,781,769,781) '#40E8FF' 4 160
End-Art
}

# Adaptive layers share the original geometry. Main car plus outermost shadow
# occupies a centred 288-pixel-high box inside the requested central 300 square.
Begin-Art 512 512 'adaptive-foreground-512' 'Transparent adaptive foreground layer: original cyan car, local wheels and shadow only, inside the central safe region.' $true
Car 256 241.6 0.9
End-Art

Begin-Art 512 512 'adaptive-background-512' 'Opaque adaptive background layer: original dark three-lane neon road only, without car or text.'
GradientBackground 512 512 '#070914' '#15102B'
Polygon @(82,0, 429,0, 478,512, 31,512) '#1B1E2A'
Line @(82,0,31,512) '#40E8FF' 4 160
Line @(429,0,478,512) '#FF48AA' 4 180
Line @(194,0,178,512) '#277F99' 3 210
Line @(318,0,334,512) '#277F99' 3 210
foreach($y in @(25,104,183,262,341,420)) { Line @(190,$y,188,($y+30)) '#66D5E8' 4 115; Line @(322,$y,324,($y+30)) '#66D5E8' 4 115 }
End-Art
