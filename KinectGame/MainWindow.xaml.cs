using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Microsoft.Kinect;
using System.Diagnostics;
using System. Windows.Threading;

namespace KinectGame
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor = null;
        private FrameDescription frameDescription = null;
        private WriteableBitmap bitmap = null;

        private ColorFrameReader colorFrameReader = null;

        private BodyFrameReader bodyFrameReader = null;

        private Body[] bodies = null;

        private Game game = null;

        private List<ColorSpacePoint> pos = new List<ColorSpacePoint>();

        private float SpineShoudler { get; set; }

      
      
        private bool DEBUGMODE;

       

        public MainWindow()
        {
            InitializeComponent();

            this.sensor = KinectSensor.GetDefault();
            this.frameDescription = this.sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);

            this.colorFrameReader = this.sensor.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            this.bodyFrameReader = this.sensor.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

            this.bitmap = new WriteableBitmap(this.frameDescription.Width, this.frameDescription.Height, 96, 96, PixelFormats.Bgr32, null);
            this.sensor.Open();

            this.game = new Game(this.ImageCanvas,this.PointCanvas, this.ImageSource.Width, this.ImageSource.Height, 150, this);
            DEBUGMODE = false;

            

            FlowDocument pauseDoc = new FlowDocument();
            Paragraph pause = new Paragraph();
            pause.Background = Brushes.LightSalmon;
            // from allen, do not blame me
            pause.Inlines.Add(new Run("\n\nPaused\n\n") { FontSize = 26, FontFamily = new FontFamily("Showcard Gothic") });
            pause.Inlines.Add(new Run("Please stand a little further!") { FontSize = 36, Foreground = Brushes.Red, FontFamily = new FontFamily("Showcard Gothic") });
            pauseDoc.Blocks.Add(pause);
            pauseDoc.TextAlignment = TextAlignment.Center;
            this.pauseTextBox.Document = pauseDoc;
            startBtn_Image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "../../../Source/start.png"));
            BackgroundImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "../../../Source/background.jpeg"));
            game.gameStatus = GameStatus.NotStartYet;
            SumupGroup.Visibility = Visibility.Hidden;
        }

        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame == null)
                    return;

                using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                {

                    this.bitmap.Lock();
                    if (this.frameDescription.Width == this.bitmap.PixelWidth && this.frameDescription.Height == this.bitmap.PixelHeight)
                    {
                        colorFrame.CopyConvertedFrameDataToIntPtr(
                            this.bitmap.BackBuffer,
                            (uint)(this.frameDescription.Width * this.frameDescription.Height * 4),
                            ColorImageFormat.Bgra);

                       

                        this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight));
                    }

                    this.bitmap.Unlock();


        

                    if (game.gameStatus == GameStatus.Pause)
                    {
                        pauseTextBox.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null)
                {
                    return;
                }

                if (this.bodies == null)
                {
                    this.bodies = new Body[frame.BodyCount];
                }

                frame.GetAndRefreshBodyData(bodies);

                Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();

                if (body == null)
                {
                    if (DEBUGMODE) { txtLeft.Text = "No body"; }
                    return;
                }


                List<BaseObject> objects = game.getObjects();
                if (DEBUGMODE)
                {
                    txtLeft.Text = ConvertSpace(body.Joints[JointType.HandRight].Position).X + "\n" + ConvertSpace(body.Joints[JointType.HandRight].Position).Y + "\n";
                    txtRight.Text = ConvertSpace(body.Joints[JointType.HandLeft].Position).X + "\n" + ConvertSpace(body.Joints[JointType.HandLeft].Position).Y + "\n";
                }
               

                SpineShoudler = body.Joints[JointType.SpineShoulder].Position.Z;
                if (DEBUGMODE) { SpineShoulderDepthTxt.Text = SpineShoudler.ToString(); }

                if (game.gameStatus == GameStatus.Pause)
                {
                    if (ConvertSpace(body.Joints[JointType.HandRight].Position).Y < ConvertSpace(body.Joints[JointType.ElbowRight].Position).Y && SpineShoudler > 1.5)
                    {
                        Debug.WriteLine(body.Joints[JointType.HandRight].Position.Y + " " + body.Joints[JointType.ElbowRight].Position.Y);
                        game.gameStatus = GameStatus.StartFromPause;
                        game.StartGame();
                        game.gameStatus = GameStatus.Gaming;
                        pauseTextBox.Visibility = Visibility.Hidden;
                        pauseText.Visibility = Visibility.Hidden;
                    }
                }
                else if (game.gameStatus == GameStatus.Gaming)
                {
                    if (SpineShoudler <= 1.5 && game.gameStatus == GameStatus.Gaming)
                    {
                        game.gameStatus = GameStatus.Pause;
                        game.StartGame();
                    }

                  

                    for (int i = 0; i < objects.Count && !objects[i].IsTouched; i++)
                    {
                        ColorSpacePoint pos_R;
                        if (body.Joints[JointType.HandRight].TrackingState != TrackingState.NotTracked)
                        {
                            pos_R = ConvertSpace(body.Joints[JointType.HandRight].Position);
                        }
                        else
                        {
                            pos_R = ConvertSpace(body.Joints[JointType.WristRight].Position);

                        }
                        if (SQR_Distance(pos_R, objects[i].Position) <= 100)
                        {
                            objects[i].IsTouched = true;
                            Debug.WriteLine(objects[i].Type + " is touched by righthand");
                            game.ObjectTouched(objects[i], Joint.Righthand);

                            if (DEBUGMODE) { Touch.Text = objects[i].Type + "is touched by righthand"; }

                            continue;
                        }
                        ColorSpacePoint pos_L;
                        if (body.Joints[JointType.HandLeft].TrackingState != TrackingState.NotTracked)
                        {
                            pos_L = ConvertSpace(body.Joints[JointType.HandLeft].Position);
                        }
                        else
                        {
                            pos_L = ConvertSpace(body.Joints[JointType.WristLeft].Position);

                        }

                        if (SQR_Distance(pos_L, objects[i].Position) <= 100)
                        {
                            objects[i].IsTouched = true;
                            Debug.WriteLine(objects[i].Type + " is touched by lefthand");
                            game.ObjectTouched(objects[i], Joint.Lefthand);

                            if (DEBUGMODE) { Touch.Text = objects[i].Type + "is touched by lefthand"; }

                            continue;
                        }

                    }
                }
            }
        }



        private void Kinect_Class2_Loaded(object sender, RoutedEventArgs e)
        {
            this.ImageSource.Source = this.bitmap;
            pauseTextBox.Visibility = Visibility.Hidden;
            countDownTxt.Visibility = Visibility.Hidden;
            countDownTxt.FontSize = 128;
        }

        private void Kinect_Class2_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
        }


        private double SQR_Distance(ColorSpacePoint a, Point b)
        {
            return Math.Sqrt((((a.X) - (b.X)) * ((a.X) - (b.X))) + (((a.Y) - (b.Y)) * ((a.Y) - (b.Y))));
        }

        private ColorSpacePoint ConvertSpace(CameraSpacePoint p)
        {
            return sensor.CoordinateMapper.MapCameraPointToColorSpace(p);
        }



        private void startBtn_Click(object sender, RoutedEventArgs e)
        {

            game.gameStatus = GameStatus.NotStartYet;
                game.StartGame();
                game.gameStatus = GameStatus.Gaming;

            
           
        }

        private void startBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            startBtn_Image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "../../../Source/ystart.png"));
        }

        private void startBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            startBtn_Image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "../../../Source/start.png"));
        }

       
    }
}

