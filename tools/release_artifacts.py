"""Collect only tested artifacts; stable macOS releases require notarization."""
import argparse, hashlib, json, os, pathlib, re, shutil, subprocess, zipfile

ROOT = pathlib.Path(__file__).resolve().parents[1]

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--check-version', action='store_true')
    parser.add_argument('--publish', action='store_true')
    args = parser.parse_args()
    tag = os.environ['RELEASE_TAG']
    version = json.loads((ROOT / 'version.json').read_text())['version']
    assert re.fullmatch(r'v' + re.escape(version) + r'(?:-preview\.[1-9][0-9]*)?', tag), 'Tag and version.json differ'
    if args.check_version:
        return
    preview = '-preview.' in tag
    source = ROOT / 'downloaded'
    output = ROOT / 'dist/release'
    output.mkdir(parents=True, exist_ok=True)
    artifacts = []
    def copy(name):
        matches = list(source.rglob(name))
        assert len(matches) == 1, f'Expected one {name}, found {len(matches)}'
        target = output / name
        shutil.copy2(matches[0], target)
        artifacts.append(target)
        return target
    for name in ['PixelRick-Setup.exe', 'PixelRick-Portable.zip']:
        copy(name)
    with zipfile.ZipFile(output / 'PixelRick-Portable.zip') as archive:
        assert archive.testzip() is None
        assert 'PixelRick.exe' in archive.namelist()
    for arch in ['arm64', 'x64']:
        folder = source / f'macOS-{arch}-review'
        report_path = next(folder.rglob('package-report.json'))
        report = json.loads(report_path.read_text())
        assert report['architecture'] == arch and report['version'] == version
        if not preview:
            assert report['developerIdSigned'] and report['notarized'], 'Unsigned macOS builds require a preview tag'
        reports = list(folder.rglob('macos-report.json'))
        assert len(reports) == 2 and all(json.loads(p.read_text())['passed'] for p in reports), 'Both launch and restart must pass'
        assert list(folder.rglob('package-validation.json')), 'Missing installed DMG validation'
        dmg = copy(f'PixelRick-macOS-{arch}.dmg')
        assert hashlib.sha256(dmg.read_bytes()).hexdigest() == report['sha256']
    evidence = output / 'PixelRick-Validation.zip'
    with zipfile.ZipFile(evidence, 'w', zipfile.ZIP_DEFLATED) as archive:
        for path in source.rglob('*'):
            if path.is_file() and path.suffix in ['.json', '.png', '.gif', '.trx'] and 'frames' not in path.parts:
                archive.write(path, path.relative_to(source))
    artifacts.append(evidence)
    checksums = output / 'SHA256SUMS.txt'
    checksums.write_text(''.join(f'{hashlib.sha256(p.read_bytes()).hexdigest()}  {p.name}\n' for p in artifacts))
    artifacts.append(checksums)
    if args.publish:
        command = ['gh', 'release', 'create', tag, *map(str, artifacts), '--verify-tag', '--title', f'PixelRick {tag}', '--notes-file', str(ROOT / 'RELEASE_NOTES.md')]
        if preview:
            command += ['--prerelease', '--latest=false']
        subprocess.run(command, cwd=ROOT, check=True)
    print('Verified release artifacts:', ', '.join(p.name for p in artifacts))

if __name__ == '__main__':
    main()
