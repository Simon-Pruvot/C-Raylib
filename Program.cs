using Raylib_cs;
using System.Numerics;

class Program
{
    static void Main()
    {
        int screenWidth = 1200;
        int screenHeight = 800;

        Raylib.InitWindow(screenWidth, screenHeight, "Test Simon1");
        Raylib.SetTargetFPS(120);

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

        Rectangle cube = new Rectangle(100, 500, 40, 60);
        float speed = 200f;

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

        float yVel = 0;
        float gravity = 1000f;
        float jump = -600f;
        bool onGround = false;
        bool onLadder = false;
        bool facingRight = true;

        int currentFrame = 0;
        float animTimer = 0f;
        float animSpeed = 0.2f;

        while (!Raylib.WindowShouldClose())
        {
            float deltaTime = Raylib.GetFrameTime();

            if (Raylib.IsKeyPressed(KeyboardKey.F11))
            {
                Raylib.ToggleFullscreen();
            }

            float moveX = 0;
            if (Raylib.IsKeyDown(KeyboardKey.Left)) moveX -= speed * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) moveX += speed * deltaTime;

            if (moveX > 0) facingRight = true;
            else if (moveX < 0) facingRight = false;

            cube.X += moveX;

            foreach (Rectangle block in solidBlocks)
            {
                if (Raylib.CheckCollisionRecs(cube, block) && !onLadder)
                {
                    if (moveX > 0)
                    {
                        cube.X = block.X - cube.Width;
                    }
                    else if (moveX < 0)
                    {
                        cube.X = block.X + block.Width;
                    }
                }
            }

            onLadder = Raylib.CheckCollisionRecs(cube, ladder);

            if (onLadder && Raylib.IsKeyDown(KeyboardKey.W))
            {
                yVel = -150f;
            }
            else
            {
                yVel += gravity * deltaTime;
            }

            cube.Y += yVel * deltaTime;

            onGround = false;

            foreach (Rectangle block in solidBlocks)
            {
                if (Raylib.CheckCollisionRecs(cube, block))
                {
                    if (yVel > 0)
                    {
                        cube.Y = block.Y - cube.Height;
                        yVel = 0;
                        onGround = true;
                    }
                    else if (yVel < 0 && !onLadder)
                    {
                        cube.Y = block.Y + block.Height;
                        yVel = 0;
                    }
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Space) && onGround)
            {
                yVel = jump;
            }

            animTimer += deltaTime;
            Texture2D currentTexture;

            Texture2D[] currentAnimArray = facingRight ? runAnimRight : runAnimLeft;

            if (onLadder)
            {
                currentFrame %= climbAnim.Length;
                
                if (Raylib.IsKeyDown(KeyboardKey.W))
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

            Rectangle sourceRec = new Rectangle(0, 0, currentTexture.Width, currentTexture.Height);

            Raylib.BeginDrawing();
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

            Raylib.DrawTexturePro(currentTexture, sourceRec, cube, Vector2.Zero, 0f, Color.White);

            Raylib.DrawText("F11 = Fullscreen | Z = Climb | SPACE = Jump", 10, 10, 20, Color.White);

            Raylib.EndDrawing();
        }

        foreach (Texture2D tex in runAnimRight) Raylib.UnloadTexture(tex);
        foreach (Texture2D tex in runAnimLeft) Raylib.UnloadTexture(tex);
        foreach (Texture2D tex in climbAnim) Raylib.UnloadTexture(tex);

        Raylib.CloseWindow();
    }
}