using System.Numerics;

partial class Program
{
    enum GameState
    {
        Menu,
        Lobby,
        Connecting,
        Playing,
        End
    }

    enum NetworkMode { Local, Host, Client }

    enum ItemType
    {
        None,
        Gun,
        HealPotion,
        ShieldPotion,
        NucPotion,
        Chest
    }

    struct PlayerInput
    {
        public bool MoveLeft;
        public bool MoveRight;
        public bool ClimbUp;
        public bool JumpPressed;
        public bool ActionPressed;   // left click: shoot or use item
        public bool PickupPressed;
        public float MouseX;
        public float MouseY;
        public int SelectedSlot;
    }

    struct Bullet
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Radius;
        public float Life;
    }

    struct Pickup
    {
        public ItemType Type;
        public Vector2 Position;
        public Vector2 Velocity;
        public bool IsOpened;
    }
}
