//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.ControlsBasics;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectSensor;
        private SkeletonPoint currentSkeletonPoint;
        private List<SkeletonPoint> listOfSkeletonPoints = new List<SkeletonPoint>();
        private List<Point> calibrationPoints = new List<Point>();
        private PartialCalibrationClass partialCalibrationClass;
        private bool calibrated = false;
        List<Brush> brushes = new List<Brush>();


        public MainWindow()
        {
            InitializeComponent();
            circle(10, 10, 100, 100, myCanvas, Brushes.Red);
            brushes.Add(Brushes.Blue);
            brushes.Add(Brushes.Red);
            brushes.Add(Brushes.Green);
            brushes.Add(Brushes.Orange);
            brushes.Add(Brushes.Teal);
            brushes.Add(Brushes.Black);
            findCentreOfGravityForKinect();
        }

        private static void circle(int x, int y, int width, int height, Canvas cv, Brush color)
        {

            Ellipse circle = new Ellipse()
            {
                Width = width,
                Height = height,
                Stroke = color,
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

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    points = new Point[skeletons.Length];
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
                        if (partialCalibrationClass != null)
                        {
                            points[counter-1] = partialCalibrationClass.kinectToProjectionPoint(currentSkeletonPoint);

                            circle((int)points[counter-1].X, (int)points[counter-1].Y, 100, 100, myCanvas, brushes[counter-1]);
                        }
                    }
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
                        calibrationPoints.Add(new Point(10 + 50, 10 + 50));
                        break;
                    case 1:
                        circle(750 - 100 - 10, 10, 100, 100, myCanvas, Brushes.Red);
                        calibrationPoints.Add(new Point(750 - 100 - 10 + 50, 10 + 50));
                        break;
                    case 2:
                        circle(750 - 100 - 10, 750 - 100 - 10, 100, 100, myCanvas, Brushes.Red);
                        calibrationPoints.Add(new Point(750 - 100 - 10 + 50, 750 - 100 - 10 + 50));
                        break;
                    case 3:
                        circle(10, 750 - 100 - 10, 100, 100, myCanvas, Brushes.Red);
                        calibrationPoints.Add(new Point(10 + 50, 750 - 100 - 10 + 50));
                        break;
                    default:
                        break;
                }


                MessageBox.Show($"Current point: {amountOfPoints}/4. " +
                    $"\n x: {currentSkeletonPoint.X}" +
                    $"\n y: {currentSkeletonPoint.Y}" +
                    $"\n z: {currentSkeletonPoint.Z}");
            }
            else
            {
                partialCalibrationClass = new PartialCalibrationClass(kinectSensor, calibrationPoints, listOfSkeletonPoints);
                partialCalibrationClass.calibrate();
                calibrated = true;
            }
        }
    }
}