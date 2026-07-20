# Code signing

Flowtype releases are currently **unsigned**. Windows SmartScreen may show "Windows protected your PC" on first install.

## What you need

1. An Authenticode code-signing certificate (standard or EV)
2. `signtool.exe` from the Windows SDK
3. A secure build machine with access to the certificate

## Signing command (after certificate is installed)

```powershell
signtool sign /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 /a Flowtype.exe
signtool sign /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 /a installer\*.ps1
```

Sign `Flowtype.exe` inside the release ZIP before distribution. EV certificates reduce SmartScreen friction faster than standard certs.

## Current status

- **Release blocker for public distribution:** yes
- **Release blocker for private trusted-friends distribution:** no, with documented SmartScreen bypass (More info → Run anyway)
