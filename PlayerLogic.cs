using System.Numerics;
using Raylib_cs;

partial class Program
{
    static Texture2D UpdatePlayer(
        ref Rectangle player,
        ref float yVel,
        ref bool onGround,
        ref bool onLadder,
        ref bool facingRight,
        ref int currentFrame,
        ref float animTimer,
        KeyboardKey leftKey,
        KeyboardKey rightKey,
        KeyboardKey upKey,
        float speed,
        float deltaTime,
        float gravity,
        float jump,
        float climbSpeed,
        float animSpeed,
        Rectangle ladder,
        Rectangle[] solidBlocks,
        Texture2D[] runAnimRight,
        Texture2D[] runAnimLeft,
        Texture2D[] climbAnim)
    {
        float moveX = 0;
        if (Raylib.IsKeyDown(leftKey)) moveX -= speed * deltaTime;
        if (Raylib.IsKeyDown(rightKey)) moveX += speed * deltaTime;

        if (moveX > 0) facingRight = true;
        else if (moveX < 0) facingRight = false;

        player.X += moveX;

        foreach (Rectangle block in solidBlocks)
        {
            if (Raylib.CheckCollisionRecs(player, block) && !onLadder)
            {
                if (moveX > 0)
                {
                    player.X = block.X - player.Width;
                }
                else if (moveX < 0)
                {
                    player.X = block.X + block.Width;
                }
            }
        }

        onLadder = Raylib.CheckCollisionRecs(player, ladder);

        if (onLadder && Raylib.IsKeyDown(upKey))
        {
            yVel = climbSpeed;
        }
        else
        {
            yVel += gravity * deltaTime;
        }

        player.Y += yVel * deltaTime;

        onGround = false;

        foreach (Rectangle block in solidBlocks)
        {
            if (Raylib.CheckCollisionRecs(player, block))
            {
                if (yVel > 0)
                {
                    player.Y = block.Y - player.Height;
                    yVel = 0;
                    onGround = true;
                }
                else if (yVel < 0 && !onLadder)
                {
                    player.Y = block.Y + block.Height;
                    yVel = 0;
                }
            }
        }

        if (Raylib.IsKeyPressed(upKey) && onGround && !onLadder)
        {
            yVel = jump;
        }

        animTimer += deltaTime;
        Texture2D currentTexture;
        Texture2D[] currentAnimArray = facingRight ? runAnimRight : runAnimLeft;

        if (onLadder)
        {
            currentFrame %= climbAnim.Length;

            if (Raylib.IsKeyDown(upKey))
            {
                if (animTimer >= animSpeed)
                {
                    currentFrame = (currentFrame + 1) % climbAnim.Length;
                    animTimer = 0f;
                }
            }
            else
            {
                currentFrame = 0;
            }
            currentTexture = climbAnim[currentFrame];
        }
        else if (moveX != 0 && onGround)
        {
            currentFrame %= currentAnimArray.Length;

            if (animTimer >= animSpeed)
            {
                currentFrame = (currentFrame + 1) % currentAnimArray.Length;
                animTimer = 0f;
            }
            currentTexture = currentAnimArray[currentFrame];
        }
        else
        {
            currentFrame = 0;
            currentTexture = currentAnimArray[0];
        }

        return currentTexture;
    }

    static bool PointInRect(Vector2 point, Rectangle rect)
    {
        return point.X >= rect.X
            && point.X <= rect.X + rect.Width
            && point.Y >= rect.Y
            && point.Y <= rect.Y + rect.Height;
    }
}
