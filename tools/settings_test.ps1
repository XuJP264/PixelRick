param([string]$Exe = '')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class PetWindowTest {
 [StructLayout(LayoutKind.Sequential)] public struct Point { public int X,Y; }
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; }
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr FindWindow(IntPtr cls,string title);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hwnd,int msg,IntPtr w,IntPtr l);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
 [DllImport("user32.dll")] public static extern bool GetCursorPos(out Point point);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern bool SetProcessDpiAwarenessContext(IntPtr value);
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr value);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd,int command);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint dx,uint dy,uint data,IntPtr extra);
}
'@
$root=Split-Path $PSScriptRoot -Parent
if (!$Exe) { $Exe=Join-Path $root 'dist/publish/PixelRick.exe' }
$config=Join-Path $env:APPDATA 'PixelRick/config.json'
$backup=if(Test-Path $config){[IO.File]::ReadAllText($config)}else{$null}
[void][PetWindowTest]::SetProcessDpiAwarenessContext([IntPtr](-4))
[void][PetWindowTest]::SetThreadDpiAwarenessContext([IntPtr](-4))
$old=New-Object PetWindowTest+Point
[void][PetWindowTest]::GetCursorPos([ref]$old)
$process=Start-Process -FilePath $Exe -WindowStyle Hidden -PassThru
try {
 Start-Sleep -Seconds 2
 $hwnd=[PetWindowTest]::FindWindow([IntPtr]0,'PixelRick')
 if ($hwnd -eq [IntPtr]::Zero) { throw 'Pet window missing' }
 [void][PetWindowTest]::ShowWindow($hwnd,4)
 $petRect=New-Object PetWindowTest+Rect
 [void][PetWindowTest]::GetWindowRect($hwnd,[ref]$petRect)
 [void][PetWindowTest]::SetCursorPos($petRect.Left+128,$petRect.Top+140)
 Start-Sleep -Milliseconds 100
 [PetWindowTest]::mouse_event(8,0,0,0,[IntPtr]0)
 Start-Sleep -Milliseconds 80
 [PetWindowTest]::mouse_event(16,0,0,0,[IntPtr]0)
 Start-Sleep -Milliseconds 500
 $desktop=[System.Windows.Automation.AutomationElement]::RootElement
 $name=New-Object System.Windows.Automation.PropertyCondition ([System.Windows.Automation.AutomationElement]::NameProperty),'Settings'
 $pidCondition=New-Object System.Windows.Automation.PropertyCondition ([System.Windows.Automation.AutomationElement]::ProcessIdProperty),$process.Id
 $condition=New-Object System.Windows.Automation.AndCondition $name,$pidCondition
 $item=$desktop.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$condition)
 if (!$item) { $desktop.FindAll([System.Windows.Automation.TreeScope]::Descendants,$pidCondition) | ForEach-Object { Write-Output $_.Current.Name }; throw 'Settings menu item missing' }
 $invoke=$item.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
 $invoke.Invoke()
 Start-Sleep -Seconds 1
 $windowName=New-Object System.Windows.Automation.PropertyCondition ([System.Windows.Automation.AutomationElement]::NameProperty),('PixelRick '+[char]0x00B7+' Settings')
 $window=$desktop.FindFirst([System.Windows.Automation.TreeScope]::Children,(New-Object System.Windows.Automation.AndCondition $windowName,$pidCondition))
 if (!$window) { throw 'Settings window missing' }
 $rect=$window.Current.BoundingRectangle
 # Exclude Windows' invisible resize border and rounded corners so no desktop content is published.
 $dpiScale=$rect.Width/430
 $captureX=[int][Math]::Ceiling($rect.X+8*$dpiScale)
 $captureY=[int][Math]::Ceiling($rect.Y+30*$dpiScale)
 $bitmap=New-Object System.Drawing.Bitmap ([int]($rect.Width-16*$dpiScale)),([int]($rect.Height-41*$dpiScale))
 $graphics=[System.Drawing.Graphics]::FromImage($bitmap)
 $graphics.CopyFromScreen($captureX,$captureY,0,0,$bitmap.Size)
 $bitmap.Save((Join-Path $root 'ArtReview/settings_screenshot.png'))
 $graphics.Dispose();$bitmap.Dispose()
 $saveCondition=New-Object System.Windows.Automation.PropertyCondition ([System.Windows.Automation.AutomationElement]::NameProperty),'Save settings'
 $save=$window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$saveCondition)
 if (!$save) { throw 'Save button missing' }
 $save.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
 Start-Sleep -Milliseconds 500
 if (!(Test-Path $config)) { throw 'Settings were not persisted' }
 Write-Output 'PASS: right-click menu, Settings window, Save, persisted configuration'
} finally {
 $hwnd=[PetWindowTest]::FindWindow([IntPtr]0,'PixelRick')
 if ($hwnd -ne [IntPtr]::Zero) { [void][PetWindowTest]::PostMessage($hwnd,0x10,[IntPtr]0,[IntPtr]0) }
 if (!$process.WaitForExit(5000)) { $process.Kill() }
 [void][PetWindowTest]::SetCursorPos($old.X,$old.Y)
 if ($null -ne $backup) { [IO.File]::WriteAllText($config,$backup) }
}
