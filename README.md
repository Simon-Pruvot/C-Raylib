# Yfight

2-player fighting game built with Raylib (C# / .NET 10).

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

## How to run

```
dotnet run
```

Run that command from the project folder (`C-Raylib/`). The game window opens immediately.

---

## Controls

| Action | Key |
|---|---|
| Move left / right | Arrow Left / Right |
| Jump | Arrow Up |
| Climb ladder | Arrow Up (while on ladder) |
| Aim | Mouse |
| Shoot / Use item | Left click |
| Pick up chest | E |
| Switch slot | 1 / 2 / 3 |
| Fullscreen | F11 |

**Local mode:** P1 uses the controls above. P2 uses WASD (+ W to jump/climb). Only P1 has a gun in local mode.

**Online mode:** Both players use the controls above on their own machines. Both have a gun.

---

## Playing online (same WiFi)

> Both machines must be on the **same WiFi network**. Do **not** test with two windows on the same PC — they share the keyboard.

1. **Host** — one player clicks `Jouer en ligne → Héberger`. Their local IP is shown on screen (e.g. `192.168.1.42`).
2. **Join** — the other player clicks `Jouer en ligne → Rejoindre`, types that IP, then clicks `Connecter`.
3. The game starts automatically once both sides connect.

If it stays stuck on "Connexion en cours":
- Make sure the **host launched the game first** and is on the Héberger waiting screen.
- Check Windows Firewall — allow the game on **UDP port 7777** (or turn off the firewall temporarily).
- Double-check the IP: use the one shown on the host's screen, not one you guessed.

---

## Project structure

| File | Role |
|---|---|
| `Program.cs` | Main loop, game states, input routing |
| `PlayerLogic.cs` | `UpdatePlayer` — movement, physics, animation |
| `GameHelpers.cs` | Draw helpers, map loading, input helpers, local IP |
| `GameTypes.cs` | Structs and enums (`PlayerInput`, `Bullet`, `GameState`, …) |
| `NetManager.cs` | UDP host/join, handshake, per-frame send/receive |
| `ONLINE.md` | Detailed explanation of how the networking was implemented |
