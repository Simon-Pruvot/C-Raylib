using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Raylib_cs;

partial class Program
{
    static void DrawMenuHeader(Texture2D[] runAnimRight, int menuFrame)
    {
        Texture2D tex = runAnimRight[menuFrame];
        Raylib.DrawTexturePro(tex, new Rectangle(0, 0, tex.Width, tex.Height),
            new Rectangle(540, 130, 120, 160), Vector2.Zero, 0f, Color.White);
        string title = "Yfight";
        int tw = Raylib.MeasureText(title, 64);
        Raylib.DrawText(title, (1200 - tw) / 2, 55, 64, Color.White);
    }

    static void DrawButton(Rectangle button, string text, Vector2 mousePos)
    {
        bool hover = Raylib.CheckCollisionPointRec(mousePos, button);
        Raylib.DrawRectangleRec(button, hover ? new Color(55, 55, 55, 255) : new Color(28, 28, 28, 255));
        Raylib.DrawRectangleLinesEx(button, hover ? 2f : 1f, hover ? Color.White : new Color(75, 75, 75, 255));
        int tw = Raylib.MeasureText(text, 22);
        Raylib.DrawText(text, (int)(button.X + (button.Width - tw) / 2f),
                             (int)(button.Y + (button.Height - 22) / 2f), 22, Color.White);
    }

