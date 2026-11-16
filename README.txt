WORST - Account Manager (PC)
============================

A Windows app (like Roblox Account Manager) that uses YOUR Account Bank.
Same idea as RAM, your colors, your accounts.

WHAT IT DOES (v1)
- Log in to your bank with Discord (same as W-BOARD).
- Lists your saved accounts with Active / Dead status.
- Tick accounts, enter a Game ID, hit "Launch selected" -> each one opens
  Roblox straight into that game (no password, no captcha).
- "Allow multiple Roblox" keeps the single-instance lock held so many
  clients run at once.
- Double-click an account to launch just that one.

BUILD (one time)
1. Install the .NET 8 SDK:  https://dotnet.microsoft.com/download/dotnet/8.0
2. Double-click build.bat
3. It produces:  publish\WORST.exe   (single file, no install needed)

RUN
- Run publish\WORST.exe
- Click Login -> approve in Discord. If the window doesn't come back on its
  own, click Login again and use "Paste code" with the code/link from the page.
- Enter your VIP key in the "VIP key" box (used to pull cookies from the vault;
  leave blank only if your bank is in FREE MODE).
- Enter a Game ID, tick accounts, Launch.

NOTES
- Needs Roblox already installed on the PC (it hands off to the Roblox Player).
- A cookie that's dead or logging in from a far-off location won't launch -
  same cookie-death rule as the phones.
- Settings are saved to %APPDATA%\WORST\settings.json

NOT IN v1 YET (say the word):
- Auto-execute scripts, auto-relaunch/watcher, add/import accounts,
  presence, server picker.
