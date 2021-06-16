using System;
using System.Collections.Generic;
using System.Windows;

namespace KinectGame {
    public class Game {
        public Game()
        {

        }

        public List<BaseObject> getObjects()
        {
            return new List<BaseObject>() {
                new Apple(Guid.NewGuid().ToString(), new Point(200, 100), false),
                new Bubble(Guid.NewGuid().ToString(), new Point(500, 150), false) };
        }
    }
}