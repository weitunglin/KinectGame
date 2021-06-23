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

namespace KinectGame
{
    public enum GameStatus
    {
        NotStartYet,
        Gaming,
        Pause,
        StartFromPause,
        GameOver
    }

    public enum Joint
    {
        Lefthand = 9,
        Righthand = 6,
        RightElbow = 7,
        Other = -1
    }

    public class Game
    {
        public GameStatus gameStatus { get; set; }
        private Random random = null;
        private Canvas canvas = null;
        private Canvas pointcanvas = null;
        private DispatcherTimer timer = null;
        private DispatcherTimer dt = null;
        private DispatcherTimer pointTimer = null;
        private double imageSourceWidth;
        private double imageSourceHeight;
        private double imageSourceBoarder;
        private List<BaseObject> objects = null;
        private MainWindow GameWindow = null;
        public int PlayerHealth { get; set; }
        public int PlayerScore { get; set; }
        public int TotalTime;
        private int Minutes { get { return TotalTime / 60; } }
        private int Seconds { get { return TotalTime % 60; } }
        public int CountDown;
        public int pointcountdown;
        private TextBlock points;
        private BaseObject CurrentTouchedobj;
        public List<BaseObject> getObjects() { return objects; }

        public Game(Canvas c, Canvas p, double sourceWidth, double sourceHeight, double sourceBoarder, MainWindow _GameWindow)
        {
            this.canvas = c;
            this.pointcanvas = p;
            this.random = new Random();
            this.imageSourceHeight = sourceHeight;
            this.imageSourceWidth = sourceWidth;
            this.imageSourceBoarder = sourceBoarder;
            this.GameWindow = _GameWindow;
            this.objects = new List<BaseObject>();
            this.gameStatus = GameStatus.NotStartYet;
            this.PlayerScore = 0;
            this.PlayerHealth = 3;
            TotalTime = 100; ///////////////////////////////////////////////////////////
            CountDown = 3;
            pointcountdown = 2;
            points = null;
            CurrentTouchedobj = null;

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(randomObjects);
            timer.Interval = new TimeSpan(0, 0, 2);

            dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 1);
            dt.Tick += dtTicker;

            pointTimer = new DispatcherTimer();
            pointTimer.Interval = new TimeSpan(0, 0, 1);
            pointTimer.Tick += pointtimertick;
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
                while (!NoDuplicate)
                {
                    x = random.Next(192, 1920 - 192 - 100);
                    y = random.Next(108, 1080 - 108 - 100);
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
                    Uid = item.Id,
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
            if (CountDown > 0)
            {
                GameWindow.countDownTxt.Visibility = Visibility.Visible;

                switch (CountDown)
                {
                    case 3:
                        GameWindow.countDownTxt.Text = "3";
                        break;
                    case 2:
                        GameWindow.countDownTxt.Text = "2";
                        break;
                    case 1:
                        GameWindow.countDownTxt.Text = "1";
                        break;
                }
                CountDown--;
                return;
            }
            else
            {
                GameWindow.countDownTxt.Visibility = Visibility.Hidden;
                timer.Start();
                TotalTime--;
                if (TotalTime <= 0)
                {
                    endGame();
                }
                else if (TotalTime <= 10)
                {
                    GameWindow.GameTimer.Foreground = Brushes.Red;
                    GameWindow.GameTimer.Text = Minutes.ToString() + ":" + Seconds.ToString();
                }
                else
                {
                    GameWindow.GameTimer.Text = Minutes.ToString() + ":" + Seconds.ToString();
                }
            }
        }

        public void StartGame()
        {
            if (gameStatus == GameStatus.NotStartYet)
            {

                PlayerScore = 0;
                PlayerHealth = 3;
                timer = new DispatcherTimer();
                timer.Tick += new EventHandler(randomObjects);
                timer.Interval = new TimeSpan(0, 0, 2);
                TotalTime = 100;
                CountDown = 3;
                dt = new DispatcherTimer();
                dt.Interval = new TimeSpan(0, 0, 1);
                dt.Tick += dtTicker;
                GameWindow.SumupGroup.Visibility = Visibility.Hidden;
                GameWindow.startBtn.Visibility = Visibility.Hidden;
                GameWindow.BackgroundImage.Visibility = Visibility.Hidden;
                GameWindow.Health_bar.Value = PlayerHealth;
                GameWindow.Score_Text.Text = PlayerScore.ToString();
                GameWindow.GameTimer.Text = Minutes.ToString() + ":" + Seconds.ToString();
                GameWindow.GameTimer.Foreground = Brushes.Black;
                dt.Start();

            }
            else if (gameStatus == GameStatus.Pause)
            {
                GameWindow.countDownTxt.Visibility = Visibility.Hidden;
                timer.Stop();
                dt.Stop();
            }
            else if (gameStatus == GameStatus.StartFromPause)
            {
                CountDown = 3;
                dt.Start();
            }
            else
            {
                canvas.Children.Clear();
                timer.Stop();
                dt.Stop();
            }
        }

