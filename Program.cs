using Raylib_cs;

class Program
{
    static void Main()
    {
        int screenWidth = 1200;
        int screenHeight = 800;

        Raylib.InitWindow(screenWidth, screenHeight, "Test Simon1");
        Raylib.SetTargetFPS(60);

        Rectangle cube = new Rectangle(100, 500, 50, 50);
        float speed = 200f;
        // platforms
        Rectangle ground = new Rectangle(0, 750, 1200, 50);

        Rectangle plat1 = new Rectangle(0, 500, 200, 20);
        Rectangle plat2 = new Rectangle(250, 300, 400, 20);

        Rectangle plat3 = new Rectangle(700, 600, 400, 20);

        // small jump blocks
        Rectangle b1 = new Rectangle(700, 300, 50, 50);
        Rectangle b2 = new Rectangle(850, 350, 50, 50);
        Rectangle b3 = new Rectangle(1000, 500, 50, 50);
        Rectangle b4 = new Rectangle(1100, 650, 50, 50);

        // ladder
        Rectangle ladder = new Rectangle(850, 600, 50, 150);

        float yVel = 0;
        float gravity = 800f;
        float jump = -350f;
        bool onGround = false;

        bool onLadder = false;

        while (!Raylib.WindowShouldClose())
        {
            float deltaTime = Raylib.GetFrameTime();

            if (Raylib.IsKeyPressed(KeyboardKey.Enter) &&
               (Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt)))
            {
                int display = Raylib.GetCurrentMonitor();

                if (Raylib.IsWindowFullscreen())
                {
                    Raylib.SetWindowSize(screenWidth, screenHeight);
                }
                else
                {
                    Raylib.SetWindowSize(
                        Raylib.GetMonitorWidth(display),
                        Raylib.GetMonitorHeight(display)
                    );
                }

                Raylib.ToggleFullscreen();
            }

            // monte desands 
            if (Raylib.CheckCollisionRecs(cube, ladder))
            {
                onLadder = true;
            }
            else
            {
                onLadder = false;
            }
            
            // si on z pas, on tombe
            if (!onLadder || !Raylib.IsKeyDown(KeyboardKey.Z))
            {
                yVel += gravity * deltaTime;
            }

            // climb
            if (onLadder && Raylib.IsKeyDown(KeyboardKey.Z))
            {
                yVel = -150f; // go up
            }

            // Movement
            if (Raylib.IsKeyDown(KeyboardKey.Left)) cube.X -= speed * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) cube.X += speed * deltaTime;

            // gravity
            yVel += gravity * deltaTime;
            cube.Y += yVel * deltaTime;

            
            // ground
            if (
                yVel >= 0 && // only when falling
                cube.X + cube.Width > ground.X &&
                cube.X < ground.X + ground.Width &&
                cube.Y + cube.Height >= ground.Y &&
                cube.Y + cube.Height <= ground.Y + 10 
            )
            {
                cube.Y = ground.Y - cube.Height;
                yVel = 0;
                onGround = true;
            }
            else
            {
                onGround = false;
            }

            // jump
            if (Raylib.IsKeyPressed(KeyboardKey.Space) && onGround)
            {
                yVel = jump;
            }

            // affichage
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);

            Raylib.DrawRectangleRec(ground, Color.LightGray);

            Raylib.DrawRectangleRec(plat1, Color.LightGray);
            Raylib.DrawRectangleRec(plat2, Color.LightGray);
            Raylib.DrawRectangleRec(plat3, Color.LightGray);

            Raylib.DrawRectangleRec(b1, Color.Gray);
            Raylib.DrawRectangleRec(b2, Color.Gray);
            Raylib.DrawRectangleRec(b3, Color.Gray);
            Raylib.DrawRectangleRec(b4, Color.Gray);

            Raylib.DrawRectangleRec(ladder, Color.White);

            Raylib.DrawRectangleRec(cube, Color.Red);

            Raylib.DrawText("ALT + ENTER = Fullscreen", 10, 10, 20, Color.Black);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}