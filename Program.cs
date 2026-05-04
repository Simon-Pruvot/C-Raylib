using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

partial class Program
{
    static void Main()
    {
        int virtualWidth  = 1200;
        int virtualHeight = 800;

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(virtualWidth, virtualHeight, "Yfight");
        Raylib.SetTargetFPS(120);

        RenderTexture2D renderTarget = Raylib.LoadRenderTexture(virtualWidth, virtualHeight);
        Raylib.SetTextureFilter(renderTarget.Texture, TextureFilter.Bilinear);

        Texture2D[] runAnimRight = { Raylib.LoadTexture("imgs/perso-droite-1.png"), Raylib.LoadTexture("imgs/perso-droite-2.png") };
        Texture2D[] runAnimLeft  = { Raylib.LoadTexture("imgs/perso-gauche-1.png"), Raylib.LoadTexture("imgs/perso-gauche-2.png") };
        Texture2D[] climbAnim    = { Raylib.LoadTexture("imgs/perso-monte-1.png"),  Raylib.LoadTexture("imgs/perso-monte-2.png")  };

        Texture2D gunRight           = Raylib.LoadTexture("imgs/gun-droite.png");
        Texture2D gunLeft            = Raylib.LoadTexture("imgs/gun-gauche.png");
        Texture2D healPotionTexture  = Raylib.LoadTexture("imgs/heal-potion.png");
        Texture2D shieldPotionTexture = Raylib.LoadTexture("imgs/shild-potion.png");
        Texture2D nucPotionTexture   = Raylib.LoadTexture("imgs/nuc-potion.png");
        Texture2D shieldTexture      = Raylib.LoadTexture("imgs/shild.png");
        Texture2D crownTexture       = Raylib.LoadTexture("imgs/crown.png");
        Texture2D chestClosedTexture = Raylib.LoadTexture("imgs/coffre-fermé-1.png");
        Texture2D chestOpenTexture   = Raylib.LoadTexture("imgs/coffre-ouvert-1.png");

        // ── Game state ──────────────────────────────────────────────────────
        int currentMap = 0;
        LoadMap(currentMap, out Rectangle[] solidBlocks, out Rectangle[] ladders,
                out Rectangle player1Start, out Rectangle player2Start);
        Rectangle player1 = player1Start;
        Rectangle player2 = player2Start;

        float speed = 200f, gravity = 1000f, jump = -600f, climbSpeed = -150f;
        float yVel1 = 0f, yVel2 = 0f;
        bool onGround1 = false, onGround2 = false;
        bool onLadder1 = false, onLadder2 = false;
        bool facingRight1 = true, facingRight2 = true;
        int currentFrame1 = 0, currentFrame2 = 0;
        float animTimer1 = 0f, animTimer2 = 0f, animSpeed = 0.2f;

        int maxHealth = 100, player1Health = 100, player2Health = 100;
        int maxShield = 20,  player1Shield = 0;
        float gunLength = 70f, gunHeight = 24f;
        int gunDamage = 20;
        float shotCooldown = 0.2f, shotTimer = 0f;
        float bulletSpeed = 900f, bulletRadius = 4f, bulletLife = 2f;
        float chestSize = 48f, pickupGravity = 600f;

        List<Bullet> bullets = new();
        List<Pickup> pickups = new();
        System.Random rng = new();
        ItemType[] inventory   = { ItemType.Gun, ItemType.None, ItemType.None };
        int selectedSlot = 0;

        // ── Network state ───────────────────────────────────────────────────
        NetworkMode networkMode      = NetworkMode.Local;
        NetManager? netManager       = null;
        PlayerInput lastRemoteInput  = default;
        string ipInputText           = "";
        bool joinInputMode           = false;
        string localIp               = "";

        // ── UI state ────────────────────────────────────────────────────────
        GameState gameState   = GameState.Menu;
        bool exitRequested    = false;
        string winnerText     = string.Empty;
        int menuFrame         = 0;
        float menuTimer       = 0f;

        // Button layout
        Rectangle playButton    = new(500, 450, 200, 60);
        Rectangle onlineButton  = new(500, 530, 200, 60);
        Rectangle hostButton    = new(500, 370, 200, 60);
        Rectangle joinButton    = new(500, 450, 200, 60);
        Rectangle backButton    = new(500, 610, 200, 60);
        Rectangle ipInputBox    = new(350, 450, 500, 50);
        Rectangle connectButton = new(500, 520, 200, 60);
        Rectangle replayButton  = new(500, 480, 200, 60);
        Rectangle quitButton    = new(500, 560, 200, 60);

        // ── Main loop ───────────────────────────────────────────────────────
        while (!Raylib.WindowShouldClose() && !exitRequested)
        {
            float deltaTime = Raylib.GetFrameTime();

            if (Raylib.IsKeyPressed(KeyboardKey.F11))
                Raylib.ToggleFullscreen();

            GetRenderScale(virtualWidth, virtualHeight, out float scale, out Vector2 offset);
            Vector2 mouse = GetVirtualMousePosition(scale, offset);

            // ═══════════════════════════════════════════════════════════════
            // MENU
            // ═══════════════════════════════════════════════════════════════
            if (gameState == GameState.Menu)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed) { menuFrame = (menuFrame + 1) % 2; menuTimer = 0f; }

                if (Raylib.CheckCollisionPointRec(mouse, playButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    networkMode = NetworkMode.Local;
                    currentMap  = 0;
                    LoadMap(currentMap, out solidBlocks, out ladders, out player1Start, out player2Start);
                    rng = new System.Random();
                    ResetGame(ref player1, ref player2, player1Start, player2Start,
                        ref yVel1, ref yVel2, ref onGround1, ref onGround2, ref onLadder1, ref onLadder2,
                        ref facingRight1, ref facingRight2, ref currentFrame1, ref currentFrame2,
                        ref animTimer1, ref animTimer2, ref player1Health, ref player2Health, maxHealth,
                        ref player1Shield, maxShield, inventory, ref selectedSlot, ref shotTimer,
                        bullets, pickups, rng, virtualWidth, chestSize);
                    winnerText = string.Empty;
                    gameState  = GameState.Playing;
                }
                else if (Raylib.CheckCollisionPointRec(mouse, onlineButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    joinInputMode = false;
                    ipInputText   = "";
                    gameState     = GameState.Lobby;
                }

                Raylib.BeginTextureMode(renderTarget);
                Raylib.ClearBackground(Color.Black);
                DrawMenuHeader(runAnimRight, menuFrame);
                DrawButton(playButton,   "Jouer (local)",   mouse);
                DrawButton(onlineButton, "Jouer en ligne",  mouse);
                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            // ═══════════════════════════════════════════════════════════════
            // LOBBY  — choose Host or Join
            // ═══════════════════════════════════════════════════════════════
            if (gameState == GameState.Lobby)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed) { menuFrame = (menuFrame + 1) % 2; menuTimer = 0f; }

                if (Raylib.CheckCollisionPointRec(mouse, backButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    joinInputMode = false;
                    ipInputText   = "";
                    gameState     = GameState.Menu;
                }
                else if (!joinInputMode)
                {
                    if (Raylib.CheckCollisionPointRec(mouse, hostButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        networkMode = NetworkMode.Host;
                        netManager?.Close();
                        netManager = new NetManager();
                        netManager.StartHost();
                        localIp   = GetLocalIp();
                        gameState = GameState.Connecting;
                    }
                    else if (Raylib.CheckCollisionPointRec(mouse, joinButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        joinInputMode = true;
                        ipInputText   = "";
                    }
                }
                else
                {
                    // IP text field active
                    HandleTextInput(ref ipInputText, 15, c => c == '.' || (c >= '0' && c <= '9'));

                    if (Raylib.CheckCollisionPointRec(mouse, connectButton)
                        && Raylib.IsMouseButtonPressed(MouseButton.Left)
                        && ipInputText.Length > 0)
                    {
                        networkMode = NetworkMode.Client;
                        netManager?.Close();
                        netManager = new NetManager();
                        netManager.StartClient(ipInputText);
                        gameState = GameState.Connecting;
                    }
                }

                Raylib.BeginTextureMode(renderTarget);
                Raylib.ClearBackground(Color.Black);
                DrawMenuHeader(runAnimRight, menuFrame);
                Raylib.DrawText("Multijoueur LAN", 460, 310, 28, Color.LightGray);

                if (!joinInputMode)
                {
                    DrawButton(hostButton, "Héberger",  mouse);
                    DrawButton(joinButton, "Rejoindre", mouse);
                }
                else
                {
                    Raylib.DrawText("IP de l'hôte :", 350, 415, 22, Color.LightGray);
                    Raylib.DrawRectangleRec(ipInputBox, Color.DarkGray);
                    Raylib.DrawRectangleLinesEx(ipInputBox, 2f, Color.Gray);
                    Raylib.DrawText(ipInputText + "|", (int)ipInputBox.X + 10, (int)ipInputBox.Y + 12, 24, Color.White);
                    DrawButton(connectButton, "Connecter", mouse);
                }

                DrawButton(backButton, "Retour", mouse);
                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            // ═══════════════════════════════════════════════════════════════
            // CONNECTING  — wait for handshake
            // ═══════════════════════════════════════════════════════════════
            if (gameState == GameState.Connecting)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed) { menuFrame = (menuFrame + 1) % 2; menuTimer = 0f; }

                bool connected;
                int  gameSeed;
                connected = networkMode == NetworkMode.Host
                    ? netManager!.HostPoll(out gameSeed)
                    : netManager!.ClientPoll(out gameSeed);

                if (connected)
                {
                    rng = new System.Random(gameSeed);
                    lastRemoteInput = default;
                    currentMap = 0;
                    LoadMap(currentMap, out solidBlocks, out ladders, out player1Start, out player2Start);
                    ResetGame(ref player1, ref player2, player1Start, player2Start,
                        ref yVel1, ref yVel2, ref onGround1, ref onGround2, ref onLadder1, ref onLadder2,
                        ref facingRight1, ref facingRight2, ref currentFrame1, ref currentFrame2,
                        ref animTimer1, ref animTimer2, ref player1Health, ref player2Health, maxHealth,
                        ref player1Shield, maxShield, inventory, ref selectedSlot, ref shotTimer,
                        bullets, pickups, rng, virtualWidth, chestSize);
                    winnerText = string.Empty;
                    gameState  = GameState.Playing;
                    // Don't continue — fall through to render the first playing frame next loop
                    continue;
                }

                Raylib.BeginTextureMode(renderTarget);
                Raylib.ClearBackground(Color.Black);
                DrawMenuHeader(runAnimRight, menuFrame);

                if (networkMode == NetworkMode.Host)
                {
                    Raylib.DrawText("En attente du joueur 2...", 360, 390, 28, Color.White);
                    Raylib.DrawText($"Votre IP : {localIp}", 430, 435, 22, Color.LightGray);
                    Raylib.DrawText("(communiquez cette IP à votre adversaire)", 295, 468, 18, Color.Gray);
                }
                else
                {
                    Raylib.DrawText($"Connexion à {ipInputText}...", 400, 410, 26, Color.White);
                }

                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            // ═══════════════════════════════════════════════════════════════
            // END SCREEN
            // ═══════════════════════════════════════════════════════════════
            if (gameState == GameState.End)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed) { menuFrame = (menuFrame + 1) % 2; menuTimer = 0f; }

                bool hoverReplay = Raylib.CheckCollisionPointRec(mouse, replayButton);
                bool hoverQuit   = Raylib.CheckCollisionPointRec(mouse, quitButton);

                if (hoverReplay && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (networkMode == NetworkMode.Local)
                    {
                        currentMap = (currentMap + 1) % 2;
                        LoadMap(currentMap, out solidBlocks, out ladders, out player1Start, out player2Start);
                        rng = new System.Random();
                        ResetGame(ref player1, ref player2, player1Start, player2Start,
                            ref yVel1, ref yVel2, ref onGround1, ref onGround2, ref onLadder1, ref onLadder2,
                            ref facingRight1, ref facingRight2, ref currentFrame1, ref currentFrame2,
                            ref animTimer1, ref animTimer2, ref player1Health, ref player2Health, maxHealth,
                            ref player1Shield, maxShield, inventory, ref selectedSlot, ref shotTimer,
                            bullets, pickups, rng, virtualWidth, chestSize);
                        winnerText = string.Empty;
                        gameState  = GameState.Playing;
                    }
                    else
                    {
                        netManager?.Close();
                        netManager    = null;
                        joinInputMode = false;
                        ipInputText   = "";
                        gameState     = GameState.Lobby;
                    }
                }
                else if (hoverQuit && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    exitRequested = true;
                }

                Raylib.BeginTextureMode(renderTarget);
                Raylib.ClearBackground(Color.Black);
                DrawMenuHeader(runAnimRight, menuFrame);

                if (!string.IsNullOrEmpty(winnerText))
                {
                    int ww = Raylib.MeasureText(winnerText, 30);
                    Raylib.DrawText(winnerText, virtualWidth / 2 - ww / 2, 170, 30, Color.White);
                }

                DrawButton(replayButton, networkMode == NetworkMode.Local ? "Rejouer" : "Retour au lobby", mouse);
                DrawButton(quitButton, "Quitter", mouse);
                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            // ═══════════════════════════════════════════════════════════════
            // PLAYING
            // ═══════════════════════════════════════════════════════════════

            // ── 1. Gather inputs ────────────────────────────────────────────
            PlayerInput p1Input, p2Input;

            if (networkMode == NetworkMode.Local)
            {
                // Slot selection (P1 only — P2 has no inventory in local mode)
                if (Raylib.IsKeyPressed(KeyboardKey.One))   selectedSlot = 0;
                if (Raylib.IsKeyPressed(KeyboardKey.Two))   selectedSlot = 1;
                if (Raylib.IsKeyPressed(KeyboardKey.Three)) selectedSlot = 2;

                p1Input = GatherInput(
                    Raylib.IsKeyDown(KeyboardKey.Left),
                    Raylib.IsKeyDown(KeyboardKey.Right),
                    Raylib.IsKeyDown(KeyboardKey.Up),
                    Raylib.IsKeyPressed(KeyboardKey.Up),
                    Raylib.IsMouseButtonPressed(MouseButton.Left),
                    Raylib.IsKeyPressed(KeyboardKey.E),
                    mouse, selectedSlot);

                p2Input = GatherInput(
                    Raylib.IsKeyDown(KeyboardKey.A),
                    Raylib.IsKeyDown(KeyboardKey.D),
                    Raylib.IsKeyDown(KeyboardKey.W),
                    Raylib.IsKeyPressed(KeyboardKey.W),
                    false, false,
                    mouse, 0);
            }
            else
            {
                // Slot selection updates selectedSlot locally (host) or from packet (client)
                if (networkMode == NetworkMode.Host)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.One))   selectedSlot = 0;
                    if (Raylib.IsKeyPressed(KeyboardKey.Two))   selectedSlot = 1;
                    if (Raylib.IsKeyPressed(KeyboardKey.Three)) selectedSlot = 2;
                }

                // Local player is always P1 on Host, P2 on Client
                PlayerInput local = GatherInput(
                    Raylib.IsKeyDown(KeyboardKey.Left),
                    Raylib.IsKeyDown(KeyboardKey.Right),
                    Raylib.IsKeyDown(KeyboardKey.Up),
                    Raylib.IsKeyPressed(KeyboardKey.Up),
                    Raylib.IsMouseButtonPressed(MouseButton.Left),
                    Raylib.IsKeyPressed(KeyboardKey.E),
                    mouse, selectedSlot);

                netManager!.SendInput(local);
                if (netManager.TryReceiveInput(out PlayerInput received))
                    lastRemoteInput = received;

                if (networkMode == NetworkMode.Host)
                {
                    p1Input = local;
                    p2Input = lastRemoteInput;
                }
                else
                {
                    // Client is P2; sync selectedSlot from P1's packet
                    selectedSlot = lastRemoteInput.SelectedSlot;
                    p1Input = lastRemoteInput;
                    p2Input = local;
                }
            }

