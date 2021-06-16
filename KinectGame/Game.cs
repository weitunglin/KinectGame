using System;

namespace KinectGame {
    public class Game {
        public Game()
        {

        }

        public List<BaseObject> getObjects()
        {
            return new List<BaseObject>() {
                new Apple(new Guid.NewGuid(), new Position(200, 100), false),
                new Bubble(new Guid.NewGuid(), new Position(500, 150), false) };
        }
    }
}