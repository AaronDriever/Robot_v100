using System;
using System.IO;
using Microsoft.SPOT.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using System.IO.Ports;

// pins:
// Servo pins: Di8, Di7
// sonar pins: Di6, Di5
// compass pins: Di2, Di3
// GPS pins: D31, D33, D2p(connected to the modual vcc so that the unit ca powered down.
// Xbee pins: 

namespace Robot_v100
{

    using HC_SR04; // HC_SR04 Sonar sensor
    using HMC_6343; // HMC_6343 Compass module
    using COM_GPS; // GPS 

    public enum MoveDirection
    {
        Fwd, Back

    }
    public enum RotateDirection
    {
        CW,
        CCW,
    }
    public sealed class Robot // The Robot class, contains everything
    {
        public class Brain // The Brain class, contains sensors stuff
        {
            Robot _parent;
            SonarSensor _sonar;
            HMC_6343 _compass;
            COM_GPS _gps;

            public Brain(Robot parent, SonarSensor sensor, HMC_6343 compass, COM_GPS gps) // Brain sensor assignments
            {
                _parent = parent;
                _sonar = sensor;
                _compass = compass;
                _gps = gps;
            }
            public double FreeSpaceFwd // not sure yet
            {
                get;
                private set;
            }
            public double FreeSpaceRight // not sure yet
            {
                get;
                private set;
            }
            public double FreeSpaceLeft // not sure yet
            {
                get;
                private set;
            }
            public double Heading // return the compass heading.
            {
                get
                {
                    return _compass.Heading;
                }
            }
            public double Pitch // returns the pitch angle.
            {
                get
                {
                    return _compass.Pitch;
                }
            }
            public double Roll // returns the roll angle.
            {
                get
                {
                    return _compass.Roll;
                }
            }
            public double Distance
            {
                get
                {
                    return _sonar.CurrentDistance;
                }
            }
            public void UpdateCompass()
            {
                _compass.Update();
            }
            public void UpdateRanges()
            {

            }
            public void MyLocation()
            {
                _gps.Date.ToString();
            }


        }
        public class Servo
        {
            public static readonly uint OFF_VALUE = 1500;
            private PWM _servoPin;
            public Servo(FEZ_Pin.PWM pin)
            {
                _servoPin = new PWM((PWM.Pin)pin);
                Value = OFF_VALUE;
            }
            private uint _pwmPeriod = 20000000;
            private uint _pwmHigh;
            public uint Value
            {
                get
                {
                    return _pwmHigh;
                }
                set
                {
                    _pwmHigh = value * 1000;
                    _servoPin.SetPulse(_pwmPeriod, _pwmHigh);
                }
            }
        }
        public class Treads
        {
            private Robot _parent;
            private Servo _treadR;
            private Servo _treadL;
            public Treads(Robot parent, FEZ_Pin.PWM left, FEZ_Pin.PWM right) // assigns Servos to treads.
            {
                _treadL = new Servo(left);
                _treadR = new Servo(right);
            }
            public void Rotate(RotateDirection dir)//, int duration
            {
                switch (dir)
                {
                    case RotateDirection.CW:
                        _treadL.Value = 1486;
                        _treadR.Value = 1454;

                        break;
                    case RotateDirection.CCW:
                        _treadL.Value = 1606;
                        _treadR.Value = 1574;
                        break;
                }
                //Thread.Sleep(duration);
                //Stop();
            }
            public void RotateDegrees(RotateDirection dir, double degrees) // rotate a given direction for a given degrees
            {
                Robot _parent = new Robot(FEZ_Pin.PWM.Di9, FEZ_Pin.PWM.Di8, FEZ_Pin.Digital.Di4, FEZ_Pin.Digital.Di5);
                _parent.MyBrain.UpdateCompass();
                double currentHeading = _parent.MyBrain.Heading;

                double newHeading = currentHeading + degrees;
                if (currentHeading < newHeading)
                {
                    _parent.MyTreads.Rotate(RotateDirection.CW);
                }
                if (currentHeading > newHeading)
                {
                    _parent.MyTreads.Rotate(RotateDirection.CCW);
                }
            }
            public void RotateTo(RotateDirection dir, double toAngle)
            {
                //Figure out current difference between current heading and desired heading;

                double dA = _parent.MyBrain.Heading - toAngle;

                //While we're not within 5 degrees of our desired heading, we use the square of the values for speed, since we don't have an Abs(double) method and we don't want
                //to use any sort of sqrt procedure.
                while ((dA * dA) > 25)
                {
                    if (dA > 0)
                    {
                        Rotate(RotateDirection.CCW);
                    }
                    else
                    {
                        Rotate(RotateDirection.CW);
                    }
                    _parent.MyBrain.UpdateCompass();
                }
            }
            /// <summary>
            /// Drive straight either forward or backwards.
            /// </summary>
            /// <param name="dir">Which direction to drive</param>
            /// <param name="duration">How long to drive</param>
            public void DriveForTime(MoveDirection dir, int duration) // Drive for time method
            {

                switch (dir)
                {
                    case MoveDirection.Fwd:
                        _treadL.Value = 1000;

                        _treadR.Value = 2000;
                        break;
                    case MoveDirection.Back:
                        _treadL.Value = 2000;

                        _treadR.Value = 1000;
                        break;
                }
                Thread.Sleep(duration);
                Stop();
            }
            /// <summary>
            /// Drive straight either forward or backwards.
            /// </summary>
            /// <param name="dir">Which direction to drive</param>
            /// <param name="cm">How far to drive</param>
            public void DriveForDistance(MoveDirection dir, double cm) // Drive for distance method, unfinished
            {

                double distanceNow = _parent.MyBrain.Distance;
                switch (dir)
                {
                    case MoveDirection.Fwd:
                        _treadL.Value = 1000;

                        _treadR.Value = 2000;
                        break;
                    case MoveDirection.Back:
                        _treadL.Value = 2000;

                        _treadR.Value = 1000;
                        break;
                }

            }
            public void Drive(MoveDirection dir) // Drive for distance method, unfinished
            {
                switch (dir)
                {
                    case MoveDirection.Fwd:
                        _treadL.Value = 1000;

                        _treadR.Value = 2000;
                        break;
                    case MoveDirection.Back:
                        _treadL.Value = 2000;

                        _treadR.Value = 1000;
                        break;
                }

            }
            public void Stop() // Stop method
            {
                _treadL.Value = Servo.OFF_VALUE;
                _treadR.Value = Servo.OFF_VALUE;
            }
        }
        public class SonarSensor
        {
            HC_SR04 _sensor;
            public SonarSensor(FEZ_Pin.Digital trigger, FEZ_Pin.Digital echo)
            {
                _sensor = new HC_SR04((Cpu.Pin)trigger, (Cpu.Pin)echo);
            }
            /// <summary>
            /// Reads distance and converts it to cm.
            /// </summary>
            public double CurrentDistance
            {
                get
                {
                    double Distance = -1;
                    //Debug.Print("Ping");

                    long ticks = _sensor.Ping();
                    //Debug.Print(ticks.ToString());
                    if (ticks > 0L)
                    {
                        double centimeters = _sensor.TicksToInches(ticks) * 2.54;
                        //Debug.Print("Distance CM: " + centimeters.ToString());

                        Distance = centimeters; //centimeters
                    }
                    return Distance;
                }
            }
        }
        public class MyLocation
        {
            COM_GPS _gps;
            public MyLocation()
            {

                while (true)
                    Debug.Print(_gps.FixTime.ToString());
            }
        }
        private double _distanceR;
        private double _distanceL;
        private double _distanceF;
        public Treads MyTreads;
        public Brain MyBrain;
        public Robot(FEZ_Pin.PWM servoRPin, FEZ_Pin.PWM servoLPin, FEZ_Pin.Digital rangeTriggerPin, FEZ_Pin.Digital rangeEchoPin)
        {
            MyTreads = new Treads(this, servoLPin, servoRPin);
            MyBrain = new Brain(this, new SonarSensor(rangeTriggerPin, rangeEchoPin), new HMC_6343(), new COM_GPS("COM4", 4800));
        }




