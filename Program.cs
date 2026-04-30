using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

partial class Program
{
    static void Main()
    {
        int virtualWidth = 1200;
        int virtualHeight = 800;

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(virtualWidth, virtualHeight, "Test Simon1");
        
        Raylib.SetTargetFPS(120);

        RenderTexture2D renderTarget = Raylib.LoadRenderTexture(virtualWidth, virtualHeight);
        Raylib.SetTextureFilter(renderTarget.Texture, TextureFilter.Bilinear);

        Texture2D[] runAnimRight = new Texture2D[] {
            Raylib.LoadTexture("imgs/perso-droite-1.png"),
            Raylib.LoadTexture("imgs/perso-droite-2.png")
        };

        Texture2D[] runAnimLeft = new Texture2D[] {
            Raylib.LoadTexture("imgs/perso-gauche-1.png"),
            Raylib.LoadTexture("imgs/perso-gauche-2.png")
        };

        Texture2D[] climbAnim = new Texture2D[] {
            Raylib.LoadTexture("imgs/perso-monte-1.png"),
            Raylib.LoadTexture("imgs/perso-monte-2.png")
        };

        Texture2D gunRight = Raylib.LoadTexture("imgs/gun-droite.png");
        Texture2D gunLeft = Raylib.LoadTexture("imgs/gun-gauche.png");
        Texture2D healPotionTexture = Raylib.LoadTexture("imgs/heal-potion.png");
        Texture2D shieldPotionTexture = Raylib.LoadTexture("imgs/shild-potion.png");
        Texture2D nucPotionTexture = Raylib.LoadTexture("imgs/nuc-potion.png");
        Texture2D shieldTexture = Raylib.LoadTexture("imgs/shild.png");
        Texture2D crownTexture = Raylib.LoadTexture("imgs/crown.png");
        Texture2D chestClosedTexture = Raylib.LoadTexture("imgs/coffre-fermé-1.png");
        Texture2D chestOpenTexture = Raylib.LoadTexture("imgs/coffre-ouvert-1.png");
        Rectangle player1Start = new Rectangle(100, 500, 40, 60);
        Rectangle player2Start = new Rectangle(180, 500, 40, 60);
        Rectangle player1 = player1Start;
        Rectangle player2 = player2Start;
        float speed = 200f;
//la map bébé
        Rectangle ground = new Rectangle(0, 750, 1200, 50);
        Rectangle plat1 = new Rectangle(0, 500, 200, 20);
        Rectangle plat2 = new Rectangle(250, 300, 400, 20);
        Rectangle plat3 = new Rectangle(700, 600, 400, 20);

        Rectangle b1 = new Rectangle(700, 300, 50, 50);
        Rectangle b2 = new Rectangle(850, 350, 50, 50);
        Rectangle b3 = new Rectangle(1000, 500, 50, 50);
        Rectangle b4 = new Rectangle(1100, 650, 50, 50);

        Rectangle[] solidBlocks = { ground, plat1, plat2, plat3, b1, b2, b3, b4 };

        Rectangle ladder = new Rectangle(850, 600, 50, 150);

        float yVel1 = 0;
        float yVel2 = 0;
        float gravity = 1000f;
        float jump = -600f;
        float climbSpeed = -150f;
        bool onGround1 = false;
        bool onGround2 = false;
        bool onLadder1 = false;
        bool onLadder2 = false;
        bool facingRight1 = true;
        bool facingRight2 = true;

        int currentFrame1 = 0;
        int currentFrame2 = 0;
        float animTimer1 = 0f;
        float animTimer2 = 0f;
        float animSpeed = 0.2f;

        int maxHealth = 100;
        int player1Health = maxHealth;
        int player2Health = maxHealth;
        float gunLength = 70f;
        float gunHeight = 24f;
        int gunDamage = 20;
        float shotCooldown = 0.2f;
        float shotTimer = 0f;
        float bulletSpeed = 900f;
        float bulletRadius = 4f;
        float bulletLife = 2f;
        List<Bullet> bullets = new List<Bullet>();
        Random rng = new Random();
        float chestSize = 48f;
        float pickupGravity = 600f;
        List<Pickup> pickups = new List<Pickup>();
        ItemType[] inventory = new ItemType[3] { ItemType.Gun, ItemType.None, ItemType.None };
        int selectedSlot = 0;
        int maxShield = 20;
        int player1Shield = 0;

        GameState gameState = GameState.Menu;
        bool exitRequested = false;
        string winnerText = string.Empty;
        Rectangle playButton = new Rectangle(500, 500, 200, 60);
        Rectangle replayButton = new Rectangle(500, 480, 200, 60);
        Rectangle quitButton = new Rectangle(500, 560, 200, 60);
        int menuFrame = 0;
        float menuTimer = 0f;

        while (!Raylib.WindowShouldClose() && !exitRequested)
        {
            float deltaTime = Raylib.GetFrameTime();

            if (Raylib.IsKeyPressed(KeyboardKey.F11))
            {
                Raylib.ToggleFullscreen();
            }

            if (gameState == GameState.Menu)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed)
                {
                    menuFrame = (menuFrame + 1) % 2;
                    menuTimer = 0f;
                }

                GetRenderScale(virtualWidth, virtualHeight, out float scale, out Vector2 offset);
                Vector2 menuMouse = GetVirtualMousePosition(scale, offset);
                bool hoverPlay = Raylib.CheckCollisionPointRec(menuMouse, playButton);

                if (hoverPlay && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    ResetGame(
                        ref player1,
                        ref player2,
                        player1Start,
                        player2Start,
                        ref yVel1,
                        ref yVel2,
                        ref onGround1,
                        ref onGround2,
                        ref onLadder1,
                        ref onLadder2,
                        ref facingRight1,
                        ref facingRight2,
                        ref currentFrame1,
                        ref currentFrame2,
                        ref animTimer1,
                        ref animTimer2,
                        ref player1Health,
                        ref player2Health,
                        maxHealth,
                        ref player1Shield,
                        maxShield,
                        inventory,
                        ref selectedSlot,
                        ref shotTimer,
                        bullets,
                        pickups,
                        rng,
                        virtualWidth,
                        chestSize);
                    winnerText = string.Empty;
                    gameState = GameState.Playing;
                }

                Raylib.BeginTextureMode(renderTarget);
                Raylib.ClearBackground(Color.Black);

                DrawMenuHeader(runAnimRight, menuFrame);
                DrawButton(playButton, "Jouer", menuMouse);

                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            if (gameState == GameState.End)
            {
                menuTimer += deltaTime;
                if (menuTimer >= animSpeed)
                {
                    menuFrame = (menuFrame + 1) % 2;
                    menuTimer = 0f;
                }

                GetRenderScale(virtualWidth, virtualHeight, out float scale, out Vector2 offset);
                Vector2 menuMouse = GetVirtualMousePosition(scale, offset);
                bool hoverReplay = Raylib.CheckCollisionPointRec(menuMouse, replayButton);
                bool hoverQuit = Raylib.CheckCollisionPointRec(menuMouse, quitButton);

                if (hoverReplay && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    ResetGame(
                        ref player1,
                        ref player2,
                        player1Start,
                        player2Start,
                        ref yVel1,
                        ref yVel2,
                        ref onGround1,
                        ref onGround2,
                        ref onLadder1,
                        ref onLadder2,
                        ref facingRight1,
                        ref facingRight2,
                        ref currentFrame1,
                        ref currentFrame2,
                        ref animTimer1,
                        ref animTimer2,
                        ref player1Health,
                        ref player2Health,
                        maxHealth,
                        ref player1Shield,
                        maxShield,
                        inventory,
                        ref selectedSlot,
                        ref shotTimer,
                        bullets,
                        pickups,
                        rng,
                        virtualWidth,
                        chestSize);
                    winnerText = string.Empty;
                    gameState = GameState.Playing;
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
                    int winnerWidth = Raylib.MeasureText(winnerText, 30);
                    Raylib.DrawText(winnerText, (int)(virtualWidth / 2f - winnerWidth / 2f), 170, 30, Color.White);
                }

                DrawButton(replayButton, "Rejouer", menuMouse);
                DrawButton(quitButton, "Quitter", menuMouse);

                Raylib.EndTextureMode();
                DrawToScreen(renderTarget, virtualWidth, virtualHeight, scale, offset);
                continue;
            }

            GetRenderScale(virtualWidth, virtualHeight, out float gameplayScale, out Vector2 gameplayOffset);
            Vector2 aimMouse = GetVirtualMousePosition(gameplayScale, gameplayOffset);

            Texture2D player1Texture = UpdatePlayer(
                ref player1,
                ref yVel1,
                ref onGround1,
                ref onLadder1,
                ref facingRight1,
                ref currentFrame1,
                ref animTimer1,
                KeyboardKey.Left,
                KeyboardKey.Right,
                KeyboardKey.Up,
                speed,
                deltaTime,
                gravity,
                jump,
                climbSpeed,
                animSpeed,
                ladder,
                solidBlocks,
                runAnimRight,
                runAnimLeft,
                climbAnim);

            Texture2D player2Texture = UpdatePlayer(
                ref player2,
                ref yVel2,
                ref onGround2,
                ref onLadder2,
                ref facingRight2,
                ref currentFrame2,
                ref animTimer2,
                KeyboardKey.A,
                KeyboardKey.D,
                KeyboardKey.W,
                speed,
                deltaTime,
                gravity,
                jump,
                climbSpeed,
                animSpeed,
                ladder,
                solidBlocks,
                runAnimRight,
                runAnimLeft,
                climbAnim);

            if (Raylib.IsKeyPressed(KeyboardKey.One)) selectedSlot = 0;
            if (Raylib.IsKeyPressed(KeyboardKey.Two)) selectedSlot = 1;
            if (Raylib.IsKeyPressed(KeyboardKey.Three)) selectedSlot = 2;

            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                Pickup pickup = pickups[i];
                pickup.Velocity = new Vector2(pickup.Velocity.X, pickup.Velocity.Y + pickupGravity * deltaTime);
                Vector2 nextPos = pickup.Position + pickup.Velocity * deltaTime;
                Rectangle nextRect = new Rectangle(nextPos.X, nextPos.Y, chestSize, chestSize);

                if (pickup.Velocity.Y > 0f)
                {
                    foreach (Rectangle block in solidBlocks)
                    {
                        if (Raylib.CheckCollisionRecs(nextRect, block))
                        {
                            nextPos.Y = block.Y - chestSize;
                            pickup.Velocity = Vector2.Zero;
                            break;
                        }
                    }
                }

                pickup.Position = nextPos;
                if (pickup.Position.Y > virtualHeight + chestSize)
                {
                    pickups.RemoveAt(i);
                }
                else
                {
                    pickups[i] = pickup;
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.E))
            {
                for (int i = pickups.Count - 1; i >= 0; i--)
                {
                    Pickup pk = pickups[i];
                    Rectangle pickupRect = new Rectangle(pk.Position.X, pk.Position.Y, chestSize, chestSize);
                    if (!Raylib.CheckCollisionRecs(player1, pickupRect)) continue;

                    if (pk.Type == ItemType.Chest && !pk.IsOpened)
                    {
                        int slotToFill = inventory[1] == ItemType.None ? 1 : inventory[2] == ItemType.None ? 2 : -1;
                        if (slotToFill != -1)
                        {
                            ItemType[] dropTypes = { ItemType.HealPotion, ItemType.ShieldPotion, ItemType.NucPotion };
                            inventory[slotToFill] = dropTypes[rng.Next(dropTypes.Length)];
                            pk.IsOpened = true;
                            pickups[i] = pk;
                        }
                    }
                    break;
                }
            }

