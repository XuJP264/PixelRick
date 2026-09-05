"""Build a self-contained bundle/DMG on its native macOS architecture.
Signing credentials are read only from environment variables populated by CI secrets.
"""
import argparse, base64, hashlib, json, os, pathlib, plistlib, shutil, subprocess, tempfile
from PIL import Image
ROOT = pathlib.Path(__file__).resolve().parents[2]
def run(*args, **kwargs):
    result=subprocess.run(list(map(str,args)),check=False,**kwargs)
    # Do not include command arguments in exceptions: signing commands contain
    # passwords even when running outside GitHub's automatic secret masking.
    if result.returncode:raise RuntimeError(f'{pathlib.Path(str(args[0])).name} failed with exit {result.returncode}')
    return result

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--arch',choices=['arm64','x64'],required=True);args=parser.parse_args()
    arch=args.arch; native='x86_64' if arch=='x64' else 'arm64'
    assert os.uname().sysname=='Darwin' and os.uname().machine==native,'Use a native architecture macOS runner'
    version=json.loads((ROOT/'version.json').read_text())['version']; dest=ROOT/'dist'/f'macos-{arch}';dest.mkdir(parents=True,exist_ok=True)
    publish=dest/'publish'
    run('dotnet','publish',ROOT/'src/PixelRick.Desktop/PixelRick.Desktop.csproj','-c','Release','-r',f'osx-{arch}','--self-contained','true','-p:DebugType=None','-p:DebugSymbols=false','-o',publish)
    run('clang','-dynamiclib','-fobjc-arc','-arch',native,'-mmacosx-version-min=13.0','-framework','AppKit','-framework','ServiceManagement','-framework','ApplicationServices',ROOT/'native/macos/PixelRickMac.m','-o',publish/'libPixelRickMac.dylib')
    stage=dest/'image';app=stage/'PixelRick.app';contents=app/'Contents'
    if stage.exists():shutil.rmtree(stage)
    (contents/'Resources').mkdir(parents=True);shutil.copytree(publish,contents/'MacOS')
    for doc in ['LICENSE','ARTWORK-NOTICE.md']:shutil.copy(ROOT/doc,contents/'Resources'/doc)
    plist={'CFBundleIdentifier':'org.pixelrick.desktop','CFBundleName':'PixelRick','CFBundleDisplayName':'PixelRick','CFBundleExecutable':'PixelRick.Desktop','CFBundlePackageType':'APPL','CFBundleInfoDictionaryVersion':'6.0','CFBundleVersion':version,'CFBundleShortVersionString':version,'CFBundleIconFile':'PixelRick.icns','LSMinimumSystemVersion':'13.0','LSUIElement':True,'NSHighResolutionCapable':True,'NSSupportsAutomaticGraphicsSwitching':True}
    with (contents/'Info.plist').open('wb') as f:plistlib.dump(plist,f)
    icons=dest/'PixelRick.iconset';icons.mkdir(exist_ok=True)
    with Image.open(ROOT/'Assets/app.ico') as icon:
        image=icon.convert('RGBA')
        for size in [16,32,128,256,512]:
            for factor in [1,2]:image.resize((size*factor,size*factor),Image.Resampling.NEAREST).save(icons/f'icon_{size}x{size}{"@2x" if factor==2 else ""}.png')
    run('iconutil','-c','icns',icons,'-o',contents/'Resources/PixelRick.icns')
    (contents/'MacOS/PixelRick.Desktop').chmod(0o755)
    # Apple treats MacOS as a code directory. Put managed DLLs/data in Resources
    # and native libraries in Frameworks; relative links preserve .NET probing.
    payload=contents/'Resources/payload';payload.mkdir()
    frameworks=contents/'Frameworks';frameworks.mkdir()
    for path in list((contents/'MacOS').iterdir()):
        if path.name=='PixelRick.Desktop':continue
        macho=path.is_file() and 'Mach-O' in subprocess.check_output(['file','-b',str(path)],text=True)
        target=(frameworks if macho else payload)/path.name
        shutil.move(path,target)
        path.symlink_to(os.path.relpath(target,path.parent),target_is_directory=target.is_dir())
        # apphost resolves the managed entry DLL's symlink and uses payload as
        # AppContext.BaseDirectory, including for hostpolicy and P/Invoke probing.
        if macho:(payload/path.name).symlink_to(os.path.relpath(target,payload))
    credentials=['MACOS_CERTIFICATE_BASE64','MACOS_CERTIFICATE_PASSWORD','MACOS_SIGNING_IDENTITY','APPLE_ID','APPLE_TEAM_ID','APPLE_APP_PASSWORD']
    present=[bool(os.environ.get(name)) for name in credentials]
    if any(present) and not all(present):raise RuntimeError('Partial signing configuration: provide all documented secrets or none')
    signed=all(present);identity=os.environ['MACOS_SIGNING_IDENTITY'] if signed else '-';keychain=None
    try:
        if signed:
            import secrets
            keychain=dest/'signing.keychain-db';password=secrets.token_hex(24)
            cert=dest/'certificate.p12';cert.write_bytes(base64.b64decode(os.environ['MACOS_CERTIFICATE_BASE64']))
            run('security','create-keychain','-p',password,keychain)
            run('security','set-keychain-settings','-lut','21600',keychain)
            run('security','unlock-keychain','-p',password,keychain)
            run('security','import',cert,'-k',keychain,'-P',os.environ['MACOS_CERTIFICATE_PASSWORD'],'-T','/usr/bin/codesign')
            run('security','set-key-partition-list','-S','apple-tool:,apple:','-k',password,keychain)
            cert.unlink()
        signargs=['--force','--sign',identity]
        if signed:signargs+=['--keychain',str(keychain),'--options','runtime','--timestamp','--entitlements',str(ROOT/'packaging/macos/entitlements.plist')]
        for path in sorted(frameworks.rglob('*')):
            if path.is_file():run('codesign',*signargs,path)
        run('codesign',*signargs,app);run('codesign','--verify','--deep','--strict','--verbose=2',app)
        os.symlink('/Applications',stage/'Applications')
        if not signed:
            (stage/'READ ME - Development build.txt').write_text('PixelRick development preview\n\nDrag PixelRick.app to Applications, then launch it.\nThis build has an ad-hoc integrity signature, not an Apple Developer ID signature or notarization.\nIf macOS blocks launch, open System Settings > Privacy & Security and use Open Anyway after reviewing the download source. No terminal or .NET installation is needed.\n')
        dmg=dest/f'PixelRick-macOS-{arch}.dmg'
        if dmg.exists():dmg.unlink()
        run('hdiutil','create','-volname','PixelRick','-srcfolder',stage,'-ov','-format','UDZO',dmg)
        if signed:
            run('codesign','--force','--sign',identity,'--keychain',keychain,'--timestamp',dmg)
            run('xcrun','notarytool','submit',dmg,'--apple-id',os.environ['APPLE_ID'],'--team-id',os.environ['APPLE_TEAM_ID'],'--password',os.environ['APPLE_APP_PASSWORD'],'--wait','--output-format','json',stdout=(dest/'notarization.json').open('w'))
            result=json.loads((dest/'notarization.json').read_text());assert result['status']=='Accepted',result['status']
            run('xcrun','stapler','staple',app);run('xcrun','stapler','validate',app)
            # Recreate the image with the stapled app, then notarize/staple the final image too.
            dmg.unlink();run('hdiutil','create','-volname','PixelRick','-srcfolder',stage,'-ov','-format','UDZO',dmg)
            run('codesign','--force','--sign',identity,'--keychain',keychain,'--timestamp',dmg)
            run('xcrun','notarytool','submit',dmg,'--apple-id',os.environ['APPLE_ID'],'--team-id',os.environ['APPLE_TEAM_ID'],'--password',os.environ['APPLE_APP_PASSWORD'],'--wait','--output-format','json',stdout=(dest/'notarization-final.json').open('w'))
            assert json.loads((dest/'notarization-final.json').read_text())['status']=='Accepted'
            run('xcrun','stapler','staple',dmg);run('xcrun','stapler','validate',dmg)
            run('spctl','--assess','--type','execute','--verbose=2',app)
            run('spctl','--assess','--type','open','--context','context:primary-signature','--verbose=2',dmg)
        run('hdiutil','verify',dmg)
        (dest/'package-report.json').write_text(json.dumps({'version':version,'architecture':arch,'developerIdSigned':signed,'notarized':signed,'quality':'signed' if signed else 'unsigned-development','dmg':dmg.name,'sha256':hashlib.sha256(dmg.read_bytes()).hexdigest()},indent=2))
        print('PACKAGE READY:',dmg)
    finally:
        if keychain and keychain.exists():subprocess.run(['security','delete-keychain',str(keychain)],check=False)
        cert=dest/'certificate.p12'
        if cert.exists():cert.unlink()
if __name__=='__main__':main()
