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
        ref float pickupSpawnTimer,
        Random rng,
        float pickupSpawnMin,
        float pickupSpawnMax)
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
        pickupSpawnTimer = GetRandomRange(rng, pickupSpawnMin, pickupSpawnMax);
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
