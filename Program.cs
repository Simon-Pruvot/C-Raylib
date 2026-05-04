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

        Texture2D gunRight            = Raylib.LoadTexture("imgs/gun-droite.png");
        Texture2D gunLeft             = Raylib.LoadTexture("imgs/gun-gauche.png");
        Texture2D healPotionTexture   = Raylib.LoadTexture("imgs/heal-potion.png");
        Texture2D shieldPotionTexture = Raylib.LoadTexture("imgs/shild-potion.png");
        Texture2D nucPotionTexture    = Raylib.LoadTexture("imgs/nuc-potion.png");
        Texture2D shieldTexture       = Raylib.LoadTexture("imgs/shild.png");
        Texture2D crownTexture        = Raylib.LoadTexture("imgs/crown.png");
        Texture2D chestClosedTexture  = Raylib.LoadTexture("imgs/coffre-fermé-1.png");
        Texture2D chestOpenTexture    = Raylib.LoadTexture("imgs/coffre-ouvert-1.png");

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
        int maxShield = 20,  player1Shield = 0,   player2Shield = 0;
        float gunLength = 42f;
        int gunDamage = 20;
        float shotCooldown = 0.2f, shotTimer1 = 0f, shotTimer2 = 0f;
        float bulletSpeed = 900f, bulletRadius = 4f, bulletLife = 2f;
        float chestSize = 48f, pickupGravity = 600f;

        List<Bullet> bullets = new();
        List<Pickup> pickups = new();
        System.Random rng    = new();

        ItemType[] inventory1 = { ItemType.Gun, ItemType.None, ItemType.None };
        int selectedSlot1 = 0;
        ItemType[] inventory2 = { ItemType.Gun, ItemType.None, ItemType.None };
        int selectedSlot2 = 0;

        NetworkMode networkMode     = NetworkMode.Local;
        NetManager? netManager      = null;
        PlayerInput lastRemoteInput = default;
        string ipInputText          = "";
        bool joinInputMode          = false;
        string localIp              = "";

        bool useBot          = false;
        System.Random botRng = new();
        float botThinkTimer  = 0f;
        const float BotThinkInterval = 0.12f;
        PlayerInput botInput = default;

        GameState gameState = GameState.Menu;
        bool exitRequested  = false;
        string winnerText   = string.Empty;
        int menuFrame       = 0;
        float menuTimer     = 0f;

        // boutons — 260×50, centrés horizontalement (x=470)
        Rectangle botButton     = new(470, 310, 260, 50);
        Rectangle playButton    = new(470, 375, 260, 50);
        Rectangle onlineButton  = new(470, 440, 260, 50);
        Rectangle hostButton    = new(470, 335, 260, 50);
        Rectangle joinButton    = new(470, 400, 260, 50);
        Rectangle backButton    = new(470, 530, 260, 50);
        Rectangle ipInputBox    = new(350, 400, 500, 44);
        Rectangle connectButton = new(470, 460, 260, 50);
        Rectangle replayButton  = new(470, 355, 260, 50);
        Rectangle menuButton    = new(470, 420, 260, 50);
        Rectangle quitButton    = new(470, 485, 260, 50);

        while (!Raylib.WindowShouldClose() && !exitRequested)
        {
            float deltaTime = Raylib.GetFrameTime();
            if (Raylib.IsKeyPressed(KeyboardKey.F11)) Raylib.ToggleFullscreen();

            GetRenderScale(virtualWidth, virtualHeight, out float scale, out Vector2 offset);
            Vector2 mouse = GetVirtualMousePosition(scale, offset);

            // MENU
            if (gameState == GameState.Menu)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed) { menuFrame = (menuFrame + 1) % 2; menuTimer = 0f; }

                void StartLocal(bool bot)
                {
                    networkMode = NetworkMode.Local;
                    useBot      = bot;
                    currentMap  = 0;
                    LoadMap(currentMap, out solidBlocks, out ladders, out player1Start, out player2Start);
                    rng = new System.Random();
                    ResetAll(ref player1, ref player2, player1Start, player2Start,
                        ref yVel1, ref yVel2, ref onGround1, ref onGround2,
                        ref onLadder1, ref onLadder2, ref facingRight1, ref facingRight2,
                        ref currentFrame1, ref currentFrame2, ref animTimer1, ref animTimer2,
                        ref player1Health, ref player2Health, maxHealth,
                        ref player1Shield, ref player2Shield, maxShield,
                        inventory1, inventory2, ref selectedSlot1, ref selectedSlot2,
                        ref shotTimer1, ref shotTimer2, bullets, pickups, rng, virtualWidth, chestSize);
                    winnerText = string.Empty;
                    gameState  = GameState.Playing;
                }

                if (Raylib.CheckCollisionPointRec(mouse, botButton)    && Raylib.IsMouseButtonPressed(MouseButton.Left)) StartLocal(true);
                if (Raylib.CheckCollisionPointRec(mouse, playButton)   && Raylib.IsMouseButtonPressed(MouseButton.Left)) StartLocal(false);
                if (Raylib.CheckCollisionPointRec(mouse, onlineButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                { joinInputMode = false; ipInputText = ""; gameState = GameState.Lobby; }

                Raylib.BeginTextureMode(renderTarget);
                Raylib.ClearBackground(Color.Black);
                DrawMenuHeader(runAnimRight, menuFrame);
                DrawButton(botButton,    "1 joueur (bot)", mouse);
                DrawButton(playButton,   "2 joueurs",       mouse);
                DrawButton(onlineButton, "En ligne",        mouse);
                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            // LOBBY
            if (gameState == GameState.Lobby)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed) { menuFrame = (menuFrame + 1) % 2; menuTimer = 0f; }

                if (Raylib.CheckCollisionPointRec(mouse, backButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                { joinInputMode = false; ipInputText = ""; gameState = GameState.Menu; }
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
                    { joinInputMode = true; ipInputText = ""; }
                }
                else
                {
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
                string lobbyTitle = "En ligne";
                Raylib.DrawText(lobbyTitle, (virtualWidth - Raylib.MeasureText(lobbyTitle, 24)) / 2, 302, 24, Color.LightGray);
                if (!joinInputMode)
                {
                    DrawButton(hostButton, "Héberger",  mouse);
                    DrawButton(joinButton, "Rejoindre", mouse);
                }
                else
                {
                    Raylib.DrawText("IP de l'hôte", (int)ipInputBox.X, (int)ipInputBox.Y - 26, 20, Color.LightGray);
                    Raylib.DrawRectangleRec(ipInputBox, new Color(28, 28, 28, 255));
                    Raylib.DrawRectangleLinesEx(ipInputBox, 1.5f, Color.Gray);
                    Raylib.DrawText(ipInputText + "|", (int)ipInputBox.X + 12, (int)ipInputBox.Y + 11, 22, Color.White);
                    DrawButton(connectButton, "Connecter", mouse);
                }
                DrawButton(backButton, "Retour", mouse);
                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            // CONNEXION
            if (gameState == GameState.Connecting)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed) { menuFrame = (menuFrame + 1) % 2; menuTimer = 0f; }

                bool connected = networkMode == NetworkMode.Host
                    ? netManager!.HostPoll(out int gameSeed)
                    : netManager!.ClientPoll(out gameSeed);

                if (connected)
                {
                    rng = new System.Random(gameSeed);
                    lastRemoteInput = default;
                    currentMap      = 0;
                    LoadMap(currentMap, out solidBlocks, out ladders, out player1Start, out player2Start);
                    ResetAll(ref player1, ref player2, player1Start, player2Start,
                        ref yVel1, ref yVel2, ref onGround1, ref onGround2,
                        ref onLadder1, ref onLadder2, ref facingRight1, ref facingRight2,
                        ref currentFrame1, ref currentFrame2, ref animTimer1, ref animTimer2,
                        ref player1Health, ref player2Health, maxHealth,
                        ref player1Shield, ref player2Shield, maxShield,
                        inventory1, inventory2, ref selectedSlot1, ref selectedSlot2,
                        ref shotTimer1, ref shotTimer2, bullets, pickups, rng, virtualWidth, chestSize);
                    winnerText = string.Empty;
                    gameState  = GameState.Playing;
                    continue;
                }

                Raylib.BeginTextureMode(renderTarget);
                Raylib.ClearBackground(Color.Black);
                DrawMenuHeader(runAnimRight, menuFrame);
                if (networkMode == NetworkMode.Host)
                {
                    string wait = "En attente du joueur 2...";
                    Raylib.DrawText(wait, (virtualWidth - Raylib.MeasureText(wait, 26)) / 2, 358, 26, Color.White);
                    string ip = $"Votre IP : {localIp}";
                    Raylib.DrawText(ip, (virtualWidth - Raylib.MeasureText(ip, 22)) / 2, 398, 22, Color.LightGray);
                    string port = "(port 7777)";
                    Raylib.DrawText(port, (virtualWidth - Raylib.MeasureText(port, 18)) / 2, 430, 18, Color.Gray);
                }
                else
                {
                    string conn = $"Connexion à {ipInputText}:7777...";
                    Raylib.DrawText(conn, (virtualWidth - Raylib.MeasureText(conn, 26)) / 2, 368, 26, Color.White);
                    string hint = "L'hôte doit avoir lancé le jeu en premier.";
                    Raylib.DrawText(hint, (virtualWidth - Raylib.MeasureText(hint, 18)) / 2, 410, 18, Color.Gray);
                }
                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            // FIN
            if (gameState == GameState.End)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed) { menuFrame = (menuFrame + 1) % 2; menuTimer = 0f; }

                if (Raylib.CheckCollisionPointRec(mouse, replayButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (networkMode == NetworkMode.Local)
                    {
                        currentMap = (currentMap + 1) % 2;
                        LoadMap(currentMap, out solidBlocks, out ladders, out player1Start, out player2Start);
                        rng = new System.Random();
                        ResetAll(ref player1, ref player2, player1Start, player2Start,
                            ref yVel1, ref yVel2, ref onGround1, ref onGround2,
                            ref onLadder1, ref onLadder2, ref facingRight1, ref facingRight2,
                            ref currentFrame1, ref currentFrame2, ref animTimer1, ref animTimer2,
                            ref player1Health, ref player2Health, maxHealth,
                            ref player1Shield, ref player2Shield, maxShield,
                            inventory1, inventory2, ref selectedSlot1, ref selectedSlot2,
                            ref shotTimer1, ref shotTimer2, bullets, pickups, rng, virtualWidth, chestSize);
                        winnerText = string.Empty;
                        gameState  = GameState.Playing;
                    }
                    else
                    {
                        // reconnexion sans ressaisir l'IP
                        netManager?.Close();
                        netManager = new NetManager();
                        if (networkMode == NetworkMode.Host)
                            netManager.StartHost();
                        else
                            netManager.StartClient(ipInputText);
                        lastRemoteInput = default;
                        gameState = GameState.Connecting;
                    }
                }
                else if (Raylib.CheckCollisionPointRec(mouse, menuButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    netManager?.Close();
                    netManager = null;
                    gameState = GameState.Menu;
                }
                else if (Raylib.CheckCollisionPointRec(mouse, quitButton) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    exitRequested = true;
                }

                Raylib.BeginTextureMode(renderTarget);
                Raylib.ClearBackground(Color.Black);
                DrawMenuHeader(runAnimRight, menuFrame);
                if (!string.IsNullOrEmpty(winnerText))
                {
                    int ww = Raylib.MeasureText(winnerText, 36);
                    Raylib.DrawText(winnerText, virtualWidth / 2 - ww / 2, 308, 36, Color.White);
                }
                DrawButton(replayButton, "Rejouer",        mouse);
                DrawButton(menuButton,   "Menu principal", mouse);
                DrawButton(quitButton,   "Quitter",        mouse);
                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            // JEU

            // inputs
            PlayerInput p1Input, p2Input;

            if (networkMode == NetworkMode.Local)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.One))   selectedSlot1 = 0;
                if (Raylib.IsKeyPressed(KeyboardKey.Two))   selectedSlot1 = 1;
                if (Raylib.IsKeyPressed(KeyboardKey.Three)) selectedSlot1 = 2;

                p1Input = GatherInput(
                    Raylib.IsKeyDown(KeyboardKey.Left), Raylib.IsKeyDown(KeyboardKey.Right),
                    Raylib.IsKeyDown(KeyboardKey.Up),   Raylib.IsKeyPressed(KeyboardKey.Up),
                    Raylib.IsMouseButtonPressed(MouseButton.Left), Raylib.IsKeyPressed(KeyboardKey.E),
                    mouse, selectedSlot1);

                if (useBot)
                {
                    botThinkTimer += deltaTime;
                    if (botThinkTimer >= BotThinkInterval)
                    {
                        botInput      = ComputeBotInput(player2, onGround2, onLadder2, player1, bullets, virtualWidth, botRng);
                        botThinkTimer = 0f;
                    }
                    p2Input = botInput;
                }
                else
                {
                    p2Input = GatherInput(
                        Raylib.IsKeyDown(KeyboardKey.A), Raylib.IsKeyDown(KeyboardKey.D),
                        Raylib.IsKeyDown(KeyboardKey.W), Raylib.IsKeyPressed(KeyboardKey.W),
                        false, false, mouse, 0);
                }
            }
            else if (networkMode == NetworkMode.Host)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.One))   selectedSlot1 = 0;
                if (Raylib.IsKeyPressed(KeyboardKey.Two))   selectedSlot1 = 1;
                if (Raylib.IsKeyPressed(KeyboardKey.Three)) selectedSlot1 = 2;

                p1Input = GatherInput(
                    Raylib.IsKeyDown(KeyboardKey.Left), Raylib.IsKeyDown(KeyboardKey.Right),
                    Raylib.IsKeyDown(KeyboardKey.Up),   Raylib.IsKeyPressed(KeyboardKey.Up),
                    Raylib.IsMouseButtonPressed(MouseButton.Left), Raylib.IsKeyPressed(KeyboardKey.E),
                    mouse, selectedSlot1);

                netManager!.SendInput(p1Input);
                if (netManager.TryReceiveInput(out PlayerInput recv)) lastRemoteInput = recv;
                p2Input       = lastRemoteInput;
                selectedSlot2 = p2Input.SelectedSlot;
            }
            else // client = P2
            {
                if (Raylib.IsKeyPressed(KeyboardKey.One))   selectedSlot2 = 0;
                if (Raylib.IsKeyPressed(KeyboardKey.Two))   selectedSlot2 = 1;
                if (Raylib.IsKeyPressed(KeyboardKey.Three)) selectedSlot2 = 2;

                p2Input = GatherInput(
                    Raylib.IsKeyDown(KeyboardKey.Left), Raylib.IsKeyDown(KeyboardKey.Right),
                    Raylib.IsKeyDown(KeyboardKey.Up),   Raylib.IsKeyPressed(KeyboardKey.Up),
                    Raylib.IsMouseButtonPressed(MouseButton.Left), Raylib.IsKeyPressed(KeyboardKey.E),
                    mouse, selectedSlot2);

                netManager!.SendInput(p2Input);
                if (netManager.TryReceiveInput(out PlayerInput recv)) lastRemoteInput = recv;
                p1Input       = lastRemoteInput;
                selectedSlot1 = p1Input.SelectedSlot;
            }

            // visée
            Vector2 player1Center = new(player1.X + player1.Width / 2f, player1.Y + player1.Height / 2f);
            Vector2 player2Center = new(player2.X + player2.Width / 2f, player2.Y + player2.Height / 2f);

            Vector2 aimDir1 = new Vector2(p1Input.MouseX, p1Input.MouseY) - player1Center;
            if (aimDir1.LengthSquared() > 0.0001f) aimDir1 = Vector2.Normalize(aimDir1); else aimDir1 = new(1f, 0f);

            Vector2 aimDir2 = new Vector2(p2Input.MouseX, p2Input.MouseY) - player2Center;
            if (aimDir2.LengthSquared() > 0.0001f) aimDir2 = Vector2.Normalize(aimDir2); else aimDir2 = new(-1f, 0f);

            // déplacement
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

            // mort par chute (carte 1 sans sol)
            if (currentMap == 1)
            {
                if (player1.Y > virtualHeight) player1Health = 0;
                if (player2.Y > virtualHeight) player2Health = 0;
            }

            // physique des coffres
            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                Pickup pk = pickups[i];
                pk.Velocity = new Vector2(pk.Velocity.X, pk.Velocity.Y + pickupGravity * deltaTime);
                Vector2 np   = pk.Position + pk.Velocity * deltaTime;
                Rectangle nr = new(np.X, np.Y, chestSize, chestSize);
                if (pk.Velocity.Y > 0f)
                    foreach (Rectangle b in solidBlocks)
                        if (Raylib.CheckCollisionRecs(nr, b)) { np.Y = b.Y - chestSize; pk.Velocity = Vector2.Zero; break; }
                pk.Position = np;
                if (pk.Position.Y > virtualHeight + chestSize) pickups.RemoveAt(i);
                else pickups[i] = pk;
            }

            // ramassage P1
            if (p1Input.PickupPressed)
            {
                for (int i = pickups.Count - 1; i >= 0; i--)
                {
                    Pickup pk = pickups[i];
                    if (!Raylib.CheckCollisionRecs(player1, new Rectangle(pk.Position.X, pk.Position.Y, chestSize, chestSize))) continue;
                    if (pk.Type == ItemType.Chest && !pk.IsOpened)
                    {
                        int slot = inventory1[1] == ItemType.None ? 1 : inventory1[2] == ItemType.None ? 2 : -1;
                        if (slot != -1)
                        {
                            ItemType[] drops = { ItemType.HealPotion, ItemType.ShieldPotion, ItemType.NucPotion };
                            inventory1[slot] = drops[rng.Next(drops.Length)];
                            pk.IsOpened = true; pickups[i] = pk;
                        }
                    }
                    break;
                }
            }

            // ramassage P2 (réseau ou bot)
            if ((networkMode != NetworkMode.Local || useBot) && p2Input.PickupPressed)
            {
                for (int i = pickups.Count - 1; i >= 0; i--)
                {
                    Pickup pk = pickups[i];
                    if (!Raylib.CheckCollisionRecs(player2, new Rectangle(pk.Position.X, pk.Position.Y, chestSize, chestSize))) continue;
                    if (pk.Type == ItemType.Chest && !pk.IsOpened)
                    {
                        int slot = inventory2[1] == ItemType.None ? 1 : inventory2[2] == ItemType.None ? 2 : -1;
                        if (slot != -1)
                        {
                            ItemType[] drops = { ItemType.HealPotion, ItemType.ShieldPotion, ItemType.NucPotion };
                            inventory2[slot] = drops[rng.Next(drops.Length)];
                            pk.IsOpened = true; pickups[i] = pk;
                        }
                    }
                    break;
                }
            }

            // tir et items
            shotTimer1 -= deltaTime; if (shotTimer1 < 0f) shotTimer1 = 0f;
            shotTimer2 -= deltaTime; if (shotTimer2 < 0f) shotTimer2 = 0f;

            if (p1Input.ActionPressed)
            {
                if (selectedSlot1 == 0 && shotTimer1 <= 0f)
                {
                    bullets.Add(new Bullet { Position = player1Center + aimDir1 * gunLength, Velocity = aimDir1 * bulletSpeed, Radius = bulletRadius, Life = bulletLife, IsP2Bullet = false });
                    shotTimer1 = shotCooldown;
                }
                else if (selectedSlot1 > 0 && inventory1[selectedSlot1] != ItemType.None)
                {
                    UseItem(inventory1[selectedSlot1], ref player1Health, ref player1Shield, maxHealth, maxShield, ref player2Health);
                    inventory1[selectedSlot1] = ItemType.None;
                }
            }

            if ((networkMode != NetworkMode.Local || useBot) && p2Input.ActionPressed)
            {
                if (selectedSlot2 == 0 && shotTimer2 <= 0f)
                {
                    bullets.Add(new Bullet { Position = player2Center + aimDir2 * gunLength, Velocity = aimDir2 * bulletSpeed, Radius = bulletRadius, Life = bulletLife, IsP2Bullet = true });
                    shotTimer2 = shotCooldown;
                }
                else if (selectedSlot2 > 0 && inventory2[selectedSlot2] != ItemType.None)
                {
                    UseItem(inventory2[selectedSlot2], ref player2Health, ref player2Shield, maxHealth, maxShield, ref player1Health);
                    inventory2[selectedSlot2] = ItemType.None;
                }
            }

            // balles
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet b = bullets[i];
                b.Position += b.Velocity * deltaTime;
                b.Life     -= deltaTime;

                bool remove = b.Life <= 0f
                    || b.Position.X < 0 || b.Position.X > virtualWidth
                    || b.Position.Y < 0 || b.Position.Y > virtualHeight;

                if (!remove && !b.IsP2Bullet && PointInRect(b.Position, player2))
                {
                    if (player2Shield > 0) player2Shield = System.Math.Max(0, player2Shield - 1);
                    else                   player2Health  = System.Math.Max(0, player2Health - gunDamage);
                    remove = true;
                }
                if (!remove && b.IsP2Bullet && PointInRect(b.Position, player1))
                {
                    if (player1Shield > 0) player1Shield = System.Math.Max(0, player1Shield - 1);
                    else                   player1Health  = System.Math.Max(0, player1Health - gunDamage);
                    remove = true;
                }

                if (!remove)
                    foreach (Rectangle block in solidBlocks)
                        if (PointInRect(b.Position, block)) { remove = true; break; }

                if (remove) bullets.RemoveAt(i); else bullets[i] = b;
            }

            // fin de round
            if (player1Health <= 0 || player2Health <= 0)
            {
                winnerText = (player1Health <= 0 && player2Health <= 0) ? "Egalité"
                           : player1Health <= 0 ? "Victoire P2"
                           : "Victoire P1";
                gameState = GameState.End;
                continue;
            }

            // rendu
            Raylib.BeginTextureMode(renderTarget);
            Raylib.ClearBackground(Color.Black);

            foreach (Rectangle block in solidBlocks)
            {
                if (block.Y >= virtualHeight) continue;
                Raylib.DrawRectangleRec(block, Color.White);
            }
            foreach (Rectangle l in ladders) Raylib.DrawRectangleRec(l, Color.Gray);

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

            DrawSprite(player1Texture, player1);
            DrawSprite(player2Texture, player2);

            if (player1Shield > 0)
                Raylib.DrawTexturePro(shieldTexture,
                    new Rectangle(0, 0, shieldTexture.Width, shieldTexture.Height),
                    new Rectangle(player1.X - 6f, player1.Y - 6f, player1.Width + 12f, player1.Height + 12f),
                    Vector2.Zero, 0f, new Color(255, 255, 255, 77));
            if ((networkMode != NetworkMode.Local || useBot) && player2Shield > 0)
                Raylib.DrawTexturePro(shieldTexture,
                    new Rectangle(0, 0, shieldTexture.Width, shieldTexture.Height),
                    new Rectangle(player2.X - 6f, player2.Y - 6f, player2.Width + 12f, player2.Height + 12f),
                    Vector2.Zero, 0f, new Color(100, 100, 255, 77));

            DrawPlayerWeapon(selectedSlot1, inventory1, aimDir1, player1Center,
                gunRight, gunLeft, gunLength, healPotionTexture, shieldPotionTexture, nucPotionTexture);

            if (networkMode != NetworkMode.Local || useBot)
                DrawPlayerWeapon(selectedSlot2, inventory2, aimDir2, player2Center,
                    gunRight, gunLeft, gunLength, healPotionTexture, shieldPotionTexture, nucPotionTexture);

            foreach (Bullet b in bullets)
                Raylib.DrawCircleV(b.Position, b.Radius, b.IsP2Bullet ? new Color(0, 220, 220, 255) : Color.Yellow);

            DrawHealthBars(virtualWidth, virtualHeight, player1Health, player2Health, maxHealth, crownTexture);
            DrawInventory(inventory1, selectedSlot1, healPotionTexture, shieldPotionTexture, nucPotionTexture, virtualHeight);

            if (networkMode != NetworkMode.Local || useBot)
                DrawInventoryRight(inventory2, selectedSlot2, healPotionTexture, shieldPotionTexture, nucPotionTexture, virtualHeight, virtualWidth);

            string role = networkMode == NetworkMode.Local
                ? (useBot ? "P1 : Flèches + Souris  |  P2 : Bot" : "P1 : Flèches + Souris  |  P2 : WASD")
                : networkMode == NetworkMode.Host ? "P1 — Flèches + Souris"
                                                  : "P2 — Flèches + Souris";
            Raylib.DrawText(role, 10, 10, 18, Color.White);
            Raylib.DrawText("E : ramasser  |  1-3 : slot  |  clic : tirer/utiliser", 10, 32, 18, Color.White);

            Raylib.EndTextureMode();
            DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
        }

        netManager?.Close();
        foreach (var t in runAnimRight) Raylib.UnloadTexture(t);
        foreach (var t in runAnimLeft)  Raylib.UnloadTexture(t);
        foreach (var t in climbAnim)    Raylib.UnloadTexture(t);
        Raylib.UnloadTexture(gunRight);   Raylib.UnloadTexture(gunLeft);
        Raylib.UnloadTexture(healPotionTexture); Raylib.UnloadTexture(shieldPotionTexture);
        Raylib.UnloadTexture(nucPotionTexture);  Raylib.UnloadTexture(shieldTexture);
        Raylib.UnloadTexture(crownTexture);
        Raylib.UnloadTexture(chestClosedTexture); Raylib.UnloadTexture(chestOpenTexture);
        Raylib.UnloadRenderTexture(renderTarget);
        Raylib.CloseWindow();
    }

    static void DrawSprite(Texture2D tex, Rectangle box)
    {
        float w = box.Height * tex.Width / (float)tex.Height;
        Raylib.DrawTexturePro(tex,
            new Rectangle(0, 0, tex.Width, tex.Height),
            new Rectangle(box.X + (box.Width - w) / 2f, box.Y, w, box.Height),
            Vector2.Zero, 0f, Color.White);
    }

    static void DrawPlayerWeapon(
        int slot, ItemType[] inventory, Vector2 aimDir, Vector2 center,
        Texture2D gunRight, Texture2D gunLeft, float gunLength,
        Texture2D heal, Texture2D shield, Texture2D nuc)
    {
        if (slot == 0)
        {
            Texture2D tex  = aimDir.X >= 0f ? gunRight : gunLeft;
            float h        = gunLength * tex.Height / (float)tex.Width;
            float angle    = MathF.Atan2(aimDir.Y, aimDir.X) * (180f / MathF.PI);
            float rotation = aimDir.X >= 0f ? angle : angle - 180f;
            Vector2 origin = aimDir.X >= 0f ? new(0f, h / 2f) : new(gunLength, h / 2f);
            Raylib.DrawTexturePro(tex,
                new Rectangle(0, 0, tex.Width, tex.Height),
                new Rectangle(center.X, center.Y, gunLength, h),
                origin, rotation, Color.White);
        }
        else if (slot > 0 && inventory[slot] != ItemType.None)
        {
            Texture2D tex = GetItemTexture(inventory[slot], heal, shield, nuc);
            Vector2 pos   = center + new Vector2(aimDir.X * 25f, aimDir.Y * 25f);
            Raylib.DrawTexturePro(tex,
                new Rectangle(0, 0, tex.Width, tex.Height),
                new Rectangle(pos.X - 12f, pos.Y - 12f, 24f, 24f),
                Vector2.Zero, 0f, Color.White);
        }
    }

    static void DrawInventoryRight(
        ItemType[] inventory, int selectedSlot,
        Texture2D heal, Texture2D shieldTex, Texture2D nuc,
        int screenHeight, int screenWidth)
    {
        int slotSize = 50, pad = 10;
        int totalW   = inventory.Length * slotSize + (inventory.Length - 1) * pad;
        int invX     = screenWidth - totalW - 10;
        int invY     = screenHeight - slotSize - 10;

        for (int i = 0; i < inventory.Length; i++)
        {
            Rectangle sr = new(invX + i * (slotSize + pad), invY, slotSize, slotSize);
            Raylib.DrawRectangleRec(sr, Color.DarkGray);
            Raylib.DrawRectangleLinesEx(sr, i == selectedSlot ? 3f : 2f,
                i == selectedSlot ? new Color(0, 220, 220, 255) : Color.Gray);
            Raylib.DrawText((i + 1).ToString(), (int)sr.X + 4, (int)sr.Y + 2, 16, Color.White);
            if (i == 0) { Raylib.DrawText("*", (int)sr.X + 20, (int)sr.Y + 14, 24, Color.White); continue; }
            if (inventory[i] != ItemType.None)
            {
                Texture2D t = GetItemTexture(inventory[i], heal, shieldTex, nuc);
                Raylib.DrawTexturePro(t, new Rectangle(0, 0, t.Width, t.Height),
                    new Rectangle(sr.X + 9, sr.Y + 9, 32, 32), Vector2.Zero, 0f, Color.White);
            }
        }
    }

    static PlayerInput ComputeBotInput(
        Rectangle bot, bool botOnGround, bool botOnLadder,
        Rectangle p1, List<Bullet> bullets,
        float virtualWidth, System.Random rng)
    {
        Vector2 botC = new(bot.X + bot.Width / 2f, bot.Y + bot.Height / 2f);
        Vector2 p1C  = new(p1.X + p1.Width / 2f,  p1.Y + p1.Height / 2f);

        bool moveLeft = false, moveRight = false, jump = false;

        float dx   = botC.X - p1C.X;
        float dist = MathF.Abs(dx);

        // garde une distance confortable
        if (dist < 300f)
        {
            if (dx > 0) moveRight = true; else moveLeft = true;
        }
        else if (dist > 500f && rng.NextDouble() < 0.35)
        {
            if (dx > 0) moveLeft = true; else moveRight = true;
        }

        if (bot.X < 40f)                             { moveLeft = false; moveRight = true; }
        if (bot.X + bot.Width > virtualWidth - 40f)  { moveRight = false; moveLeft = true; }

        // esquive les balles de P1
        foreach (Bullet b in bullets)
        {
            if (b.IsP2Bullet) continue;
            bool sameLevel   = MathF.Abs(b.Position.Y - botC.Y) < 70f;
            bool approaching = (b.Velocity.X > 0 && b.Position.X < botC.X - 10)
                            || (b.Velocity.X < 0 && b.Position.X > botC.X + 10);
            bool nearby      = MathF.Abs(b.Position.X - botC.X) < 280f;

            if (sameLevel && approaching && nearby && botOnGround)
            {
                jump = true;
                break;
            }
        }

        if (botOnGround && !jump && rng.NextDouble() < 0.04)
            jump = true;

        // tir si à portée (cadence réduite)
        bool shoot = dist < 550f && rng.NextDouble() < 0.2;

        return new PlayerInput
        {
            MoveLeft = moveLeft, MoveRight = moveRight, JumpPressed = jump,
            ActionPressed = shoot,
            MouseX = p1C.X, MouseY = p1C.Y
        };
    }

    static void ResetAll(
        ref Rectangle player1, ref Rectangle player2,
        Rectangle p1Start, Rectangle p2Start,
        ref float yVel1, ref float yVel2,
        ref bool onGround1, ref bool onGround2,
        ref bool onLadder1, ref bool onLadder2,
        ref bool facingRight1, ref bool facingRight2,
        ref int frame1, ref int frame2,
        ref float animTimer1, ref float animTimer2,
        ref int p1Health, ref int p2Health, int maxHealth,
        ref int p1Shield, ref int p2Shield, int maxShield,
        ItemType[] inv1, ItemType[] inv2,
        ref int slot1, ref int slot2,
        ref float shotTimer1, ref float shotTimer2,
        List<Bullet> bullets, List<Pickup> pickups,
        System.Random rng, int virtualWidth, float chestSize)
    {
        player1 = p1Start; player2 = p2Start;
        yVel1 = 0f; yVel2 = 0f;
        onGround1 = false; onGround2 = false;
        onLadder1 = false; onLadder2 = false;
        facingRight1 = true; facingRight2 = true;
        frame1 = 0; frame2 = 0;
        animTimer1 = 0f; animTimer2 = 0f;
        p1Health = maxHealth; p2Health = maxHealth;
        p1Shield = 0; p2Shield = 0;
        shotTimer1 = 0f; shotTimer2 = 0f;
        bullets.Clear(); pickups.Clear();
        inv1[0] = ItemType.Gun; inv1[1] = ItemType.None; inv1[2] = ItemType.None; slot1 = 0;
        inv2[0] = ItemType.Gun; inv2[1] = ItemType.None; inv2[2] = ItemType.None; slot2 = 0;

        if (rng.NextDouble() < 0.5)
        {
            float x = rng.Next(40, virtualWidth - 40);
            pickups.Add(new Pickup { Type = ItemType.Chest, Position = new Vector2(x, -chestSize), Velocity = Vector2.Zero });
        }
    }
}