        public void RobotLoop() // Do robot stuff here.
        {
            //MyBrain.Distance.ToString();
            //MyBrain.MyLocation();


            //While we're not within 5 degrees of our desired heading, we use the square of the values for speed, since we don't have an Abs(double) method and we don't want
            //to use any sort of sqrt procedure         

            

             


            // MyTreads.Stop()
            double numToEnter = 200;
            double sum = 0;
            double average = 0;
            double input;
            //int someavg = (int)average;

            // Get 10 numbers from the user
            for (int i = 0; i != numToEnter; ++i)
            {
                input = MyBrain.Heading;

                sum += input;
            }

            // Get average and print it
            average = sum / numToEnter;
            
            Debug.Print("The average Heading is: " + (int)average);

            //double[] arr = new double[5] { MyBrain.Heading, MyBrain.Heading, MyBrain.Heading, MyBrain.Heading, MyBrain.Heading };
             double currentHeading = (int)average;

             double newHeading = 300;
             if (currentHeading < newHeading + 5)
             {
                 MyTreads.Rotate(RotateDirection.CW);
             }
             if (currentHeading > newHeading - 5)
             {
                 MyTreads.Rotate(RotateDirection.CCW);
             }


        }

    }


    public class Program
    {
        public static void Main()
        {
            Robot robot = new Robot(FEZ_Pin.PWM.Di9, FEZ_Pin.PWM.Di8, FEZ_Pin.Digital.Di4, FEZ_Pin.Digital.Di5);// Pin assignment.

            while (true)
            {
                 robot.MyBrain.UpdateCompass();
                Debug.Print("The Real    Heading is: " + robot.MyBrain.Heading.ToString());
               
                //Debug.Print("Compass heading: " + robot.MyBrain.Heading.ToString()); // should print the compass heading.
                // Debug.Print("Compass pitch: " + MyBrain.Pitch.ToString());
                // Debug.Print("Compass roll: " + MyBrain.Roll.ToString());
                // Debug.Print("Distance CM:" + MyBrain.Distance.ToString());
                robot.RobotLoop();




            }
        }
    }
}

/*
 The End!
*/
