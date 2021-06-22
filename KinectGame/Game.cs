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
        private double imageSourceBoarder;
        private List<BaseObject> objects = null;
        

        public Game(Canvas c, double sourceWidth, double sourceHeight, double sourceBoarder)
        {
            this.canvas = c;
            this.random = new Random();
            this.imageSourceHeight = sourceHeight;
            this.imageSourceWidth = sourceWidth;
            this.imageSourceBoarder = sourceBoarder;
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
                int type = random.Next(0, 4);
                int x = 0;
                int y = 0;
                string id = Guid.NewGuid().ToString();
                BaseObject item = null;

                bool NoDuplicate = false;
                while(!NoDuplicate)
                {
                    x = random.Next(0, 1920 - 192);
                    y = random.Next(0, 1080 - 108);
                    NoDuplicate = true;
                    foreach (BaseObject PositionCheck in objects)
                    {
                        Console.WriteLine(x.ToString() + " , " + y.ToString() + " / " + PositionCheck.Position.X.ToString() + " , " + PositionCheck.Position.Y.ToString());
                        if (x >= PositionCheck.Position.X - imageSourceBoarder && x <= PositionCheck.Position.X + 100 + imageSourceBoarder && y >= PositionCheck.Position.Y - imageSourceBoarder && y <= PositionCheck.Position.Y + 100 + imageSourceBoarder)
                        {
                            NoDuplicate = false;
                        }
                    }
                }

                switch (type)
                {
                    case 0:
                        item = new Apple(id, new Point(x, y), false);
                        break;
                    case 1:
                        item = new Bubble(id, new Point(x, y), false);
                        break;
                    case 2:
                        item = new Bomb(id, new Point(x, y), false);
                        break;
                    case 3:
                        item = new Heart(id, new Point(x, y), false);
                        break;
                }

                Image image = new Image
                {
                    Width = 100,
                    Height = 100,
                    Stretch = Stretch.Uniform,
                    Source = new BitmapImage(item.ImageUri)
                };
                addObjectToCanvas(item, image);
                objects.Add(item);
            }
        }

        private void addObjectToCanvas(BaseObject item, Image image)
        {
            
            double X = item.Position.X * 1280.0 / 1920.0;
            double Y = item.Position.Y * 720.0 / 1080.0;
            double marginX = 1280.0 / 10;
            double marginY = 720.0 / 10;
            Canvas.SetLeft(image, X);
            Canvas.SetTop(image, Y);
            item.IsTouched = true;
            if (marginX < X && X < this.imageSourceWidth - marginX)
            {
                if (marginY < Y && Y < this.imageSourceHeight - marginY)
                {
                    item.IsTouched = false;
                    Canvas.SetLeft(image, X);
                    Canvas.SetTop(image, Y);
                    canvas.Children.Add(image);
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