using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

partial class Program
{
    static void DrawMenuHeader(Texture2D[] runAnimRight, int menuFrame)
    {
        Texture2D menuTexture = runAnimRight[menuFrame];
        Rectangle menuSource = new Rectangle(0, 0, menuTexture.Width, menuTexture.Height);
        Rectangle menuDest = new Rectangle(540, 200, 120, 180);
        Raylib.DrawTexturePro(menuTexture, menuSource, menuDest, Vector2.Zero, 0f, Color.White);

        Raylib.DrawText("Yfight", 500, 100, 60, Color.White);
    }

    static void DrawButton(Rectangle button, string text, Vector2 mousePos)
    {
        bool hover = Raylib.CheckCollisionPointRec(mousePos, button);
        Color buttonColor = hover ? Color.Gray : Color.DarkGray;
        Raylib.DrawRectangleRec(button, buttonColor);

        int textWidth = Raylib.MeasureText(text, 30);
        int textX = (int)(button.X + (button.Width - textWidth) / 2f);
        int textY = (int)(button.Y + (button.Height - 30) / 2f);
        Raylib.DrawText(text, textX, textY, 30, Color.White);
    }

    static void ResetGame(
        ref Rectangle player1,
        ref Rectangle player2,
        Rectangle player1Start,
        Rectangle player2Start,
        ref float yVel1,
        ref float yVel2,
        ref bool onGround1,
        ref bool onGround2,
        ref bool onLadder1,
        ref bool onLadder2,
        ref bool facingRight1,
        ref bool facingRight2,
        ref int currentFrame1,
        ref int currentFrame2,
        ref float animTimer1,
        ref float animTimer2,
        ref int player1Health,
        ref int player2Health,
        int maxHealth,
        ref int player1Shield,
        int maxShield,
        ItemType[] inventory,
        ref int selectedSlot,
        ref float shotTimer,
        List<Bullet> bullets,
        List<Pickup> pickups,
        Random rng,
        int virtualWidth,
        float chestSize)
    {
        player1 = player1Start;
        player2 = player2Start;
        yVel1 = 0f;
        yVel2 = 0f;
        onGround1 = false;
        onGround2 = false;
        onLadder1 = false;
        onLadder2 = false;
        facingRight1 = true;
        facingRight2 = true;
        currentFrame1 = 0;
        currentFrame2 = 0;
        animTimer1 = 0f;
        animTimer2 = 0f;
        player1Health = maxHealth;
        player2Health = maxHealth;
        player1Shield = 0;
        shotTimer = 0f;
        bullets.Clear();
        pickups.Clear();
        inventory[0] = ItemType.Gun;
        inventory[1] = ItemType.None;
        inventory[2] = ItemType.None;
        selectedSlot = 0;

        if (rng.NextDouble() < 0.5)
        {
            float spawnX = rng.Next(40, virtualWidth - 40);
            pickups.Add(new Pickup
            {
                Type = ItemType.Chest,
                Position = new Vector2(spawnX, -chestSize),
                Velocity = Vector2.Zero,
                IsOpened = false
            });
        }
    }

    static float GetRandomRange(Random rng, float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }

    static Texture2D GetItemTexture(ItemType item, Texture2D heal, Texture2D shield, Texture2D nuc)
    {
        return item switch
        {
            ItemType.HealPotion => heal,
            ItemType.ShieldPotion => shield,
            ItemType.NucPotion => nuc,
            _ => heal
        };
    }

    static void UseItem(
        ItemType item,
        ref int playerHealth,
        ref int playerShield,
        int maxHealth,
        int maxShield,
        ref int opponentHealth)
    {
        switch (item)
        {
            case ItemType.HealPotion:
                playerHealth = Math.Min(maxHealth, playerHealth + 50);
                break;
            case ItemType.ShieldPotion:
                playerShield = maxShield;
                break;
            case ItemType.NucPotion:
                opponentHealth = 0;
                break;
        }
    }