    static Texture2D GetItemTexture(ItemType item, Texture2D heal, Texture2D shield, Texture2D nuc)
    {
        return item switch
        {
            ItemType.HealPotion   => heal,
            ItemType.ShieldPotion => shield,
            ItemType.NucPotion    => nuc,
            _                     => heal
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
            case ItemType.HealPotion:   playerHealth = Math.Min(maxHealth, playerHealth + 50); break;
            case ItemType.ShieldPotion: playerShield = maxShield; break;
            case ItemType.NucPotion:    opponentHealth = 0; break;
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
        int slotSize = 50, slotPadding = 10;
        int invX = 10, invY = screenHeight - slotSize - 10;

        for (int i = 0; i < inventory.Length; i++)
        {
            Rectangle slotRect = new Rectangle(invX + i * (slotSize + slotPadding), invY, slotSize, slotSize);
            Raylib.DrawRectangleRec(slotRect, Color.DarkGray);
            Raylib.DrawRectangleLinesEx(slotRect, i == selectedSlot ? 3f : 2f, i == selectedSlot ? Color.Yellow : Color.Gray);
            Raylib.DrawText((i + 1).ToString(), (int)slotRect.X + 4, (int)slotRect.Y + 2, 16, Color.White);

            if (i == 0) { Raylib.DrawText("*", (int)slotRect.X + 20, (int)slotRect.Y + 14, 24, Color.White); continue; }

            if (inventory[i] != ItemType.None)
            {
                Texture2D itemTexture = GetItemTexture(inventory[i], healTexture, shieldTexture, nucTexture);
                Raylib.DrawTexturePro(itemTexture,
                    new Rectangle(0, 0, itemTexture.Width, itemTexture.Height),
                    new Rectangle(slotRect.X + 9, slotRect.Y + 9, 32, 32),
                    Vector2.Zero, 0f, Color.White);
            }
        }
    }

    static void GetRenderScale(int virtualWidth, int virtualHeight, out float scale, out Vector2 offset)
    {
        int w = Raylib.GetScreenWidth(), h = Raylib.GetScreenHeight();
        scale = MathF.Min(w / (float)virtualWidth, h / (float)virtualHeight);
        offset = new Vector2((w - virtualWidth * scale) / 2f, (h - virtualHeight * scale) / 2f);
    }

    static Vector2 GetVirtualMousePosition(float scale, Vector2 offset)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        return new Vector2((mouse.X - offset.X) / scale, (mouse.Y - offset.Y) / scale);
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
            solidBlocks = new[] {
                new Rectangle(0, 750, 1200, 50),
                new Rectangle(0, 500, 200, 20),
                new Rectangle(250, 300, 400, 20),
                new Rectangle(700, 600, 400, 20),
                new Rectangle(700, 300, 50, 50),
                new Rectangle(850, 350, 50, 50),
                new Rectangle(1000, 500, 50, 50),
                new Rectangle(1100, 650, 50, 50)
            };
            ladders      = new[] { new Rectangle(850, 600, 50, 150) };
            player1Start = new Rectangle(100, 500, 40, 60);
            player2Start = new Rectangle(950, 600, 40, 60);
        }
        else
        {
            solidBlocks = new[] {
                new Rectangle(435, 765, 215, 35),
                new Rectangle(325, 180, 465, 20),
                new Rectangle(240, 440, 220, 20),
                new Rectangle(0, 470, 210, 20),
                new Rectangle(730, 660, 470, 20),
                new Rectangle(535, 580, 50, 40),
                new Rectangle(380, 660, 50, 30)
            };
            ladders = new[] {
                new Rectangle(548, 180, 50, 400), // petit bloc → plateforme haute
                new Rectangle(370, 180, 50, 260)  // plateforme milieu → plateforme haute
            };
            player1Start = new Rectangle(50, 410, 40, 60);
            player2Start = new Rectangle(1000, 600, 40, 60);
        }
    }

    static void DrawHealthBars(int virtualWidth, int virtualHeight, int p1Health, int p2Health, int maxHealth, Texture2D crown)
    {
        int barW = 50, barH = (int)(virtualHeight * 0.70f), barY = (int)(virtualHeight * 0.15f), margin = 100;
        int p1X = margin, p2X = virtualWidth - margin - barW;
        float p1Ratio = p1Health / (float)maxHealth, p2Ratio = p2Health / (float)maxHealth;
        Color dimRed = new Color(180, 80, 80, 255);

        Raylib.DrawRectangle(p1X, barY, barW, barH, dimRed);
        Raylib.DrawRectangle(p1X, barY + (barH - (int)(barH * p1Ratio)), barW, (int)(barH * p1Ratio), Color.Red);
        Raylib.DrawRectangle(p2X, barY, barW, barH, dimRed);
        Raylib.DrawRectangle(p2X, barY + (barH - (int)(barH * p2Ratio)), barW, (int)(barH * p2Ratio), Color.Red);

        int hpFontSize = 40, hpY = barY - hpFontSize - 10;
        string p1Str = p1Health.ToString(), p2Str = p2Health.ToString();
        Raylib.DrawText(p1Str, p1X + (barW - Raylib.MeasureText(p1Str, hpFontSize)) / 2, hpY, hpFontSize, Color.LightGray);
        Raylib.DrawText(p2Str, p2X + (barW - Raylib.MeasureText(p2Str, hpFontSize)) / 2, hpY, hpFontSize, Color.LightGray);

        int crownSize = 60, crownY = hpY - crownSize - 4;
        Rectangle crownSrc = new Rectangle(0, 0, crown.Width, crown.Height);
        if (p1Health >= p2Health)
            Raylib.DrawTexturePro(crown, crownSrc, new Rectangle(p1X + (barW - crownSize) / 2, crownY, crownSize, crownSize), Vector2.Zero, 0f, Color.White);
        if (p2Health >= p1Health)
            Raylib.DrawTexturePro(crown, crownSrc, new Rectangle(p2X + (barW - crownSize) / 2, crownY, crownSize, crownSize), Vector2.Zero, 0f, Color.White);
    }

    static void DrawToScreen(RenderTexture2D target, int virtualWidth, int virtualHeight, float scale, Vector2 offset)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawTexturePro(
            target.Texture,
            new Rectangle(0, 0, target.Texture.Width, -target.Texture.Height),
            new Rectangle(offset.X, offset.Y, virtualWidth * scale, virtualHeight * scale),
            Vector2.Zero, 0f, Color.White);
        Raylib.EndDrawing();
    }

    static PlayerInput GatherInput(
        bool left, bool right, bool climbUp, bool jumpPressed,
        bool action, bool pickup,
        Vector2 mouse, int selectedSlot)
    {
        return new PlayerInput
        {
            MoveLeft      = left,
            MoveRight     = right,
            ClimbUp       = climbUp,
            JumpPressed   = jumpPressed,
            ActionPressed = action,
            PickupPressed = pickup,
            MouseX        = mouse.X,
            MouseY        = mouse.Y,
            SelectedSlot  = selectedSlot,
        };
    }

    static void HandleTextInput(ref string text, int maxLen, Func<char, bool> filter)
    {
        int c;
        while ((c = Raylib.GetCharPressed()) > 0)
        {
            if (text.Length < maxLen && filter((char)c))
                text += (char)c;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && text.Length > 0)
            text = text[..^1];
    }

    static string GetLocalIp()
    {
        try
        {
            using var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.Connect("8.8.8.8", 80);
            return ((IPEndPoint)s.LocalEndPoint!).Address.ToString();
        }
        catch { }
        return "??.??.??.??";
    }
}
