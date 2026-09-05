"""Native mouse-message test against the real WPF window; restores cursor/config."""
import ctypes,os,pathlib,subprocess,time,json,sys
from ctypes import wintypes
root=pathlib.Path(__file__).resolve().parents[1];u=ctypes.windll.user32
u.SetProcessDpiAwarenessContext(ctypes.c_void_p(-4))
u.FindWindowW.restype=wintypes.HWND
u.SendMessageW.argtypes=[wintypes.HWND,wintypes.UINT,wintypes.WPARAM,wintypes.LPARAM]
folder=pathlib.Path(os.environ['APPDATA'])/'PixelRick';folder.mkdir(exist_ok=True)
config=folder/'config.json';backup=config.read_bytes() if config.exists() else None
log=folder/'diagnostics.log';start=log.stat().st_size if log.exists() else 0
old=wintypes.POINT();u.GetCursorPos(ctypes.byref(old));p=None
try:
    config.write_text(json.dumps({'Scale':2,'Wander':False,'InteractionFrequency':0,'RememberPosition':False,'AlwaysOnTop':True}))
    env=os.environ.copy();env['DOTNET_ROOT']=str(pathlib.Path(os.environ['LOCALAPPDATA'])/'PixelRickBuild/dotnet')
    exe=pathlib.Path(sys.argv[1]) if len(sys.argv)>1 else root/'src/PixelRick/bin/Debug/net8.0-windows/PixelRick.exe'
    p=subprocess.Popen([str(exe),'--diagnostics'],env=env);time.sleep(2)
    h=u.FindWindowW(None,'PixelRick');assert h,'No window'
    def rect():
        r=wintypes.RECT();u.GetWindowRect(h,ctypes.byref(r));return r
    def message(msg,x=128,y=140,flags=0):u.SendMessageW(h,msg,flags,(y<<16)|(x&65535))
    r=rect();u.SetCursorPos(r.left+128,r.top+140)
    message(0x201,flags=1);message(0x202);time.sleep(.8)
    assert 'state=' in log.read_text()[start:],'Click did not trigger a reaction'
    # Drag 220 physical pixels up, pause to release without a throw, then land.
    time.sleep(1.8);r=rect();u.SetCursorPos(r.left+128,r.top+140);message(0x201,flags=1)
    for i in range(1,12):
        u.SetCursorPos(r.left+128,r.top+140-i*20);message(0x200,128,140-i*20,1);time.sleep(.03)
    time.sleep(.2);message(0x202);time.sleep(2)
    content=log.read_text()[start:]
    for state in ['Grabbed','Falling','Landing']:assert f'state={state}' in content,f'Missing {state}'
    r=rect();u.SetCursorPos(r.left+128,r.top+140)
    message(0x201,flags=1);message(0x202);time.sleep(.08);message(0x203,flags=1);message(0x202);time.sleep(3.3)
    content=log.read_text()[start:]
    for state in ['PortalEnter','PortalExit']:assert f'state={state}' in content,f'Missing {state}'
    # A second process must forward Show and exit rather than create another pet.
    second=subprocess.run([str(exe)],env=env,timeout=5);assert second.returncode==0
    assert p.poll() is None,'Pet crashed'
    u.PostMessageW(h,0x10,0,0);assert p.wait(timeout=5)==0
    print('PASS: click, native drag, fall, landing, double-click portals, single instance')
finally:
    if p and p.poll() is None:
        h=u.FindWindowW(None,'PixelRick')
        if h:u.PostMessageW(h,0x10,0,0)
        try:p.wait(timeout=4)
        except subprocess.TimeoutExpired:p.terminate()
    u.SetCursorPos(old.x,old.y)
    if backup is not None:config.write_bytes(backup)
    elif config.exists():config.unlink()
