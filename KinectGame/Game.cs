using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KinectGame {
    public class Game {
        private Random random = null;
        private Canvas canvas = null;
        private DispatcherTimer timer = null;
        private double imageSourceWidth;
        private double imageSourceHeight;
        private List<BaseObject> objects = null;

        public Game(Canvas c, double sourceWidth, double sourceHeight)
        {
            this.canvas = c;
            this.random = new Random();
            this.imageSourceHeight = sourceHeight;
            this.imageSourceWidth = sourceWidth;

            this.objects = new List<BaseObject>();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(randomObjects);
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Start();
        }

        public List<BaseObject> getObjects()
        {
            return objects;
        }

        private void randomObjects(Object source, EventArgs e)
        {
            objects.Clear();
            int objectNum = random.Next(2, 4);

            for (int i = 0; i < objectNum; ++i)
            {
                int type = random.Next(0, 2);
                int x = random.Next(0, 1920);
                int y = random.Next(0, 1080);
                string id = Guid.NewGuid().ToString();

                switch (type)
                {
                    case 0:
                        objects.Add(new Apple(id, new Point(x, y), false));
                        break;
                    case 1:
                        objects.Add(new Bubble(id, new Point(x, y), false));
                        break;
                }
            }

            canvas.Children.Clear();

            foreach (var item in objects)
            {
                Ellipse myEllipse = new Ellipse();
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
                myEllipse.Fill = mySolidColorBrush;
                myEllipse.Width = 40;
                myEllipse.Height = 40;
                canvas.Children.Add(myEllipse);
                double X = item.Position.X * 1280.0 / 1920.0;
                double Y = item.Position.Y * 720.0 / 1080.0;
                if (0 < X && X < this.imageSourceWidth)
                {
                    if (0 < Y && Y < this.imageSourceHeight)
                    {
                        Canvas.SetLeft(myEllipse, X);
                        Canvas.SetTop(myEllipse, Y);
                    }
                }
            }
        }
    }
}