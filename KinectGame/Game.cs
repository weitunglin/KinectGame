using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KinectGame {
    public enum GameStatus{
        NotStartYet,
        Gaming,
        Pause,
        GameOver
    }
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
        }

        public List<BaseObject> getObjects()
        {
            return objects;
        }

        private void randomObjects(Object source, EventArgs e)
        {
            objects.Clear();
            canvas.Children.Clear();
            int objectNum = random.Next(2, 4);

            for (int i = 0; i < objectNum; ++i)
            {
                int type = random.Next(0, 2);
                int x = random.Next(0, 1920);
                int y = random.Next(0, 1080);
                string id = Guid.NewGuid().ToString();

                BaseObject item = null;

                switch (type)
                {
                    case 0:
                        item = new Apple(id, new Point(x, y), false);
                        break;
                    case 1:
                        item = new Bubble(id, new Point(x, y), false);
                        break;
                }

                Image image = new Image
                {
                    Width = 100,
                    Height = 100,
                    Source = new BitmapImage(item.ImageUri)
                };
                addObjectToCanvas(item.Position, image);
                objects.Add(item);
            }
        }

        private void addObjectToCanvas(Point pos, Image image)
        {
            canvas.Children.Add(image);
            double X = pos.X * 1280.0 / 1920.0;
            double Y = pos.Y * 720.0 / 1080.0;
            double marginX = 1280.0 / 10;
            double marginY = 720.0 / 10;
            if (marginX < X && X < this.imageSourceWidth - marginX)
            {
                if (marginY < Y && Y < this.imageSourceHeight - marginY)
                {
                    Canvas.SetLeft(image, X);
                    Canvas.SetTop(image, Y);
                }
            }
        }

        public void StartGame(GameStatus gameStatus)
        {
            if (gameStatus == GameStatus.NotStartYet)
            {
                timer.Start();
            } else
            {
                canvas.Children.Clear();
                timer.Stop();
            }
        }
    }
}