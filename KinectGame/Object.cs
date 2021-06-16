using System.Windows;

namespace KinectGame {
    public enum ObjectType
    {
        Apple,
        Bubble,
        Bomb,
        Heart
    }

    public class BaseObject
    {
        public string Id { get; set; }

        public ObjectType Type { get; set; }

        public Point Position { get; set; }

        public bool IsTouched { get; set; }

        public int Credit { get; set; }

        public int Heart { get; set; }

        public BaseObject(string id, ObjectType type, Point position, bool isTouched)
        {
            Id = id;
            Type = type;
            Position = position;
            IsTouched = isTouched;
        }
    }

    public class Apple : BaseObject
    {
        public Apple(string id, Point position, bool isTouched) : base(id, ObjectType.Apple, position, isTouched)
        {
           Credit = 10; 
           Heart = 0;
        }
    }

    public class Bubble : BaseObject
    {
        public Bubble(string id, Point position, bool isTouched) : base(id, ObjectType.Bubble, position, isTouched)
        {
            Credit = 5;
            Heart = 0;
        }
    }

    public class Bomb : BaseObject
    {
        public Bomb(string id, Point position, bool isTouched) : base(id, ObjectType.Bomb, position, isTouched)
        {
            Credit = 0;
            Heart = -1;
        }
    }

    public class Heart : BaseObject
    {
        public Heart(string id, Point position, bool isTouched) : base(id, ObjectType.Heart, position, isTouched)
        {
            Credit = 0;
            Heart = 1;
        }
    } 
}