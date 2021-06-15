namespace KinectGame {
    enum Type
    {
        Apple,
        Bubble,
        Bomb,
        Heart
    }

    public class BaseObject
    {
        public string Id { get; set; }

        public Type Type { get; set; }

        public Point Position { get; set; }

        public bool IsTouched { get; set; }

        public int Credit { get; set; }

        public int Heart { get; set; }

        public Object(string id, Type type, Point position, bool isTouched)
        {
            Id = id;
            Type = type;
            Position = position;
            IsTouched = isTouched;
        }
    }

    public class Apple : BaseObject
    {
        public Apple(string id, Position position, bool isTouched) : base(id, Type.Apple, position, isTouched)
        {
           Credit = 10; 
           Heart = 0;
        }
    }

    public class Bubble : BaseObject
    {
        public Bubble(string id, Position position, bool isTouched) : base(id, Type.Bubble, position, isTouched)
        {
            Credit = 5;
            Heart = 0;
        }
    }

    public class Bomb : BaseObject
    {
        public Bomb(string id, Position position, bool isTouched) : base(id, Type.Bomb, position, isTouched)
        {
            Credit = 0;
            Heart = -1;
        }
    }

    public class Heart : BaseObject
    {
        public Heart(string id, Position position, bool isTouched) : base(id, Type.Heart, position, isTouched)
        {
            Credit = 0;
            Heart = 1;
        }
    } 
}