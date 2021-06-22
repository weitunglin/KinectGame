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

        private float SpineShoudler = new float();

        private GameStatus gameStatus = GameStatus.NotStartYet;
        Ellipse Circle = new Ellipse();
        BrushConverter bc = new BrushConverter();
      

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

            this.game = new Game(this.ImageCanvas, this.ImageSource.Width, this.ImageSource.Height, 150);
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


                    //if(timer == 0||health == 0)
                    //{
                    //    gamestatus = GameStatus.GameOver;
                    //}

                    if (gameStatus == GameStatus.Pause)
                    {
                        pausebtn.Visibility = Visibility.Visible;
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
                    txtLeft.Text = "No body";
                    return;

                }




                List<BaseObject> objects = game.getObjects();



                const int lefthand = 9;
                const int righthand = 6;


                add_to_list(body);


                txtLeft.Text = pos[lefthand].X.ToString() + "\n" + pos[lefthand].Y.ToString() + "\n";
                txtRight.Text = pos[righthand].X.ToString() + "\n" + pos[righthand].Y.ToString() + "\n" /*+ pos[6].X.ToString() + "\n" + pos[6].Y.ToString()*/;
                SpineShoulderDepthTxt.Text = SpineShoudler.ToString();

                //draw circle test
                if (!float.IsInfinity(pos[lefthand].Y) && (!float.IsInfinity(pos[lefthand].X)))
                {

                    Circle.Visibility = Visibility.Visible;
                    Canvas.SetTop(Circle, pos[lefthand].Y - 200);
                    Canvas.SetLeft(Circle, pos[lefthand].X - 200);
                }


                if (SpineShoudler <= 1.5)
                {
                    //    gameStatus = GameStatus.Pause;
                }



                for (int i = 0; i < objects.Count && !objects[i].IsTouched; i++)
                {

                    if (SQR_Distance(pos[righthand], objects[i].Position) <= 100)
                    {
                        objects[i].IsTouched = true;
                        Debug.WriteLine(objects[i].Id + "is touched by righthand");
                        Debug.WriteLine(objects[i].Position.ToString() + " " + pos[righthand].X.ToString() + pos[righthand].Y.ToString());

                        continue;
                    }
                    else if (SQR_Distance(pos[lefthand], objects[i].Position) <= 100)
                    {
                        objects[i].IsTouched = true;
                        Debug.WriteLine(objects[i].Id + "is touched by lefthand");
                        Debug.WriteLine(objects[i].Position.ToString() + " " + pos[lefthand].X.ToString() + pos[lefthand].Y.ToString());

                        continue;
                    }
                    for (int j = 0; j < pos.Count; j++)
                    {
                        if (SQR_Distance(pos[j], objects[i].Position) <= 100)
                        {
                            objects[i].IsTouched = true;
                            Debug.WriteLine(objects[i].Id + "is touched by " + j);
                            Debug.WriteLine(objects[i].Position.ToString() + " " + pos[j].X.ToString() + pos[j].Y.ToString());
                            break;
                        }
                    }
                }
            }
        }



        private void Kinect_Class2_Loaded(object sender, RoutedEventArgs e)
        {
            this.ImageSource.Source = this.bitmap;
            pausebtn.Visibility = Visibility.Hidden;
            Brush brush = (Brush)bc.ConvertFrom("Red");
            Circle.Fill = new SolidColorBrush(Colors.Red);
            Circle.Width = 600;
            Circle.Height = 600;
            Canvas.SetLeft(Circle, 100);
            Canvas.SetTop(Circle, 100);
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
            return Math.Sqrt((a.X * a.X) - (b.X * b.X) + (a.Y) * (a.Y) - (b.Y) * (b.Y));
        }

        private void add_to_list(Body body)
        {
            pos.Clear();

            //torso
            CameraSpacePoint head_ = body.Joints[JointType.Head].Position;
            CameraSpacePoint spineshoulder_ = body.Joints[JointType.SpineShoulder].Position;
            CameraSpacePoint leftshoulder_ = body.Joints[JointType.ShoulderLeft].Position;
            CameraSpacePoint rightshoulder_ = body.Joints[JointType.ShoulderRight].Position;
            CameraSpacePoint lefthip_ = body.Joints[JointType.HipLeft].Position;
            CameraSpacePoint righthip_ = body.Joints[JointType.HipRight].Position;

            ColorSpacePoint head_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(head_);
            ColorSpacePoint spineshoulder_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(spineshoulder_);
            ColorSpacePoint leftshoulder_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(leftshoulder_);
            ColorSpacePoint rightshoulder_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(rightshoulder_);
            ColorSpacePoint lefthip_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(lefthip_);
            ColorSpacePoint righthip_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(righthip_);

            SpineShoudler = body.Joints[JointType.SpineShoulder].Position.Z;

            pos.Add(head_pos);
            pos.Add(spineshoulder_pos);
            pos.Add(leftshoulder_pos);
            pos.Add(rightshoulder_pos);
            pos.Add(lefthip_pos);
            pos.Add(righthip_pos);

            //right hand
            CameraSpacePoint righthand_ = body.Joints[JointType.HandRight].Position;
            CameraSpacePoint rightelbow_ = body.Joints[JointType.ElbowRight].Position;
            CameraSpacePoint rightwrist_ = body.Joints[JointType.WristRight].Position;

            ColorSpacePoint righthand_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(righthand_);
            ColorSpacePoint rightelbow_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(rightelbow_);
            ColorSpacePoint rightwrist_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(rightwrist_);

            pos.Add(righthand_pos);
            pos.Add(rightelbow_pos);
            pos.Add(rightwrist_pos);

            //left hand
            CameraSpacePoint leftthand_ = body.Joints[JointType.HandLeft].Position;
            CameraSpacePoint leftelbow_ = body.Joints[JointType.ElbowLeft].Position;
            CameraSpacePoint leftwrist_ = body.Joints[JointType.WristLeft].Position;

            ColorSpacePoint leftthand_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(leftthand_);
            ColorSpacePoint leftelbow_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(leftelbow_);
            ColorSpacePoint leftwrist_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(leftwrist_);

            pos.Add(leftthand_pos);
            pos.Add(leftelbow_pos);
            pos.Add(leftwrist_pos);


            //right
            CameraSpacePoint rightknee_ = body.Joints[JointType.KneeRight].Position;
            CameraSpacePoint rightankel_ = body.Joints[JointType.AnkleRight].Position;
            CameraSpacePoint rightfoot_ = body.Joints[JointType.FootRight].Position;

            ColorSpacePoint rightknee_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(rightknee_);
            ColorSpacePoint rightankel_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(rightankel_);
            ColorSpacePoint rightfoot_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(rightfoot_);

            pos.Add(rightknee_pos);
            pos.Add(rightankel_pos);
            pos.Add(rightfoot_pos);

            //left leg
            CameraSpacePoint leftknee_ = body.Joints[JointType.KneeLeft].Position;
            CameraSpacePoint leftankel_ = body.Joints[JointType.AnkleLeft].Position;
            CameraSpacePoint leftfoot_ = body.Joints[JointType.FootLeft].Position;

            ColorSpacePoint leftknee_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(leftknee_);
            ColorSpacePoint leftankel_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(leftankel_);
            ColorSpacePoint leftfoot_pos = sensor.CoordinateMapper.MapCameraPointToColorSpace(leftfoot_);

            pos.Add(leftknee_pos);
            pos.Add(leftankel_pos);
            pos.Add(leftfoot_pos);
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            if (gameStatus == GameStatus.NotStartYet)
            {
                game.StartGame(gameStatus);
                //  startBtn.Content = "Pause";
                startBtn.Visibility = Visibility.Hidden;
                gameStatus = GameStatus.Gaming;
            }
            else if (gameStatus == GameStatus.Gaming)
            {
                game.StartGame(gameStatus);
                startBtn.Content = "Start";
                gameStatus = GameStatus.NotStartYet;
            }


        }
    }
}
