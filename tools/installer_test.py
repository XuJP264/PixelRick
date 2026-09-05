"""Install, verify, upgrade, uninstall, verify preference preservation, reinstall."""
import pathlib,os,subprocess,hashlib,json,sys
root=pathlib.Path(__file__).resolve().parents[1]
installer=root/'dist/PixelRick-Setup.exe'
target=pathlib.Path(os.environ['LOCALAPPDATA'])/'Programs/PixelRick'
config=pathlib.Path(os.environ['APPDATA'])/'PixelRick/config.json'
if target.exists():raise SystemExit('Refusing to alter an existing installation; test on a clean profile.')
def run_install():
    subprocess.run([str(installer),'/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/SP-'],check=True,timeout=90)
def validate():
    subprocess.run([sys.executable,str(root/'tools/validate_package.py'),str(target)],check=True)
    for src in (root/'dist/publish').rglob('*'):
        if src.is_file():assert hashlib.sha256(src.read_bytes()).digest()==hashlib.sha256((target/src.relative_to(root/'dist/publish')).read_bytes()).digest(),src.name
    shortcut=pathlib.Path(os.environ['APPDATA'])/'Microsoft/Windows/Start Menu/Programs/PixelRick/PixelRick.lnk'
    assert shortcut.exists(),'Missing Start Menu shortcut'
run_install();validate()
subprocess.run([sys.executable,str(root/'tools/smoke_test.py'),str(target/'PixelRick.exe')],check=True)
run_install();validate()
before=config.read_bytes()
subprocess.run([str(target/'unins000.exe'),'/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART'],check=True,timeout=60)
assert not (target/'PixelRick.exe').exists(),'Uninstall left executable'
assert config.read_bytes()==before,'Uninstall changed preferences'
run_install();validate()
report={'passed':True,'install':True,'upgrade':True,'uninstall':True,'preferencesPreserved':True,'startMenuShortcut':True,'runtimeFilesMatch':True,'finalInstallation':str(target)}
(root/'dist/installer_validation.json').write_text(json.dumps(report,indent=2))
print('PASS: install, byte-for-byte contents, launch, upgrade, uninstall, preferences, reinstall')