    static void DrawInventory(
        ItemType[] inventory,
        int selectedSlot,
        Texture2D healTexture,
        Texture2D shieldTexture,
        Texture2D nucTexture,
        int screenHeight)
    {
        int slotSize = 50;
        int slotPadding = 10;
        int invX = 10;
        int invY = screenHeight - slotSize - 10;

        for (int i = 0; i < inventory.Length; i++)
        {
            Rectangle slotRect = new Rectangle(invX + i * (slotSize + slotPadding), invY, slotSize, slotSize);
            Raylib.DrawRectangleRec(slotRect, Color.DarkGray);

            if (i == selectedSlot)
            {
                Raylib.DrawRectangleLinesEx(slotRect, 3f, Color.Yellow);
            }
            else
            {
                Raylib.DrawRectangleLinesEx(slotRect, 2f, Color.Gray);
            }

            string number = (i + 1).ToString();
            Raylib.DrawText(number, (int)slotRect.X + 4, (int)slotRect.Y + 2, 16, Color.White);

            if (i == 0)
            {
                Raylib.DrawText("*", (int)slotRect.X + 20, (int)slotRect.Y + 14, 24, Color.White);
                continue;
            }

            if (inventory[i] != ItemType.None)
            {
                Texture2D itemTexture = GetItemTexture(inventory[i], healTexture, shieldTexture, nucTexture);
                Rectangle itemSource = new Rectangle(0, 0, itemTexture.Width, itemTexture.Height);
                Rectangle itemDest = new Rectangle(slotRect.X + 9, slotRect.Y + 9, 32, 32);
                Raylib.DrawTexturePro(itemTexture, itemSource, itemDest, Vector2.Zero, 0f, Color.White);
            }
        }
    }

    static void GetRenderScale(int virtualWidth, int virtualHeight, out float scale, out Vector2 offset)
    {
        int windowWidth = Raylib.GetScreenWidth();
        int windowHeight = Raylib.GetScreenHeight();
        scale = MathF.Min(windowWidth / (float)virtualWidth, windowHeight / (float)virtualHeight);
        offset = new Vector2(
            (windowWidth - virtualWidth * scale) / 2f,
            (windowHeight - virtualHeight * scale) / 2f);
    }

