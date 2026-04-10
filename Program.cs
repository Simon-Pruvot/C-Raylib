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
        Rectangle ground = new Rectangle(0, 550, screenWidth, 50);

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

            // Movement
            if (Raylib.IsKeyDown(KeyboardKey.Left)) cube.X -= speed * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) cube.X += speed * deltaTime;

            // Ground collision
            if (cube.Y + cube.Height > ground.Y)
                cube.Y = ground.Y - cube.Height;

            // Drawing
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);

            Raylib.DrawRectangleRec(ground, Color.DarkGray);
            Raylib.DrawRectangleRec(cube, Color.Red);

            Raylib.DrawText("ALT + ENTER = Fullscreen", 10, 10, 20, Color.Black);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}