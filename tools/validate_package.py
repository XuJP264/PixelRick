import json,sys,pathlib
from PIL import Image
folder=pathlib.Path(sys.argv[1])
for filename in ['PixelRick.exe','PixelRick.dll','PixelRick.runtimeconfig.json','coreclr.dll','hostfxr.dll','PresentationFramework.dll','Assets/app.ico']:
    assert (folder/filename).is_file(),f'Missing {filename}'
metadata=json.loads((folder/'Assets/Character/Rick/animations.json').read_text())
assert len(metadata)==25
for name,m in metadata.items():
    with Image.open(folder/'Assets'/m['file']) as im:
        assert im.mode=='RGBA' and im.size==(128*m['frames'],128),name
assert not list(folder.rglob('*reference*')),'Source art leaked into runtime'
print(f'PASS: self-contained runtime and {len(metadata)} sprite sheets')
