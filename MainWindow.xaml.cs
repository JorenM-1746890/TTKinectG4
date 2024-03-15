//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.ControlsBasics;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using System.Reflection;
    using System.ComponentModel;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private KinectSensor kinectSensor;
        private SkeletonPoint currentSkeletonPoint, lFeetSkeleton, rFeetSkeleton;
        private List<SkeletonPoint> listOfSkeletonPoints = new List<SkeletonPoint>();
        private List<Point> calibrationPoints = new List<Point>();
        private PartialCalibrationClass partialCalibrationClass;
        private bool calibrated = false;
        List<Brush> brushes = new List<Brush>();
        static HandsUpGesture handsUpGesture = new HandsUpGesture();
        static ClapGesture clapGesture = new ClapGesture();
        private DateTime timeSinceGesture = DateTime.Now;
        private DispatcherTimer timer;
        private Random random;
        private Queue<int> currentMoves = new Queue<int>();
        //string soundFilePath = "C:\\Users\\devin\\OneDrive\\Documents\\UHasselt_Master1\\Tools and Technologies for Interactive Systems Development\\Kinect Project\\TTKinectG4\\Sounds\\sea_shanty_2_remix.mp3";
        string soundFilePath = "D:\\School2\\TTKinectG4\\Sounds\\sea_shanty_2_remix.mp3";
        MediaPlayer mediaPlayer = new MediaPlayer();
        private int p1Score = 0;
        private int p2Score = 0;
        Point[] lFeet = new Point[0];
        Point[] rFeet = new Point[0];
        bool timerRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            circle(10, 10, 100, 100, myCanvas, Brushes.White);
            brushes.Add(Brushes.Blue);
            brushes.Add(Brushes.Red);
            brushes.Add(Brushes.Green);
            brushes.Add(Brushes.Orange);
            brushes.Add(Brushes.Teal);
            brushes.Add(Brushes.Black);
            findCentreOfGravityForKinect();
            
            random = new Random();
            currentMoves.Enqueue(4);
            currentMoves.Enqueue(4);
            currentMoves.Enqueue(4);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(((170.0 / 4.0) / 60.0) * 1000.0);
            timer.Tick += Timer_Tick;

            mediaPlayer.Open(new Uri(soundFilePath));
            mediaPlayer.Play();
            mediaPlayer.Pause();
        }

        public int P1Score
        {
            get { return p1Score; }
            set
            {
                p1Score = value;
                OnPropertyChanged(nameof(P1Score));
            }
        }

        public int P2Score
        {
            get { return p2Score; }
            set
            {
                p2Score = value;
                OnPropertyChanged(nameof(P2Score));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            testCanvas.Children.Clear();

            // check for score
            Button[] p1Buttons = new Button[] { Left1, Up1, Down1, Right1, new Button() };
            Button[] p2Buttons = new Button[] { Left2, Up2, Down2, Right2, new Button() };

            for (int i = 0; i < lFeet.Length; i++)
            {
                if (CheckButtons((int)lFeet[i].X, (int)lFeet[i].Y).Equals(p1Buttons[currentMoves.Peek()]) || CheckButtons((int)rFeet[i].X, (int)rFeet[i].Y).Equals(p1Buttons[currentMoves.Peek()]))
                {
                    P1Score += 100;
                }
                if (CheckButtons((int)lFeet[i].X, (int)lFeet[i].Y).Equals(p2Buttons[currentMoves.Peek()]) || CheckButtons((int)rFeet[i].X, (int)rFeet[i].Y).Equals(p2Buttons[currentMoves.Peek()]))
                {
                    P2Score += 100;
                }
            }
            // tot hier :)

            int randomNumber = random.Next(0, 4);
            if (currentMoves.Count >= 3)
            {
                currentMoves.Dequeue();
            }
            currentMoves.Enqueue(randomNumber);

            drawBoard(currentMoves.ToArray());

                    
        }

        private void drawBoard(int[] moves)
        {
            string[] images = new string[] { "Images\\left.png", "Images\\up.png", "Images\\down.png" , "Images\\right.png" };
            string[] filledImages = new string[] { "Images\\left_filled.png", "Images\\up_filled.png", "Images\\down_filled.png", "Images\\right_filled.png" };
            for (int i = 0; i < moves.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    string boardPath = "";
                    if (moves[i] == j)
                    {
                        boardPath = filledImages[j];
                    }
                    else
                    {
                        boardPath = images[j];
                    }
                    Image image = new Image()
                    {
                        Width = 140,
                        Height = 140,
                        Source = new BitmapImage(new Uri(boardPath, UriKind.Relative))
                    };
                    // TODO 120 is hard coded
                    image.SetValue(Canvas.LeftProperty, j*140.0 + 120);
                    image.SetValue(Canvas.TopProperty, (moves.Length - i - 1) *140.0);
                    testCanvas.Children.Add(image);
                    Image image2 = new Image()
                    {
                        Width = 140,
                        Height = 140,
                        Source = new BitmapImage(new Uri(boardPath, UriKind.Relative))
                    };
                    image2.SetValue(Canvas.LeftProperty, j * 140.0 + 120 + 800);
                    image2.SetValue(Canvas.TopProperty, (moves.Length - i - 1) * 140.0);
                    testCanvas.Children.Add(image2);
                }
            }
            
        }

        private static void circle(int x, int y, int width, int height, Canvas cv, Brush color)
        {

            Ellipse circle = new Ellipse()
            {
                Width = width,
                Height = height,
                Stroke = Brushes.Black,
                Fill = color,
            };

            cv.Children.Add(circle);

            circle.SetValue(Canvas.LeftProperty, (double)x);
            circle.SetValue(Canvas.TopProperty, (double)y);
        }

        private void findCentreOfGravityForKinect()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    kinectSensor = potentialSensor;
                    break;
                }
            }

            if (kinectSensor != null)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.kinectSensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.kinectSensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                handsUpGesture.GestureRecognized += HandsUpGestureRecognized;
                clapGesture.ClapRecognized += ClapGestureRecognized;

                // Start the sensor!
                try
                {
                    this.kinectSensor.Start();
                }
                catch (IOException)
                {
                    this.kinectSensor = null;
                }
            }
        }

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            Point[] points = new Point[0];
            DateTime currentTime = DateTime.Now;
            TimeSpan currTS = new TimeSpan(currentTime.Ticks);
            TimeSpan gestTS = new TimeSpan(timeSinceGesture.Ticks);
            double timeDiff = currTS.Subtract(gestTS).TotalMilliseconds;


            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    points = new Point[skeletons.Length];
                    lFeet = new Point[skeletons.Length];
                    rFeet = new Point[skeletons.Length];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            if (calibrated)
            {
                int counter = 0;
                myCanvas.Children.Clear();

                foreach (Skeleton skeleton in skeletons)
                {
                    counter++;
                    if (skeleton.Position.X != 0 && skeleton.Position.Y != 0)
                    {
                        currentSkeletonPoint = skeleton.Position;
                        lFeetSkeleton = skeleton.Joints[JointType.AnkleLeft].Position;
                        rFeetSkeleton = skeleton.Joints[JointType.AnkleRight].Position;
                        if (partialCalibrationClass != null)
                        {
                            points[counter-1] = partialCalibrationClass.kinectToProjectionPoint(currentSkeletonPoint);
                            lFeet[counter - 1] = partialCalibrationClass.kinectToProjectionPoint(lFeetSkeleton);
                            rFeet[counter - 1] = partialCalibrationClass.kinectToProjectionPoint(rFeetSkeleton);

                            circle((int)points[counter-1].X, (int)points[counter-1].Y, 100, 100, myCanvas, brushes[counter-1]);
                            circle((int)lFeet[counter - 1].X, (int)lFeet[counter - 1].Y, 20, 20, myCanvas, brushes[counter - 1]);
                            circle((int)rFeet[counter - 1].X, (int)rFeet[counter - 1].Y, 20, 20, myCanvas, brushes[counter - 1]);
                        }
                    }

                    // check handsup gesture
                    if (timeDiff > 1000.0)
                    {
                        
                        if (clapGesture.Update(skeleton))
                        {
                            Console.WriteLine(timeDiff);
                            timeSinceGesture = DateTime.Now;
                            if (!timerRunning)
                            {
                                timer.Start();
                                mediaPlayer.Play();
                                timerRunning = true;
                            }
                            else
                            {
                                timer.Stop();
                                mediaPlayer.Pause();
                                timerRunning = false;
                            }
                        }
                    }
                    
                    Button buttonLFoot = CheckButtons((int)lFeet[counter - 1].X, (int)lFeet[counter - 1].Y);
                    Button buttonRFoot = CheckButtons((int)rFeet[counter - 1].X, (int)rFeet[counter - 1].Y);
                }
            }
            else
            {
                foreach (Skeleton skeleton in skeletons)
                {
                    if (skeleton.Position.X != 0 && skeleton.Position.Y != 0)
                    {
                        currentSkeletonPoint = skeleton.Position;
                        circle((int)(750 / 2 + skeleton.Position.X * 200), (int)(750 / 2 + skeleton.Position.Y * 200), 100, 100, myCanvas, Brushes.Green);

                        // check handsup gesture
                        if (timeDiff > 1000.0)
                        {
                            if (handsUpGesture.Update(skeleton))
                            {
                                Button_Click(this, new RoutedEventArgs());
                                timeSinceGesture = DateTime.Now;
                            }
                        }
                    }
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (listOfSkeletonPoints.Count < 4)
            {
                myCanvas.Children.Clear();

                int amountOfPoints = listOfSkeletonPoints.Count;
                listOfSkeletonPoints.Add(currentSkeletonPoint);

                switch (amountOfPoints)
                {
                    case 0:
                        circle((int)myCanvas.Width - 100 - 10, 10, 100, 100, myCanvas, Brushes.White);
                        calibrationPoints.Add(new Point(10 + 50, 10 + 50));
                        break;
                    case 1:
                        circle((int)myCanvas.Width - 100 - 10, (int)myCanvas.Height - 100 - 10, 100, 100, myCanvas, Brushes.White);
                        calibrationPoints.Add(new Point((int)myCanvas.Width - 100 - 10 + 50, 10 + 50));
                        break;
                    case 2:
                        circle(10, (int)myCanvas.Height - 100 - 10, 100, 100, myCanvas, Brushes.White);
                        calibrationPoints.Add(new Point((int)myCanvas.Width - 100 - 10 + 50, (int)myCanvas.Height - 100 - 10 + 50));
                        break;
                    case 3:
                        calibrationPoints.Add(new Point(10 + 50, (int)myCanvas.Height - 100 - 10 + 50));
                        break;
                    default:
                        break;
                }
            }
            else
            {
                partialCalibrationClass = new PartialCalibrationClass(kinectSensor, calibrationPoints, listOfSkeletonPoints);
                partialCalibrationClass.calibrate();
                calibrated = true;
                gameArea.Visibility = Visibility.Visible;
            }
        }

        static void HandsUpGestureRecognized(object sender, EventArgs e)
        {
            Console.WriteLine("HANDS UP billyReady");
        }

        static void ClapGestureRecognized(object sender, EventArgs e)
        {
            Console.WriteLine("HYPERCLAP aniki");
        }

        public Button CheckButtons(int x, int y)
        {
            Point up1 = Up1.TransformToAncestor(this).Transform(new Point(0, 0));
            Point up2 = Up2.TransformToAncestor(this).Transform(new Point(0, 0));
            Point left1 = Left1.TransformToAncestor(this).Transform(new Point(0, 0));
            Point left2 = Left2.TransformToAncestor(this).Transform(new Point(0, 0));
            Point right1 = Right1.TransformToAncestor(this).Transform(new Point(0, 0));
            Point right2 = Right2.TransformToAncestor(this).Transform(new Point(0, 0));
            Point down1 = Down1.TransformToAncestor(this).Transform(new Point(0, 0));
            Point down2 = Down2.TransformToAncestor(this).Transform(new Point(0, 0));

            Button badButton = new Button();
            badButton.Name = "fake";

            if (x >= up1.X - 50 && x <= up1.X + 50 + Up1.ActualWidth && y >= up1.Y - 50 && y <= up1.Y + 50 + Up1.ActualHeight)
            {
                return Up1;
            }
            else if (x >= up2.X - 50 && x <= up2.X + 50 + Up2.ActualWidth && y >= up2.Y - 50 && y <= up2.Y + 50 + Up2.ActualHeight)
            {
                return Up2;
            }
            else if (x >= left1.X - 50 && x <= left1.X + 50 + Left1.ActualWidth && y >= left1.Y - 50 && y <= left1.Y + 50 + Left1.ActualHeight)
            {
                return Left1;
            }
            else if (x >= left2.X - 50 && x <= left2.X + 50 + Left2.ActualWidth && y >= left2.Y - 50 && y <= left2.Y + 50 + Left2.ActualHeight)
            {
                return Left2;
            }
            else if (x >= right1.X - 50 && x <= right1.X + 50 + Right1.ActualWidth && y >= right1.Y - 50 && y <= right1.Y + 50 + Right1.ActualHeight)
            {
                return Right1;
            }
            else if (x >= right2.X - 50 && x <= right2.X + 50 + Right2.ActualWidth && y >= right2.Y - 50 && y <= right2.Y + 50 + Right2.ActualHeight)
            {
                return Right2;
            }
            else if (x >= down1.X - 50 && x <= down1.X + 50 + Down1.ActualWidth && y >= down1.Y - 50 && y <= down1.Y + 50 + Down1.ActualHeight)
            {
                return Down1;
            }
            else if (x >= down2.X - 50 && x <= down2.X + 50 + Down2.ActualWidth && y >= down2.Y - 50 && y <= down2.Y + 50 + Down2.ActualHeight)
            {
                return Down2;
            }
            else
            {
                return badButton;
            }
        }
    }
}