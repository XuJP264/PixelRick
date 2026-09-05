"""Launch the real Windows app, capture it, and require a clean timed exit."""
import os, subprocess, time, pathlib, sys
from PIL import ImageGrab
root=pathlib.Path(__file__).resolve().parents[1]
sdk=pathlib.Path(os.environ['LOCALAPPDATA'])/'PixelRickBuild/dotnet'
env=os.environ.copy();env['DOTNET_ROOT']=str(sdk)
exe=pathlib.Path(sys.argv[1]) if len(sys.argv)>1 else root/'src/PixelRick/bin/Debug/net8.0-windows/PixelRick.exe'
p=subprocess.Popen([str(exe),'--smoke-test','--capture',str(root/'ArtReview/app_screenshot.png')],env=env)
time.sleep(3)
assert p.poll() is None, f'Application exited early: {p.returncode}'
import ctypes
from ctypes import wintypes
user32=ctypes.windll.user32
hwnd=user32.FindWindowW(None,'PixelRick');rect=wintypes.RECT()
assert hwnd and user32.GetWindowRect(hwnd,ctypes.byref(rect)), 'Missing pet window'
# Capture only the app's own transparent render for public review. Never publish desktop contents.
assert p.wait(timeout=20)==0,'Application crashed'
log=pathlib.Path(os.environ['APPDATA'])/'PixelRick/diagnostics.log'
assert 'SMOKE PASS' in log.read_text(),'Missing smoke completion'
print('PASS: real WPF window launched, rendered, and exited cleanly')