            // ── 2. P1 aim direction (mouse from p1Input so client stays in sync) ──
            Vector2 player1Center = new(player1.X + player1.Width / 2f, player1.Y + player1.Height / 2f);
            Vector2 p1Mouse       = new(p1Input.MouseX, p1Input.MouseY);
            Vector2 aimDir        = p1Mouse - player1Center;
            if (aimDir.LengthSquared() > 0.0001f) aimDir = Vector2.Normalize(aimDir);
            else aimDir = new Vector2(1f, 0f);

            // ── 3. Move players ─────────────────────────────────────────────
            Texture2D player1Texture = UpdatePlayer(
                ref player1, ref yVel1, ref onGround1, ref onLadder1, ref facingRight1,
                ref currentFrame1, ref animTimer1, p1Input,
                speed, deltaTime, gravity, jump, climbSpeed, animSpeed,
                ladders, solidBlocks, runAnimRight, runAnimLeft, climbAnim);

            Texture2D player2Texture = UpdatePlayer(
                ref player2, ref yVel2, ref onGround2, ref onLadder2, ref facingRight2,
                ref currentFrame2, ref animTimer2, p2Input,
                speed, deltaTime, gravity, jump, climbSpeed, animSpeed,
                ladders, solidBlocks, runAnimRight, runAnimLeft, climbAnim);

