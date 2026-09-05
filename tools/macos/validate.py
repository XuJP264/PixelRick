"""Run the installed .app from a mounted DMG, require UI evidence and normal exits."""
import argparse,json,pathlib,plistlib,subprocess,shutil,os,hashlib
from PIL import Image
ROOT=pathlib.Path(__file__).resolve().parents[2]
def main():
    p=argparse.ArgumentParser();p.add_argument('--arch',required=True);args=p.parse_args()
    dest=ROOT/'dist'/f'macos-{args.arch}';dmg=dest/f'PixelRick-macOS-{args.arch}.dmg';mount=dest/'mounted';mount.mkdir(exist_ok=True)
    subprocess.run(['hdiutil','attach','-nobrowse','-readonly','-mountpoint',str(mount),str(dmg)],check=True)
    try:
        app=dest/'installed/PixelRick.app';app.parent.mkdir(exist_ok=True)
        if app.exists():shutil.rmtree(app)
        subprocess.run(['ditto',str(mount/'PixelRick.app'),str(app)],check=True)
        assert (mount/'Applications').is_symlink()
    finally:subprocess.run(['hdiutil','detach',str(mount)],check=True)
    contents=app/'Contents';plist=plistlib.loads((contents/'Info.plist').read_bytes());assert plist['NSHighResolutionCapable'] and plist['LSUIElement']
    subprocess.run(['codesign','--verify','--deep','--strict',str(app)],check=True)
    exe=contents/'MacOS/PixelRick.Desktop'
    architecture=subprocess.check_output(['lipo','-archs',str(exe)],text=True).strip();assert architecture==('arm64' if args.arch=='arm64' else 'x86_64')
    assert (contents/'MacOS/libcoreclr.dylib').exists();assert (contents/'MacOS/libPixelRickMac.dylib').exists()
    metadata=json.loads((contents/'MacOS/Assets/Character/Rick/animations.json').read_text())
    for a in metadata.values():assert hashlib.sha256((contents/'MacOS/Assets'/a['file']).read_bytes()).digest()==hashlib.sha256((ROOT/'Assets'/a['file']).read_bytes()).digest()
    review=dest/'review';review.mkdir(exist_ok=True)
    result=subprocess.run([str(exe),'--self-test','--review-dir',str(review)],timeout=100,stdout=(review/'stdout.log').open('w'),stderr=(review/'stderr.log').open('w'))
    if (review/'failure.txt').exists():print((review/'failure.txt').read_text())
    if result.returncode:print((review/'stderr.log').read_text())
    if (review/'macos-report.json').exists():print((review/'macos-report.json').read_text())
    assert result.returncode==0,f'App validation exit {result.returncode}'
    report=json.loads((review/'macos-report.json').read_text());assert report['passed']
    # Repeat launch and clean quit using the same bundle. The test itself persists/reloads config.
    second=dest/'restart-review'
    subprocess.run([str(exe),'--self-test','--review-dir',str(second)],check=True,timeout=100,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
    frames=[]
    for path in sorted((review/'frames').glob('*.png')):
        with Image.open(path) as im:
            bg=Image.new('RGB',im.size,'#182130');bg.paste(im,mask=im.getchannel('A'));frames.append(bg)
    assert frames
    frames[0].save(review/'macos-behavior-demo.gif',save_all=True,append_images=frames[1:],duration=200,loop=0)
    with Image.open(review/'macos-1x.png') as a, Image.open(review/'macos-2x.png') as b:
        assert b.size==(a.width*2,a.height*2)
        assert a.getbbox() and b.getbbox()
    (review/'package-validation.json').write_text(json.dumps({'dmgMount':True,'applicationCopy':True,'architecture':architecture,'selfContained':True,'assetHashes':True,'quitRestart':True,'renderScales':[1,2]},indent=2))
    print('MACOS VALIDATION PASS',args.arch)
if __name__=='__main__':main()
