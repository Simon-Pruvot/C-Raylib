using Raylib_cs;

Raylib.InitWindow(800, 600, "Mon Jeu");
Texture2D image = Raylib.LoadTexture("image.png");

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.White);
    Raylib.DrawTexture(image, 0, 0, Color.White);
    Raylib.EndDrawing();
}

Raylib.UnloadTexture(image);
Raylib.CloseWindow();