        public void ObjectTouched(BaseObject TouchedObj, Joint TouchPart)
        {
            if (TouchPart != Joint.Other)
            {
                PlayerScore += TouchedObj.Credit;
                PlayerHealth += TouchedObj.Heart;
                bool ScoreHeart = false;
                if (PlayerHealth > 3)
                {

                    ScoreHeart = true;
                    PlayerHealth = 3;
                    PlayerScore += 3; // over health score

                }
                else if (PlayerHealth <= 0)
                {
                    PlayerHealth = 0;
                    endGame(true);
                }
                GameWindow.Health_bar.Value = PlayerHealth;
                GameWindow.Score_Text.Text = PlayerScore.ToString();
               // GameWindow.Sum_Score_Title.Content = "Score Sum: " + PlayerScore.ToString();
               // GameWindow.Sum_Life_Title.Content = "Life Left: " + PlayerHealth.ToString();
                for (int i = canvas.Children.Count - 1; i >= 0; --i)
                {
                    if (canvas.Children[i].Uid == TouchedObj.Id)
                    {
                        canvas.Children.RemoveAt(i);
                        break;
                    }
                }
                CurrentTouchedobj = TouchedObj;
                show_points(TouchedObj, ScoreHeart);
            }
        }

        private void endGame(bool dead = false)
        {
            dt.Stop();
            timer.Stop();
            canvas.Children.Clear();
            GameWindow.GameTimer.Text = "Timeout";

            GameWindow.Sum_Score_Title.Content = "Score Sum: " + PlayerScore.ToString();
            if (!dead)
            {
                GameWindow.Sum_Life_Title.Content = "Life Left: " + PlayerHealth.ToString();
            }
            else
            {

                GameWindow.Sum_Life_Title.Content = "YOU ARE DEAD";
                GameWindow.Sum_Life_Title.Foreground = Brushes.Red;
            }
            GameWindow.SumupGroup.Visibility = Visibility.Visible;
            gameStatus = GameStatus.GameOver;
            GameWindow.startBtn.Visibility = Visibility.Visible;
            GameWindow.BackgroundImage.Visibility = Visibility.Visible;
        }

        private void show_points(BaseObject TouchedObj, bool ScoreHeart)
        {
            points = new TextBlock();
            points.FontFamily = new FontFamily("Showcard Gothic");
            points.FontSize = 60;
            points.Foreground = Brushes.Red;
            if (ScoreHeart == false)
            {
                points.Text = "+" + TouchedObj.Credit.ToString();
            }
            else
            {
                points.Text = "+3";
            }
            pointcountdown = 2;
            Canvas.SetLeft(points, CurrentTouchedobj.Position.X * 1280.0 / 1920.0);
            Canvas.SetTop(points, (CurrentTouchedobj.Position.Y * 720.0 / 1080.0));
            pointcanvas.Children.Add(points);
            pointTimer.Start();
        }

        private void pointtimertick(object sender, EventArgs e)
        {

            switch (pointcountdown)
            {
                case 2:
                    pointcanvas.Children.Clear();
                    Canvas.SetLeft(points, CurrentTouchedobj.Position.X * 1280.0 / 1920.0);
                    Canvas.SetTop(points, (CurrentTouchedobj.Position.Y * 720.0 / 1080.0) - 25);
                    pointcanvas.Children.Add(points);
                    break;
                case 1:
                    pointcanvas.Children.Clear();
                    Canvas.SetLeft(points, CurrentTouchedobj.Position.X * 1280.0 / 1920.0);
                    Canvas.SetTop(points, (CurrentTouchedobj.Position.Y * 720.0 / 1080.0) - 50);
                    pointcanvas.Children.Add(points);
                    break;
                case 0:
                    pointcanvas.Children.Clear();          
                    pointTimer.Stop();
                    break;
            }
            pointcountdown--;
        }
    }
}