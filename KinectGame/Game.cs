using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KinectGame {
    public class Game {
        private Random random = null;
        private Canvas canvas = null;
        private double imageSourceWidth;
        private double imageSourceHeight;

        public Game(Canvas c, double sourceWidth, double sourceHeight)
        {
            this.canvas = c;
            this.random = new Random();
            this.imageSourceHeight = sourceHeight;
            this.imageSourceWidth = sourceWidth;
        }

        public List<BaseObject> getObjects()
        {
            this.canvas.Children.Clear();

            var objects = new List<BaseObject>() {
                new Apple(Guid.NewGuid().ToString(), new Point(random.Next(0, 1919), random.Next(0, 1079)), false),
                new Bubble(Guid.NewGuid().ToString(), new Point(random.Next(0, 1919), random.Next(0, 1079)), false) };

            foreach (var item in objects)
            {
                Ellipse myEllipse = new Ellipse();
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
                myEllipse.Fill = mySolidColorBrush;
                myEllipse.Width = 20;
                myEllipse.Height = 20;
                this.canvas.Children.Add(myEllipse);
                double X = item.Position.X * 1280.0 / 512;
                double Y = item.Position.Y * 720.0 / 424;
                if (0 < X && X < this.imageSourceWidth)
                {
                    if (0 < Y && Y < this.imageSourceHeight)
                    {
                        Canvas.SetLeft(myEllipse, X);
                        Canvas.SetTop(myEllipse, Y);
                    }
                }
            }

            return objects;
        }
    }
}