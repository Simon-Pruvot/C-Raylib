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
        PlayerInput input,
        float speed,
        float deltaTime,
        float gravity,
        float jump,
        float climbSpeed,
        float animSpeed,
        Rectangle[] ladders,
        Rectangle[] solidBlocks,
        Texture2D[] runAnimRight,
        Texture2D[] runAnimLeft,
        Texture2D[] climbAnim)
    {
        float moveX = 0;
        if (input.MoveLeft)  moveX -= speed * deltaTime;
        if (input.MoveRight) moveX += speed * deltaTime;

        if (moveX > 0) facingRight = true;
        else if (moveX < 0) facingRight = false;

        player.X += moveX;

        foreach (Rectangle block in solidBlocks)
        {
            if (Raylib.CheckCollisionRecs(player, block) && !onLadder)
            {
                if (moveX > 0)      player.X = block.X - player.Width;
                else if (moveX < 0) player.X = block.X + block.Width;
            }
        }

        onLadder = false;
        foreach (Rectangle l in ladders)
        {
            if (Raylib.CheckCollisionRecs(player, l)) { onLadder = true; break; }
        }

        if (onLadder && input.ClimbUp)
            yVel = climbSpeed;
        else
            yVel += gravity * deltaTime;

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

        if (input.JumpPressed && onGround && !onLadder)
            yVel = jump;

        animTimer += deltaTime;
        Texture2D[] currentAnimArray = facingRight ? runAnimRight : runAnimLeft;

        if (onLadder)
        {
            currentFrame %= climbAnim.Length;
            if (input.ClimbUp)
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
            return climbAnim[currentFrame];
        }
        else if (moveX != 0 && onGround)
        {
            currentFrame %= currentAnimArray.Length;
            if (animTimer >= animSpeed)
            {
                currentFrame = (currentFrame + 1) % currentAnimArray.Length;
                animTimer = 0f;
            }
            return currentAnimArray[currentFrame];
        }
        else
        {
            currentFrame = 0;
            return currentAnimArray[0];
        }
    }

    static bool PointInRect(Vector2 point, Rectangle rect)
    {
        return point.X >= rect.X
            && point.X <= rect.X + rect.Width
            && point.Y >= rect.Y
            && point.Y <= rect.Y + rect.Height;
    }
}