            shotTimer -= deltaTime;
            if (shotTimer < 0f) shotTimer = 0f;

            Vector2 player1Center = new Vector2(player1.X + player1.Width / 2f, player1.Y + player1.Height / 2f);
            Vector2 aimDir = aimMouse - player1Center;
            if (aimDir.LengthSquared() > 0.0001f)
            {
                aimDir = Vector2.Normalize(aimDir);
            }
            else
            {
                aimDir = new Vector2(1f, 0f);
            }

            Vector2 gunEnd = player1Center + aimDir * gunLength;

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (selectedSlot == 0 && shotTimer <= 0f)
                {
                    bullets.Add(new Bullet
                    {
                        Position = gunEnd,
                        Velocity = aimDir * bulletSpeed,
                        Radius = bulletRadius,
                        Life = bulletLife
                    });
                    shotTimer = shotCooldown;
                }
                else if (selectedSlot > 0 && inventory[selectedSlot] != ItemType.None)
                {
                    UseItem(
                        inventory[selectedSlot],
                        ref player1Health,
                        ref player1Shield,
                        maxHealth,
                        maxShield,
                        ref player2Health);
                    inventory[selectedSlot] = ItemType.None;
                }
            }

            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];
                bullet.Position += bullet.Velocity * deltaTime;
                bullet.Life -= deltaTime;

                bool remove = bullet.Life <= 0f
                    || bullet.Position.X < 0f
                    || bullet.Position.X > virtualWidth
                    || bullet.Position.Y < 0f
                    || bullet.Position.Y > virtualHeight;

                if (!remove && PointInRect(bullet.Position, player2))
                {
                    player2Health = Math.Max(0, player2Health - gunDamage);
                    remove = true;
                }

                if (!remove)
                {
                    foreach (Rectangle block in solidBlocks)
                    {
                        if (PointInRect(bullet.Position, block))
                        {
                            remove = true;
                            break;
                        }
                    }
                }

                if (remove)
                {
                    bullets.RemoveAt(i);
                }
                else
                {
                    bullets[i] = bullet;
                }
            }

            if (player1Health <= 0 || player2Health <= 0)
            {
                if (player1Health <= 0 && player2Health <= 0) winnerText = "Egalite";
                else if (player1Health <= 0) winnerText = "Victoire P2";
                else winnerText = "Victoire P1";
                gameState = GameState.End;
                continue;
            }

            Rectangle sourceRec1 = new Rectangle(0, 0, player1Texture.Width, player1Texture.Height);
            Rectangle sourceRec2 = new Rectangle(0, 0, player2Texture.Width, player2Texture.Height);

            Raylib.BeginTextureMode(renderTarget);
            Raylib.ClearBackground(Color.Black);

            Raylib.DrawRectangleRec(ground, Color.White);
            Raylib.DrawRectangleRec(plat1, Color.White);
            Raylib.DrawRectangleRec(plat2, Color.White);
            Raylib.DrawRectangleRec(plat3, Color.White);

            Raylib.DrawRectangleRec(b1, Color.White);
            Raylib.DrawRectangleRec(b2, Color.White);
            Raylib.DrawRectangleRec(b3, Color.White);
            Raylib.DrawRectangleRec(b4, Color.White);

            Raylib.DrawRectangleRec(ladder, Color.Gray);

            foreach (Pickup pickup in pickups)
            {
                Texture2D pickupTexture = pickup.Type == ItemType.Chest
                    ? (pickup.IsOpened ? chestOpenTexture : chestClosedTexture)
                    : GetItemTexture(pickup.Type, healPotionTexture, shieldPotionTexture, nucPotionTexture);
                Rectangle pickupSource = new Rectangle(0, 0, pickupTexture.Width, pickupTexture.Height);
                Rectangle pickupDest = new Rectangle(pickup.Position.X, pickup.Position.Y, chestSize, chestSize);
                Raylib.DrawTexturePro(pickupTexture, pickupSource, pickupDest, Vector2.Zero, 0f, Color.White);
            }

            Raylib.DrawTexturePro(player1Texture, sourceRec1, player1, Vector2.Zero, 0f, Color.White);
            Raylib.DrawTexturePro(player2Texture, sourceRec2, player2, Vector2.Zero, 0f, Color.White);

            if (player1Shield > 0)
            {
                Rectangle shieldSource = new Rectangle(0, 0, shieldTexture.Width, shieldTexture.Height);
                Rectangle shieldDest = new Rectangle(player1.X - 6f, player1.Y - 6f, player1.Width + 12f, player1.Height + 12f);
                Raylib.DrawTexturePro(shieldTexture, shieldSource, shieldDest, Vector2.Zero, 0f, new Color(255, 255, 255, 77));
            }

            if (selectedSlot == 0)
            {
                Texture2D gunTexture = aimDir.X >= 0f ? gunRight : gunLeft;
                Rectangle gunSource = new Rectangle(0, 0, gunTexture.Width, gunTexture.Height);
                Rectangle gunDest = new Rectangle(player1Center.X, player1Center.Y, gunLength, gunHeight);
                float gunAngle = MathF.Atan2(aimDir.Y, aimDir.X) * (180f / MathF.PI);
                float rotation = aimDir.X >= 0f ? gunAngle : gunAngle - 180f;
                Vector2 gunOrigin = aimDir.X >= 0f
                    ? new Vector2(0f, gunHeight / 2f)
                    : new Vector2(gunLength, gunHeight / 2f);
                Raylib.DrawTexturePro(gunTexture, gunSource, gunDest, gunOrigin, rotation, Color.White);
            }
            else if (selectedSlot > 0 && inventory[selectedSlot] != ItemType.None)
            {
                Texture2D heldTexture = GetItemTexture(inventory[selectedSlot], healPotionTexture, shieldPotionTexture, nucPotionTexture);
                Rectangle heldSource = new Rectangle(0, 0, heldTexture.Width, heldTexture.Height);
                Vector2 heldPos = player1Center + new Vector2(aimDir.X * 25f, aimDir.Y * 25f);
                Rectangle heldDest = new Rectangle(heldPos.X - 12f, heldPos.Y - 12f, 24f, 24f);
                Raylib.DrawTexturePro(heldTexture, heldSource, heldDest, Vector2.Zero, 0f, Color.White);
            }

            foreach (Bullet bullet in bullets)
            {
                Raylib.DrawCircleV(bullet.Position, bullet.Radius, Color.Yellow);
            }

            DrawHealthBars(virtualWidth, virtualHeight, player1Health, player2Health, maxHealth, crownTexture);

            DrawInventory(inventory, selectedSlot, healPotionTexture, shieldPotionTexture, nucPotionTexture, virtualHeight);

            Raylib.DrawText("F11 = Fullscreen | P1: Arrows + Mouse | P2: ZQSD", 10, 10, 20, Color.White);
            Raylib.DrawText("E = Ramasser | 1-3 = Equip | Click = Utiliser", 10, 90, 20, Color.White);

            Raylib.EndTextureMode();
            DrawToScreen(renderTarget, virtualWidth, virtualHeight, gameplayScale, gameplayOffset);
        }

        foreach (Texture2D tex in runAnimRight) Raylib.UnloadTexture(tex);
        foreach (Texture2D tex in runAnimLeft) Raylib.UnloadTexture(tex);
        foreach (Texture2D tex in climbAnim) Raylib.UnloadTexture(tex);
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
