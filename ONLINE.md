# How we added LAN multiplayer

## The problem

The original game ran everything on one machine — both players shared the same keyboard (P1 = Arrows, P2 = WASD) and the same game loop. To play over WiFi we need each player on their own machine, which means two separate programs talking to each other.

## The approach: input synchronisation

Instead of sending the full game state every frame (heavy, complex), we send only the **inputs** — the keystrokes and mouse position. Both machines run the **exact same game simulation** independently. Since they start from the same state and receive the same inputs, they always produce the same result.

This works perfectly on LAN where latency is under 1 ms.

## What changed

### `GameTypes.cs`
Added a `PlayerInput` struct — 13 bytes that describe everything one player did in a single frame: move left/right, climb, jump, action (shoot/use item), pickup, mouse position, and selected slot.

Added `NetworkMode` (`Local`, `Host`, `Client`) and two new `GameState` values (`Lobby`, `Connecting`).

### `PlayerLogic.cs`
`UpdatePlayer` used to read the keyboard directly (`Raylib.IsKeyDown`). It now accepts a `PlayerInput` struct instead, so it works identically whether the input came from the local keyboard or arrived over the network.

### `NetManager.cs` *(new file)*
A thin wrapper around .NET's `UdpClient`.

- **Host** binds to port `7777` and waits.
- **Client** sends a one-byte hello (`0xAB`) to the host's IP every frame until the host responds.
- **Handshake**: once the host receives the hello it generates a random seed, sends it back as 4 bytes, and marks itself connected. The client receives the seed and marks itself connected.
- **Every frame**: each side serialises its `PlayerInput` into 13 bytes and fires it via UDP. `TryReceiveInput` is non-blocking — if no packet arrived this frame the last received input is reused (on LAN this almost never happens).

### `GameHelpers.cs`
Three small helpers added: `GatherInput` (builds a `PlayerInput` from raw booleans + mouse), `HandleTextInput` (Raylib character polling for the IP text field), and `GetLocalIp` (finds the machine's LAN IPv4 address to display on the host screen).

### `Program.cs`
Two new screens:

- **Lobby** — choose "Héberger" (host) or "Rejoindre" (join). Joining reveals an IP text field.
- **Connecting** — host shows its local IP and waits; client shows "Connexion en cours…". Both poll `NetManager` every frame. The moment the handshake completes both sides seed their `Random` identically and call `ResetGame` — the simulation is now in sync.

During gameplay the loop is:
1. Read local keyboard/mouse → `PlayerInput`
2. Send it via UDP
3. Try to receive the remote `PlayerInput` (non-blocking)
4. Host feeds its own input as P1, remote as P2. Client does the opposite.
5. Both machines call the same `UpdatePlayer`, bullet, and pickup logic → identical result.

The **aim direction** for P1's gun uses `p1Input.MouseX/Y` on both machines, so bullet trajectories are identical everywhere.

## How to play

1. Both players must be on the **same WiFi network**.
2. One player launches the game, clicks **Jouer en ligne → Héberger**. Their local IP is shown on screen (e.g. `192.168.1.42`).
3. The other player launches the game, clicks **Jouer en ligne → Rejoindre**, types that IP, and clicks **Connecter**.
4. The game starts automatically once both sides handshake.
5. Controls on each machine: **Arrow keys** to move/jump/climb, **Mouse** to aim, **Left click** to shoot or use item, **E** to open chests, **1/2/3** to switch slots.

> The VPS is not needed for LAN play. If you ever want to play over the internet, a simple UDP relay on the VPS (forwarding packets between two clients) would remove the need to be on the same network.
