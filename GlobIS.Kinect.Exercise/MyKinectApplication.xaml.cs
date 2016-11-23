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
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace GlobIS.Kinect.HCIExercise
{
    /// <summary>
    /// Interaction logic for MyKinectApplication.xaml
    /// </summary>
    public partial class MyKinectApplication : Window
    {
        public MyKinectApplication()
        {
            Trace.WriteLine("Application starts...");
            InitializeComponent();

            Loaded += MyKinectApplication_Loaded;
            Closed += MyKinectApplication_Closed;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Exit when Escape is pressed
            if (e.Key == Key.Escape)
            {
                this.Close();
            }

        }

        void MyKinectApplication_Closed(object sender, EventArgs e)
        {
            // Shutdown the kinect sensor
            Trace.WriteLine("Kinect sensor is being shut down.");
            sensor.Close();
        }

        // Fields to store Kinect-specific data
        KinectSensor sensor;
        BodyFrameReader bodyReader;
        Body[] bodies;
        Body trackedBody;

        // Setup Kinect sensor
        void MyKinectApplication_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the default Kinect Sensor (there can only be one anyway)
            sensor = KinectSensor.GetDefault();

            // Request a new reader for the body frames (skeletal tracking)
            bodyReader = sensor.BodyFrameSource.OpenReader();

            // Allocate buffer for bodies (skeletons)
            // The Kinect runtime will always create 6 body objects,
            // even if less persons are actually being tracked
            bodies = new Body[6];

            // Start the Kinect sensor
            sensor.Open();

            // Register event handler to process body frames
            bodyReader.FrameArrived += bodyReader_FrameArrived;
        }

        void bodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            // Retrieve the current body frame and dispose it after the block
            using (BodyFrame bFrame = e.FrameReference.AcquireFrame())
            {
                // Check whether the frame has not expired yet (i.e. it is available)
                if (bFrame != null)
                {
                    // Copy the body data into our buffer
                    bFrame.GetAndRefreshBodyData(bodies);

                    // TODO If you want to handle more than one body, you have to
                    // adapt the following lines

                    // Get the first tracked body
                    trackedBody = (from s in bodies
                                   where s.IsTracked
                                   select s).FirstOrDefault();

                    if (trackedBody != null)
                    {
                        RecognizeGestures(trackedBody);
                        JointOrientation headOrientation = trackedBody.JointOrientations[JointType.Head];
                        CameraSpacePoint headPosition = trackedBody.Joints[JointType.Head].Position;
                        // ... do stuff
                    }

                    // Draw the recognized bodies in the transparent overlay window
                    DrawStickMen(bodies);
                }
            }
        }


        /// <summary>
        /// States for a very basic state machine
        /// </summary>
        enum MyStates
        {
            NEUTRAL,
            RIGHT_ARM_OUT,
            LEFT_HAND_IN,
            RIGHT_HAND_IN,
            CLAP,
            FACE_PALM,
            BLACK,
            NOGESTURE
        }

        // Starting state
        MyStates currentState = MyStates.NEUTRAL;
        DateTime lastClap = DateTime.MinValue;
        DateTime lastFacePalm = DateTime.MinValue;


        /// <summary>
        /// TODO This is where you are supposed to implement your pose/gesture recognition
        /// </summary>
        /// <param name="trackedBody">Tracked body data</param>
        private void RecognizeGestures(Body trackedBody)
        {

            // TODO Add your pose/gesture recognition code here!

            var leftState = trackedBody.HandLeftState;
            var rightState = trackedBody.HandRightState;

            if (currentState != MyStates.NOGESTURE) {

                switch (currentState)
                {
                    case MyStates.NEUTRAL:
                        // Transition condition for right arm stretched out

                        if (LeftHandIn && LeftHandAboveElbow && leftState == HandState.Closed)
                        {
                            Trace.WriteLine("StartingNext");
                            currentState = MyStates.LEFT_HAND_IN;
                        }
                        else if (RightHandIn && RightHandAboveElbow && rightState == HandState.Closed)
                        {
                            Trace.WriteLine("StartingPrev");
                            currentState = MyStates.RIGHT_HAND_IN;
                        }
                        else if (Clap)
                        {
                            Trace.WriteLine("Clapping");
                            currentState = MyStates.CLAP;
                        }
                        else if (FacePalm)
                        {
                            Trace.WriteLine("Face Palm start");
                            lastFacePalm = DateTime.Now;
                            currentState = MyStates.FACE_PALM;
                        }
                        //else if (ToggleGesture)
                        //{
                        //    Trace.WriteLine("Disabling");
                        //    currentState = MyStates.NOGESTURE;
                        //}


                        break;

                    case MyStates.LEFT_HAND_IN:
                        if (!LeftHandIn && LeftHandAboveElbow && leftState == HandState.Open)
                        {
                            nextSlide();
                            currentState = MyStates.NEUTRAL;
                            Trace.WriteLine("Next");
                        }
                        else if ((LeftHandIn && leftState == HandState.Open) || !LeftHandAboveElbow)
                        {
                            Trace.WriteLine("Aborting Next");
                            currentState = MyStates.NEUTRAL;
                        }
                        else if (Clap)
                        {
                            Trace.WriteLine("Clapping");
                            currentState = MyStates.CLAP;
                        }
                        else if (FacePalm)
                        {
                            Trace.WriteLine("Face Palm start");
                            lastFacePalm = DateTime.Now;
                            currentState = MyStates.FACE_PALM;
                        }

                        break;
                    case MyStates.RIGHT_HAND_IN:
                        if (!RightHandIn && RightHandAboveElbow && rightState == HandState.Open)
                        {
                            previousSlide();
                            currentState = MyStates.NEUTRAL;
                            Trace.WriteLine("Previous");
                        }
                        else if ((RightHandIn && rightState == HandState.Open) || !RightHandAboveElbow)
                        {
                            Trace.WriteLine("Aborting Previous");
                            currentState = MyStates.NEUTRAL;
                        }
                        else if (Clap)
                        {
                            Trace.WriteLine("Clapping");
                            currentState = MyStates.CLAP;
                        }

                        break;

                    case MyStates.CLAP:

                        if (NotClap)
                        {
                            currentState = MyStates.NEUTRAL;
                            Trace.WriteLine("End Clapping");
                            if (DateTime.Now - lastClap < TimeSpan.FromSeconds(1) && lastClap != DateTime.MinValue)
                                endPresentation();
                            else
                                startPresentation();
                            lastClap = DateTime.Now;
                        }
                        break;
                    case MyStates.FACE_PALM:
                        if (!FacePalm)
                        {
                            currentState = MyStates.NEUTRAL;
                            Trace.WriteLine("Aborting Face Palm");
                        }

                        if (DateTime.Now - lastFacePalm > TimeSpan.FromSeconds(0.5) && lastFacePalm != DateTime.MinValue)
                        {
                            toggleBlackScreen();
                            currentState = MyStates.BLACK;
                            Trace.WriteLine("Face Palm!");
                        }
                        break;
                    case MyStates.BLACK:
                        if (!FacePalm)
                        {
                            Trace.WriteLine("Ending Face Palm");
                            currentState = MyStates.NEUTRAL;
                            lastFacePalm = DateTime.MinValue;
                            toggleBlackScreen();
                        }
                        break;
                }
            }
            else
            {
                if (ToggleGesture)
                {
                    currentState = MyStates.NEUTRAL;
                    Trace.WriteLine("Gesture enabling");
                }
            }
        }

       


        #region Poses

        // Transition condition for right arm stretched out
        public bool RightArmOut
        {
            get { return ((trackedBody.Joints[JointType.HandRight].Position.X - trackedBody.Joints[JointType.ShoulderRight].Position.X) > 0.5); }
        }

            public bool LeftHandIn
        {
            get
            {
                return trackedBody.Joints[JointType.HandLeft].Position.X > trackedBody.Joints[JointType.ElbowLeft].Position.X;
            }
        }

        public bool LeftHandAboveElbow
        {
            get
            {
                return trackedBody.Joints[JointType.HandLeft].Position.Y > trackedBody.Joints[JointType.ElbowLeft].Position.Y;
            }
        }

        public bool RightHandIn
        {
            get { return trackedBody.Joints[JointType.HandRight].Position.X < trackedBody.Joints[JointType.ElbowRight].Position.X; }

        }
        public bool RightHandAboveElbow
        {
            get { return trackedBody.Joints[JointType.HandRight].Position.Y > trackedBody.Joints[JointType.ElbowRight].Position.Y; }
        }

        public bool Clap
        {
            get { return Math.Abs(trackedBody.Joints[JointType.HandRight].Position.X - trackedBody.Joints[JointType.HandLeft].Position.X) < 0.05 &&
                    Math.Abs(trackedBody.Joints[JointType.HandRight].Position.Y - trackedBody.Joints[JointType.HandLeft].Position.Y) < 0.05; }
        }

        public bool NotClap
        {
            get
            {
                return Math.Abs(trackedBody.Joints[JointType.HandRight].Position.X - trackedBody.Joints[JointType.HandLeft].Position.X) >= 0.2 ||
                  Math.Abs(trackedBody.Joints[JointType.HandRight].Position.Y - trackedBody.Joints[JointType.HandLeft].Position.Y) >= 0.2;
            }
        }

        public bool FacePalm
        {
            get
            {
                return (Math.Abs(trackedBody.Joints[JointType.HandLeft].Position.X - trackedBody.Joints[JointType.Head].Position.X) < 0.1 &&
                    Math.Abs(trackedBody.Joints[JointType.HandLeft].Position.Y - trackedBody.Joints[JointType.Head].Position.Y) < 0.1);
                    

            }
        }

        public bool ToggleGesture
        {
            get
            {
                return (Math.Abs(trackedBody.Joints[JointType.HandRight].Position.X - trackedBody.Joints[JointType.Head].Position.X) < 0.1 &&
                    Math.Abs(trackedBody.Joints[JointType.HandRight].Position.Y - trackedBody.Joints[JointType.Head].Position.Y) < 0.1);

            }
        }





        #endregion

            #region PowerPoint Operations

            /// <summary>
            /// Advances to the next PowerPoint slide
            /// </summary>
        private void nextSlide()
        {
            // Sends a right arrow keystroke to the currently active application window
            SendKeys.SendWait("{RIGHT}");
            HighlightBody(trackedBody);
        }

        /// <summary>
        /// Returns to the previous PowerPoint slide
        /// </summary>
        private void previousSlide()
        {
            // Sends a left arrow keystroke to the currently active application window
            SendKeys.SendWait("{LEFT}");
            HighlightBody(trackedBody);
        }

        /// <summary>
        /// Jumps to the first slide of the presentation
        /// </summary>
        private void jumpToStart()
        {
            SendKeys.SendWait("{HOME}");
            HighlightBody(trackedBody);
        }

        /// <summary>
        /// Jumps to the last slide of the presentation
        /// </summary>
        private void jumpToEnd()
        {
            SendKeys.SendWait("{END}");
            HighlightBody(trackedBody);
        }

        /// <summary>
        /// Display a blank black slide, or return to the presentation from a blank black slide
        /// </summary>
        private void toggleBlackScreen()
        {
            SendKeys.SendWait("b");
            HighlightBody(trackedBody);
        }

        /// <summary>
        /// Toggle media playback between play and pause
        /// </summary>
        private void togglePlayback()
        {
            // Send tab first to focus the video
            // This workaround is needed because the video will not start if it is
            // not focused or the mouse does not hover over it (for whatever reason)
            // Unfortunately, this means that we can have no other videos or
            // hyperlinks on the same slide because tab will cycle through them
            //SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{TAB}%p");
            HighlightBody(trackedBody);
            //SendKeys.SendWait("+{TAB}");
        }

        /// <summary>
        /// Seek forward in the media playback
        /// </summary>
        private void seekForward()
        {
            SendKeys.SendWait("%+{PGDN}");
            HighlightBody(trackedBody);
        }

        /// <summary>
        /// Seek backward in the media playback
        /// </summary>
        private void seekBackward()
        {
            SendKeys.SendWait("%+{PGUP}");
            HighlightBody(trackedBody);
        }

        /// <summary>
        /// Seek backward in the media playback
        /// </summary>
        private void startPresentation()
        {
            SendKeys.SendWait("{F5}");
            HighlightBody(trackedBody);
        }

        private void endPresentation()
        {
            SendKeys.SendWait("{ESC}");
            HighlightBody(trackedBody);
        }


        #endregion

        #region StickMen Operations from Microsoft's SlideShow Example

        /// <summary>
        /// Time until skeleton ceases to be highlighted.
        /// </summary>
        private DateTime highlightTime = DateTime.MinValue;

        /// <summary>
        /// The ID of the skeleton to highlight.
        /// </summary>
        private ulong highlightId = 8;

        /// <summary>
        /// The ID if the skeleton to be tracked.
        /// </summary>
        private ulong nearestId = 8;

        /// <summary>
        /// Array of arrays of contiguous line segements that represent a skeleton.
        /// </summary>
        private static readonly JointType[][] SkeletonSegmentRuns = new JointType[][]
        {
            new JointType[] 
            { 
                JointType.Head, JointType.SpineShoulder, JointType.SpineBase 
            },
            new JointType[] 
            { 
                JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
                JointType.SpineShoulder,
                JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight
            },
            new JointType[]
            {
                JointType.FootLeft, JointType.AnkleLeft, JointType.KneeLeft, JointType.HipLeft,
                JointType.SpineBase,
                JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight
            }
        };

        /// <summary>
        /// Select a skeleton to be highlighted.
        /// </summary>
        /// <param name="skeleton">The skeleton</param>
        private void HighlightBody(Body body)
        {
            // Set the highlight time to be a short time from now.
            this.highlightTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.5);

            // Record the ID of the skeleton.
            this.highlightId = body.TrackingId;
        }

        /// <summary>
        /// Draw stick men for all the tracked bodies.
        /// </summary>
        /// <param name="skeletons">The bodies to draw.</param>
        private void DrawStickMen(Body[] bodies)
        {
            // Remove any previous skeletons.
            StickMen.Children.Clear();

            foreach (var body in bodies)
            {
                // Only draw tracked bodies.
                if (body.IsTracked)
                {
                    // Draw a background for the next pass.
                    this.DrawStickMan(body, Brushes.WhiteSmoke, 7);
                }
            }

            foreach (var body in bodies)
            {
                // Only draw tracked bodies.
                if (body.IsTracked)
                {
                    // Pick a brush, Red for a body that recently performed gestures, black for the nearest, gray otherwise.
                    var brush = DateTime.UtcNow < this.highlightTime && body.TrackingId == this.highlightId ? Brushes.Red :
                        body.TrackingId == this.nearestId ? Brushes.Black : Brushes.Gray;

                    // Draw the individual body.
                    this.DrawStickMan(body, brush, 3);
                }
            }
        }

        /// <summary>
        /// Draw an individual body.
        /// </summary>
        /// <param name="skeleton">The body to draw.</param>
        /// <param name="brush">The brush to use.</param>
        /// <param name="thickness">This thickness of the stroke.</param>
        private void DrawStickMan(Body body, Brush brush, int thickness)
        {
            Debug.Assert(body.IsTracked, "The body is being tracked.");

            foreach (var run in SkeletonSegmentRuns)
            {
                var next = this.GetJointPoint(body, run[0]);
                for (var i = 1; i < run.Length; i++)
                {
                    var prev = next;
                    next = this.GetJointPoint(body, run[i]);

                    var line = new Line
                    {
                        Stroke = brush,
                        StrokeThickness = thickness,
                        X1 = prev.X,
                        Y1 = prev.Y,
                        X2 = next.X,
                        Y2 = next.Y,
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeStartLineCap = PenLineCap.Round
                    };

                    StickMen.Children.Add(line);
                }
            }
        }

        /// <summary>
        /// Convert body joint to a point on the StickMen canvas.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <param name="jointType">The joint to project.</param>
        /// <returns>The projected point.</returns>
        private Point GetJointPoint(Body body, JointType jointType)
        {
            var joint = body.Joints[jointType];

            // Points are centered on the StickMen canvas and scaled according to its height allowing
            // approximately +/- 1.5m from center line.
            var point = new Point
            {
                X = (StickMen.Width / 2) + (StickMen.Height * joint.Position.X / 3),
                Y = (StickMen.Width / 2) - (StickMen.Height * joint.Position.Y / 3)
            };

            return point;
        }


        #endregion
    }
}
