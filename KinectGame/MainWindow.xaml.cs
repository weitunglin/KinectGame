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

        public MainWindow()
        {
            InitializeComponent();

            this.game = new Game();

            this.sensor = KinectSensor.GetDefault();
            this.frameDescription = this.sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);

            this.colorFrameReader = this.sensor.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            this.bodyFrameReader = this.sensor.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

            this.bitmap = new WriteableBitmap(this.frameDescription.Width, this.frameDescription.Height, 96, 96, PixelFormats.Bgr32, null);
            this.sensor.Open();
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
                }
            }
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(bodies);

                    Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();

                    List<Point>pos = null;

                    //torso
                    Point head_pos = new Point(body.Joints[JointType.Head].Position.X, body.Joints[JointType.Head].Position.Y);
                    Point spineshoulder_pos = new Point(body.Joints[JointType.SpineShoulder].Position.X, body.Joints[JointType.SpineShoulder].Position.Y);
                    Point leftshoulder_pos = new Point(body.Joints[JointType.ShoulderLeft].Position.X, body.Joints[JointType.ShoulderLeft].Position.Y);
                    Point rightshoulder_pos = new Point(body.Joints[JointType.ShoulderRight].Position.X, body.Joints[JointType.ShoulderRight].Position.Y);
                    Point lefthip_pos = new Point(body.Joints[JointType.HipLeft].Position.X, body.Joints[JointType.HipLeft].Position.Y);
                    Point righthip_pos = new Point(body.Joints[JointType.HipRight].Position.X, body.Joints[JointType.HipRight].Position.Y);

                    pos.Add(head_pos);
                    pos.Add(spineshoulder_pos);
                    pos.Add(leftshoulder_pos);
                    pos.Add(rightshoulder_pos);
                    pos.Add(lefthip_pos);
                    pos.Add(righthip_pos);

                    //right hand
                    Point righthand_pos = new Point(body.Joints[JointType.HandRight].Position.X, body.Joints[JointType.HandRight].Position.Y);
                    Point rightelbow_pos = new Point(body.Joints[JointType.ElbowRight].Position.X, body.Joints[JointType.ElbowRight].Position.Y);
                    Point rightwrist_pos=new Point(body.Joints[JointType.WristRight].Position.X, body.Joints[JointType.WristRight].Position.Y);

                    pos.Add(righthand_pos);
                    pos.Add(rightelbow_pos);
                    pos.Add(rightwrist_pos);

                    //left hand
                    Point leftthand_pos = new Point(body.Joints[JointType.HandLeft].Position.X, body.Joints[JointType.HandLeft].Position.Y);
                    Point leftelbow_pos = new Point(body.Joints[JointType.ElbowLeft].Position.X, body.Joints[JointType.ElbowLeft].Position.Y);
                    Point leftwrist_pos = new Point(body.Joints[JointType.WristLeft].Position.X, body.Joints[JointType.WristLeft].Position.Y);

                    pos.Add(leftthand_pos);
                    pos.Add(leftelbow_pos);
                    pos.Add(leftwrist_pos);


                    //right leg
                    Point rightknee_pos = new Point(body.Joints[JointType.KneeRight].Position.X, body.Joints[JointType.KneeRight].Position.Y);
                    Point rightankel_pos = new Point(body.Joints[JointType.AnkleRight].Position.X, body.Joints[JointType.AnkleRight].Position.Y);
                    Point rightfoot_pos = new Point(body.Joints[JointType.FootRight].Position.X, body.Joints[JointType.FootRight].Position.Y);

                    pos.Add(rightknee_pos);
                    pos.Add(rightankel_pos);
                    pos.Add(rightfoot_pos);

                    //left leg
                    Point leftknee_pos = new Point(body.Joints[JointType.KneeLeft].Position.X, body.Joints[JointType.KneeLeft].Position.Y);
                    Point leftankel_pos = new Point(body.Joints[JointType.AnkleLeft].Position.X, body.Joints[JointType.AnkleLeft].Position.Y);
                    Point leftfoot_pos = new Point(body.Joints[JointType.FootLeft].Position.X, body.Joints[JointType.FootLeft].Position.Y);

                    pos.Add(leftknee_pos);
                    pos.Add(leftankel_pos);
                    pos.Add(leftfoot_pos);

                    txtLeft.Text = body.Joints[JointType.HandLeft].Position.Y.ToString();
                    txtRight.Text = body.Joints[JointType.HandRight].Position.Y.ToString();

                    List<BaseObject> objects = game.getObjects();

                    for (int i = 0; i < objects.Count && !objects[i].IsTouched; i++)
                    {
                        for (int j = 0; j < pos.Count; j++)
                        {
                            if (SQR_Distance(pos[j], objects[i].Position) <= 300*300)
                            {
                                objects[i].IsTouched = true;
                            }
                        }
                    }
                }
            }
        }

        private void Kinect_Class2_Loaded(object sender, RoutedEventArgs e)
        {
            this.ImageSource.Source = this.bitmap;
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

        private double SQR_Distance(Point a, Point b)
        {
            return ((a.X * a.X) - (b.X * b.X) + (a.Y) * (a.Y) - (b.Y) * (b.Y));
        }
    }
}
