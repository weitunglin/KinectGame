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
    public enum TouchPartEnum
    {
        rightHand,
        leftHand,
        Other
    }
    public class Game {
        private GameStatus gameStatus = GameStatus.NotStartYet;
        private Random random = null;
        private Canvas canvas = null;
        private DispatcherTimer timer = null;
        private DispatcherTimer dt = null;
        private double imageSourceWidth;
        private double imageSourceHeight;
        private double imageSourceBoarder;
        private List<BaseObject> objects = null;
        private TextBox GameTimer = null;
        private int PlayerHealth;
        private int PlayerScore;
        public int TotalTime;
        private int Minutes
        {
            get
            {
                return TotalTime / 60;
            }
        }
        private int Seconds
        {
            get
            {
                return TotalTime % 60;
            }
        }

        public Game(Canvas c, double sourceWidth, double sourceHeight, double sourceBoarder, TextBox _GameTimer)
        {
            this.canvas = c;
            this.random = new Random();
            this.imageSourceHeight = sourceHeight;
            this.imageSourceWidth = sourceWidth;
            this.imageSourceBoarder = sourceBoarder;
            this.GameTimer = _GameTimer;
            this.objects = new List<BaseObject>();
            this.PlayerScore = 0;
            this.PlayerHealth = 3;

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(randomObjects);
            timer.Interval = new TimeSpan(0, 0, 2);
            TotalTime = 15;
            dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 1);
            dt.Tick += dtTicker;
        }

        public List<BaseObject> getObjects()
        {
            return objects;
        }

        public GameStatus GetStatus()
        {
            return gameStatus;
        }

        public void SetStatus(GameStatus NewStatus)
        {
            this.gameStatus = NewStatus;
        }

        public int GetPlayerHealth()
        {
            return this.PlayerHealth;
        }

        public int GetPlayerScore()
        {
            return this.PlayerScore;
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

        private void dtTicker(object sender, EventArgs e)
        {
            TotalTime--;
            if (TotalTime <= 0)
            {

                endGame();
            }
            else if (TotalTime <= 10)
            {
                GameTimer.Foreground = Brushes.Red;
                GameTimer.Text = Minutes.ToString() + ":" + Seconds.ToString();
            }
            else
            {
                GameTimer.Text = Minutes.ToString() + ":" + Seconds.ToString();
            }

        }

        public void StartGame()
        {
            if (gameStatus == GameStatus.NotStartYet)
            {
                timer.Start();
                dt.Start();

            } else
            {
                canvas.Children.Clear();
                timer.Stop();
                dt.Stop();
            }
        }

        public void ObjectTouched(BaseObject TouchedObj, TouchPartEnum TouchPart)
        {
            if (TouchPart != TouchPartEnum.Other)
            {
                PlayerScore += TouchedObj.Credit;
                PlayerHealth += TouchedObj.Heart;
                if (PlayerHealth >= 3)
                {
                    PlayerHealth = 3;
                    PlayerScore += 3; // over health score
                }
                else if (PlayerHealth <= 0)
                {
                    PlayerHealth = 0;
                    endGame();
                }
            }
        }

        private void endGame()
        {
            dt.Stop();
            timer.Stop();
            canvas.Children.Clear();
            GameTimer.Text = "Timeout";
            gameStatus = GameStatus.GameOver;
        }
    }
}