    static Vector2 GetVirtualMousePosition(float scale, Vector2 offset)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        return new Vector2(
            (mouse.X - offset.X) / scale,
            (mouse.Y - offset.Y) / scale);
    }

    static void LoadMap(
        int mapIndex,
        out Rectangle[] solidBlocks,
        out Rectangle[] ladders,
        out Rectangle player1Start,
        out Rectangle player2Start)
    {
        if (mapIndex == 0)
        {
            Rectangle ground = new Rectangle(0, 750, 1200, 50);
            Rectangle plat1 = new Rectangle(0, 500, 200, 20);
            Rectangle plat2 = new Rectangle(250, 300, 400, 20);
            Rectangle plat3 = new Rectangle(700, 600, 400, 20);
            Rectangle b1 = new Rectangle(700, 300, 50, 50);
            Rectangle b2 = new Rectangle(850, 350, 50, 50);
            Rectangle b3 = new Rectangle(1000, 500, 50, 50);
            Rectangle b4 = new Rectangle(1100, 650, 50, 50);

            solidBlocks = new[] { ground, plat1, plat2, plat3, b1, b2, b3, b4 };
            ladders = new[] { new Rectangle(850, 600, 50, 150) };
            player1Start = new Rectangle(100, 500, 40, 60);
            player2Start = new Rectangle(180, 500, 40, 60);
        }
        else
        {
            // Off-screen safety floor so players in pits don't fall forever.
            Rectangle safetyFloor = new Rectangle(0, 800, 1200, 100);
            Rectangle bottomCenter = new Rectangle(435, 765, 215, 35);
            Rectangle topPlat = new Rectangle(325, 180, 465, 20);
            Rectangle midPlat = new Rectangle(240, 440, 220, 20);
            Rectangle leftPlat = new Rectangle(0, 470, 210, 20);
            Rectangle rightPlat = new Rectangle(730, 660, 470, 20);
            Rectangle block1 = new Rectangle(535, 580, 50, 40);
            Rectangle block2 = new Rectangle(380, 660, 50, 30);

            solidBlocks = new[] { safetyFloor, bottomCenter, topPlat, midPlat, leftPlat, rightPlat, block1, block2 };
            ladders = new[]
            {
                new Rectangle(635, 0, 50, 180),
                new Rectangle(370, 200, 50, 240)
            };
            player1Start = new Rectangle(50, 410, 40, 60);
            player2Start = new Rectangle(1000, 600, 40, 60);
        }
    }

    static void DrawHealthBars(int virtualWidth, int virtualHeight, int p1Health, int p2Health, int maxHealth, Texture2D crown)
    {
        int barW = 50;
        int barH = (int)(virtualHeight * 0.70f);
        int barY = (int)(virtualHeight * 0.15f);
        int margin = 100;

        int p1X = margin;
        int p2X = virtualWidth - margin - barW;

        float p1Ratio = p1Health / (float)maxHealth;
        float p2Ratio = p2Health / (float)maxHealth;

        Color dimRed = new Color(180, 80, 80, 255);
        Color red = Color.Red;

        // Bright red fills from the bottom; the lost HP shows as dim red on top.
        Raylib.DrawRectangle(p1X, barY, barW, barH, dimRed);
        int p1Fill = (int)(barH * p1Ratio);
        Raylib.DrawRectangle(p1X, barY + (barH - p1Fill), barW, p1Fill, red);

        Raylib.DrawRectangle(p2X, barY, barW, barH, dimRed);
        int p2Fill = (int)(barH * p2Ratio);
        Raylib.DrawRectangle(p2X, barY + (barH - p2Fill), barW, p2Fill, red);

        int hpFontSize = 40;
        int hpY = barY - hpFontSize - 10;

        string p1Str = p1Health.ToString();
        string p2Str = p2Health.ToString();

        int p1TextW = Raylib.MeasureText(p1Str, hpFontSize);
        int p2TextW = Raylib.MeasureText(p2Str, hpFontSize);

        int p1TextX = p1X + (barW - p1TextW) / 2;
        int p2TextX = p2X + (barW - p2TextW) / 2;

        Raylib.DrawText(p1Str, p1TextX, hpY, hpFontSize, Color.LightGray);
        Raylib.DrawText(p2Str, p2TextX, hpY, hpFontSize, Color.LightGray);

        // Crown above the leading player (or both if tied)
        int crownSize = 60;
        int crownY = hpY - crownSize - 4;
        Rectangle crownSrc = new Rectangle(0, 0, crown.Width, crown.Height);

        bool p1Leading = p1Health >= p2Health;
        bool p2Leading = p2Health >= p1Health;

        if (p1Leading)
        {
            int crownX = p1X + (barW - crownSize) / 2;
            Raylib.DrawTexturePro(crown, crownSrc, new Rectangle(crownX, crownY, crownSize, crownSize), Vector2.Zero, 0f, Color.White);
        }
        if (p2Leading)
        {
            int crownX = p2X + (barW - crownSize) / 2;
            Raylib.DrawTexturePro(crown, crownSrc, new Rectangle(crownX, crownY, crownSize, crownSize), Vector2.Zero, 0f, Color.White);
        }
    }

    static void DrawToScreen(RenderTexture2D target, int virtualWidth, int virtualHeight, float scale, Vector2 offset)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Rectangle source = new Rectangle(0, 0, target.Texture.Width, -target.Texture.Height);
        Rectangle dest = new Rectangle(offset.X, offset.Y, virtualWidth * scale, virtualHeight * scale);
        Raylib.DrawTexturePro(target.Texture, source, dest, Vector2.Zero, 0f, Color.White);

        Raylib.EndDrawing();
    }
}