            // ── 4. Pickup physics ───────────────────────────────────────────
            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                Pickup pk = pickups[i];
                pk.Velocity = new Vector2(pk.Velocity.X, pk.Velocity.Y + pickupGravity * deltaTime);
                Vector2 nextPos  = pk.Position + pk.Velocity * deltaTime;
                Rectangle nextRc = new(nextPos.X, nextPos.Y, chestSize, chestSize);

                if (pk.Velocity.Y > 0f)
                {
                    foreach (Rectangle block in solidBlocks)
                    {
                        if (Raylib.CheckCollisionRecs(nextRc, block))
                        {
                            nextPos.Y  = block.Y - chestSize;
                            pk.Velocity = Vector2.Zero;
                            break;
                        }
                    }
                }

                pk.Position = nextPos;
                if (pk.Position.Y > virtualHeight + chestSize) pickups.RemoveAt(i);
                else pickups[i] = pk;
            }

            // ── 5. P1 pickup (E) ────────────────────────────────────────────
            if (p1Input.PickupPressed)
            {
                for (int i = pickups.Count - 1; i >= 0; i--)
                {
                    Pickup pk = pickups[i];
                    if (!Raylib.CheckCollisionRecs(player1, new Rectangle(pk.Position.X, pk.Position.Y, chestSize, chestSize))) continue;
                    if (pk.Type == ItemType.Chest && !pk.IsOpened)
                    {
                        int slot = inventory[1] == ItemType.None ? 1 : inventory[2] == ItemType.None ? 2 : -1;
                        if (slot != -1)
                        {
                            ItemType[] drops = { ItemType.HealPotion, ItemType.ShieldPotion, ItemType.NucPotion };
                            inventory[slot] = drops[rng.Next(drops.Length)];
                            pk.IsOpened = true;
                            pickups[i]  = pk;
                        }
                    }
                    break;
                }
            }

            // ── 6. Shooting / item use (P1 only) ────────────────────────────
            shotTimer -= deltaTime;
            if (shotTimer < 0f) shotTimer = 0f;

            Vector2 gunEnd = player1Center + aimDir * gunLength;

            if (p1Input.ActionPressed)
            {
                if (selectedSlot == 0 && shotTimer <= 0f)
                {
                    bullets.Add(new Bullet
                    {
                        Position = gunEnd,
                        Velocity = aimDir * bulletSpeed,
                        Radius   = bulletRadius,
                        Life     = bulletLife
                    });
                    shotTimer = shotCooldown;
                }
                else if (selectedSlot > 0 && inventory[selectedSlot] != ItemType.None)
                {
                    UseItem(inventory[selectedSlot], ref player1Health, ref player1Shield, maxHealth, maxShield, ref player2Health);
                    inventory[selectedSlot] = ItemType.None;
                }
            }

            // ── 7. Bullet update ────────────────────────────────────────────
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet b = bullets[i];
                b.Position += b.Velocity * deltaTime;
                b.Life     -= deltaTime;

                bool remove = b.Life <= 0f || b.Position.X < 0 || b.Position.X > virtualWidth
                                           || b.Position.Y < 0 || b.Position.Y > virtualHeight;

                if (!remove && PointInRect(b.Position, player2))
                {
                    player2Health = System.Math.Max(0, player2Health - gunDamage);
                    remove = true;
                }

                if (!remove)
                    foreach (Rectangle block in solidBlocks)
                        if (PointInRect(b.Position, block)) { remove = true; break; }

                if (remove) bullets.RemoveAt(i);
                else        bullets[i] = b;
            }

            // ── 8. Win condition ────────────────────────────────────────────
            if (player1Health <= 0 || player2Health <= 0)
            {
                winnerText = (player1Health <= 0 && player2Health <= 0) ? "Egalité"
                           : player1Health <= 0 ? "Victoire P2"
                           : "Victoire P1";
                gameState = GameState.End;
                continue;
            }

            // ── 9. Render ───────────────────────────────────────────────────
            Raylib.BeginTextureMode(renderTarget);
            Raylib.ClearBackground(Color.Black);

            foreach (Rectangle block in solidBlocks)
            {
                if (block.Y >= virtualHeight) continue;
                Raylib.DrawRectangleRec(block, Color.White);
            }
            foreach (Rectangle l in ladders)
                Raylib.DrawRectangleRec(l, Color.Gray);

            foreach (Pickup pk in pickups)
            {
                Texture2D tex = pk.Type == ItemType.Chest
                    ? (pk.IsOpened ? chestOpenTexture : chestClosedTexture)
                    : GetItemTexture(pk.Type, healPotionTexture, shieldPotionTexture, nucPotionTexture);
                Raylib.DrawTexturePro(tex,
                    new Rectangle(0, 0, tex.Width, tex.Height),
                    new Rectangle(pk.Position.X, pk.Position.Y, chestSize, chestSize),
                    Vector2.Zero, 0f, Color.White);
            }

            Raylib.DrawTexturePro(player1Texture, new Rectangle(0, 0, player1Texture.Width, player1Texture.Height), player1, Vector2.Zero, 0f, Color.White);
            Raylib.DrawTexturePro(player2Texture, new Rectangle(0, 0, player2Texture.Width, player2Texture.Height), player2, Vector2.Zero, 0f, Color.White);

            if (player1Shield > 0)
                Raylib.DrawTexturePro(shieldTexture,
                    new Rectangle(0, 0, shieldTexture.Width, shieldTexture.Height),
                    new Rectangle(player1.X - 6f, player1.Y - 6f, player1.Width + 12f, player1.Height + 12f),
                    Vector2.Zero, 0f, new Color(255, 255, 255, 77));

            if (selectedSlot == 0)
            {
                Texture2D gunTex = aimDir.X >= 0f ? gunRight : gunLeft;
                float gunAngle   = MathF.Atan2(aimDir.Y, aimDir.X) * (180f / MathF.PI);
                float rotation   = aimDir.X >= 0f ? gunAngle : gunAngle - 180f;
                Vector2 origin   = aimDir.X >= 0f ? new Vector2(0f, gunHeight / 2f) : new Vector2(gunLength, gunHeight / 2f);
                Raylib.DrawTexturePro(gunTex,
                    new Rectangle(0, 0, gunTex.Width, gunTex.Height),
                    new Rectangle(player1Center.X, player1Center.Y, gunLength, gunHeight),
                    origin, rotation, Color.White);
            }
            else if (selectedSlot > 0 && inventory[selectedSlot] != ItemType.None)
            {
                Texture2D heldTex = GetItemTexture(inventory[selectedSlot], healPotionTexture, shieldPotionTexture, nucPotionTexture);
                Vector2 heldPos   = player1Center + new Vector2(aimDir.X * 25f, aimDir.Y * 25f);
                Raylib.DrawTexturePro(heldTex,
                    new Rectangle(0, 0, heldTex.Width, heldTex.Height),
                    new Rectangle(heldPos.X - 12f, heldPos.Y - 12f, 24f, 24f),
                    Vector2.Zero, 0f, Color.White);
            }

            foreach (Bullet b in bullets)
                Raylib.DrawCircleV(b.Position, b.Radius, Color.Yellow);

            DrawHealthBars(virtualWidth, virtualHeight, player1Health, player2Health, maxHealth, crownTexture);
            DrawInventory(inventory, selectedSlot, healPotionTexture, shieldPotionTexture, nucPotionTexture, virtualHeight);

            string controls = networkMode == NetworkMode.Local
                ? "F11=Plein écran | P1: Flèches+Souris | P2: ZQSD"
                : networkMode == NetworkMode.Host
                    ? "F11=Plein écran | Flèches+Souris | Vous êtes P1"
                    : "F11=Plein écran | Flèches+Souris | Vous êtes P2";
            Raylib.DrawText(controls, 10, 10, 18, Color.White);
            Raylib.DrawText("E=Ramasser | 1-3=Equip | Clic=Utiliser", 10, 32, 18, Color.White);

            Raylib.EndTextureMode();
            DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
        }

        // ── Cleanup ─────────────────────────────────────────────────────────
        netManager?.Close();
        foreach (var t in runAnimRight) Raylib.UnloadTexture(t);
        foreach (var t in runAnimLeft)  Raylib.UnloadTexture(t);
        foreach (var t in climbAnim)    Raylib.UnloadTexture(t);
        Raylib.UnloadTexture(gunRight);
        Raylib.UnloadTexture(gunLeft);
        Raylib.UnloadTexture(healPotionTexture);
        Raylib.UnloadTexture(shieldPotionTexture);
        Raylib.UnloadTexture(nucPotionTexture);
        Raylib.UnloadTexture(shieldTexture);
        Raylib.UnloadTexture(crownTexture);
        Raylib.UnloadTexture(chestClosedTexture);
        Raylib.UnloadTexture(chestOpenTexture);
        Raylib.UnloadRenderTexture(renderTarget);
        Raylib.CloseWindow();
    }
}
