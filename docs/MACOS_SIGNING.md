# macOS release signing

The packaging script runs only on macOS, on a runner matching the architecture.
No Mac App Store distribution or proprietary Avalonia tooling is used.

Without credentials, it applies only an **ad-hoc integrity signature**, packages
the DMG, and marks the report `unsigned-development`. This is not a Developer ID
signature and is not notarization. Such artifacts must be published as prereleases.
The accepted v0.1.0 Windows release remains available and unchanged.

To enable production signing, configure these GitHub Actions **repository secrets**:

| Secret | Value |
| --- | --- |
| `MACOS_CERTIFICATE_BASE64` | Base64-encoded exported Developer ID Application P12 |
| `MACOS_CERTIFICATE_PASSWORD` | P12 export password |
| `MACOS_SIGNING_IDENTITY` | Full Developer ID Application identity |
| `APPLE_ID` | Apple developer account email |
| `APPLE_TEAM_ID` | Apple team identifier |
| `APPLE_APP_PASSWORD` | App-specific password for notarytool |

All six must be present together. Partial configuration fails rather than silently
falling back to unsigned output. Values are read from the job environment; never
place them in files committed to Git. The temporary P12 and keychain are removed
in `finally`. GitHub masks secret values in job logs.

The native macOS job imports the certificate into a temporary keychain, signs
each Mach-O binary and then the bundle with hardened runtime and the .NET JIT
entitlement, verifies the signature, creates and signs the DMG, submits it with
`xcrun notarytool --wait`, staples the app, rebuilds the final DMG with that ticket,
notarizes and staples the final DMG, and verifies both with `spctl` and `stapler`.
Any failure stops publication. The notarization path cannot be end-to-end tested
until valid Apple credentials are supplied.

Unsigned preview installation uses ordinary macOS UI: open the DMG, drag the app
to Applications, launch, and if blocked use System Settings → Privacy & Security
→ Open Anyway after verifying the download source. No terminal is required.
