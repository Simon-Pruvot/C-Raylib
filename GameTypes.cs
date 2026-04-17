using System.Numerics;

partial class Program
{
    enum GameState
    {
        Menu,
        Playing,
        End
    }

    enum ItemType
    {
        None,
        Gun,
        HealPotion,
        ShieldPotion,
        NucPotion
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
    }
}
