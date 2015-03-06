using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;



namespace Robot_v100
{
    using HC_SR04; // this lets us use the HC_SR04.cs that is added.
    public enum MoveDirection
    {
        Fwd, Back

    }
    public enum RotateDirection
    {
        CW,
        CCW,
    }
    public sealed class Robot
    {
        private class Brain
        {
            Robot _parent;
            SonarSensor _sonar;
            HMC_6343 _compass;
            public Brain(Robot parent,SonarSensor sensor, HMC_6343 compass)
            {
                _parent = parent;
                _sonar = sensor;
                _compass = compass;
            }
            public double FreeSpaceFwd
            {
                get;
                private set;
            }
            public double FreeSpaceRight
            {
                get;
                private set;
            }
            public double FreeSpaceLeft
            {
                get;
                private set;
            }
            public double Heading
            {
                get
                {
                    return _compass.Heading;
                }
                
            }
            public double Pitch
            {
                get
                {
                    return _compass.Pitch;
                }

            }
            public void UpdateCompass()
            {
                _compass.Update();
                
            }
            public void UpdateRanges()
            {
                _parent._treads.Stop();

            }
        }
        private class Servo
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
        private class SonarSensor
        {
            HC_SR04 _sensor;
            public SonarSensor(FEZ_Pin.Digital trigger, FEZ_Pin.Digital echo)
            {
                _sensor = new HC_SR04((Cpu.Pin)trigger, (Cpu.Pin)echo);
            }
            public double CurrentDistance
            {
                get
                {
                    double Distance = -1;
                    Debug.Print("Ping");
                    long ticks = _sensor.Ping();
                    if (ticks > 0L)
                    {
                        double centimeters = _sensor.TicksToInches(ticks) * 2.54;
                        Debug.Print("Distance CM: " + centimeters.ToString());
                        Distance = centimeters; //centimeters

                    }
                    return Distance;
                }
            }
        }
        private class Treads
        {
            private Robot _parent;
            private Servo _treadR;
            private Servo _treadL;
            public Treads(Robot parent,FEZ_Pin.PWM left, FEZ_Pin.PWM right)
            {
                _treadL = new Servo(left);
                _treadR = new Servo(right);
            }
            public void Rotate(RotateDirection dir, int duration)
            {
                switch (dir)
                {
                    case RotateDirection.CW:
                        _treadL.Value = 1000;
                        _treadR.Value = 1000;

                        break;
                    case RotateDirection.CCW:
                        _treadL.Value = 2000;
                        _treadR.Value = 2000;
                        break;


                }
                Thread.Sleep(duration);
                Stop();
            }
            
            public void Rotate(double degrees){
                _parent.MyBrain.UpdateCompass();
                double newHeading=_parent.MyBrain.Heading+degrees;
                if(newHeading>360){
                }
            }
            public void RotateTo(RotateDirection dir, double toAngle)
            {
                //Figure out current difference between current heading and desired heading;
                _parent.MyBrain.UpdateCompass();
                double dA=_parent.MyBrain.Heading-toAngle;
                
                //While we're not within 5 degrees of our desired heading, we use the square of the values for speed, since we don't have an Abs(double) method and we don't want
                //to use any sort of sqrt procedure.
                while((dA*dA)>25){
                     if(dA>0){
                         Rotate(RotateDirection.CCW,100);
                     }
                     else{
                         Rotate(RotateDirection.CW,100);
                     }
                    _parent.MyBrain.UpdateCompass();
                }
            }

            /// <summary>
            /// Drive straight either foward or backwards.
            /// </summary>
            /// <param name="dir">Which direction to drive</param>
            /// <param name="duration">How far to drive</param>
            public void Drive(MoveDirection dir, int duration)
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
            public void Drive(MoveDirection dir, double cm)
            {
            }
            public void Stop()
            {
                _treadL.Value = Servo.OFF_VALUE;
                _treadR.Value = Servo.OFF_VALUE;
            }
        }
        private double _distanceR;
        private double _distanceL;
        private double _distanceF;
        public Treads MyTreads;
        public Brain MyBrain;
        public Robot(
            FEZ_Pin.PWM servoRPin,
            FEZ_Pin.PWM servoLPin,
            FEZ_Pin.Digital rangeTriggerPin,
            FEZ_Pin.Digital rangeEchoPin)
        {

            MyTreads= new Treads(this,servoLPin, servoRPin);
            MyBrain= new Brain(this,new SonarSensor(rangeTriggerPin, rangeEchoPin),new HMC_6343());

        }




        public void RobotLoop()
        {
            while (true)
            {
                Debug.Print("Current Distance: " + _rangeFinder.CurrentDistance.ToString());
                Thread.Sleep(1000);
                _treads.Rotate(RotateDirection.CW, 1000);
                _treads.Rotate(RotateDirection.CCW, 1000);
                //Set up new direction based on peripheral distances
                //move foward until within 10 cm;

            }
        }
    }
    public class Program
    {
        // put pin assignments here.

        //static PWM servoR = null; //new PWM((PWM.Pin)FEZ_Pin.PWM.Di9); // Right servo control pin
        //static PWM servoL = null;// new PWM((PWM.Pin)FEZ_Pin.PWM.Di8); // Left servo control pin
        //static HC_SR04 sensor = null; //new HC_SR04((Cpu.Pin)FEZ_Pin.Digital.Di4, (Cpu.Pin)FEZ_Pin.Digital.Di5);
        //static int tock = 500;

        public static void Main()
        {
            //Declare threads here

            //Start threads here.


            //Start functions here.

            Robot robot = new Robot(FEZ_Pin.PWM.Di9, FEZ_Pin.PWM.Di8, FEZ_Pin.Digital.Di4, FEZ_Pin.Digital.Di5);
            robot.RobotLoop();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
       /* public static double GetDistance() // Ping!
        {
            double Distance = 0;

            Debug.Print("Ping");
            long ticks = sensor.Ping();
            if (ticks > 0L)
            {

                double inches = sensor.TicksToInches(ticks);
                double centimeters = inches * 2.54;
                Debug.Print("Distance CM: " + centimeters);
                Distance = centimeters; //centimeters

            }
            return Distance;

        }

        public static void AlphaProtocol() // My first directive
        {

            double CurrentDistance = GetDistance();
            double LeftDistance;
            double RightDistance;
            GetDistance();
            Debug.Print("Distance CM: " + CurrentDistance);

            if (CurrentDistance < 10) // if distance is less than 10cm...
            {


                Move.Stop();
                Thread.Sleep(tock);
                Move.Reverse();
                Thread.Sleep(tock);
                Move.RotateLeft();
                Thread.Sleep(tock);
                Move.Stop();
                Thread.Sleep(tock);
                LeftDistance = GetDistance(); //
                Thread.Sleep(tock);
                Move.RotateRight();
                Thread.Sleep(tock * 2);
                Move.Stop();
                Thread.Sleep(tock);
                RightDistance = GetDistance(); //
                Thread.Sleep(tock);

                if (LeftDistance > RightDistance)
                {
                    Move.RotateLeft();
                    Thread.Sleep(tock * 2);
                }
                //  else
                //  {
                //      Forward();
                //  }

            }
            else
            {
                Move.Forward();
            }
        }

        public static void BetaProtocol() // not ready yet
        {

        }


        class Move // move class
        {
            /// <summary>Movement Class.
            /// here all movement method go.
            /// </summary>
            public static void Forward()
            {
                int dur;
                Debug.Print("Forward");
                servoL.SetPulse(20 * 1000 * 1000, 1000 * 1000); // left server forward
                //Thread.Sleep(1000);//wait for a second
                servoR.SetPulse(20 * 1000 * 1000, 2000 * 1000); // right servo forward
                //Thread.Sleep(1000);//wait for a second  
            }

            public static void Reverse()
            {
                Debug.Print("Reverse");
                servoL.SetPulse(20 * 1000 * 1000, 2000 * 1000); // left servo reverse
                servoR.SetPulse(20 * 1000 * 1000, 1000 * 1000); // right servo revers
            }

            public static void RotateLeft()
            {
                Debug.Print("Left");
                servoL.SetPulse(20 * 1000 * 1000, 2000 * 1000); // left server forward  
                servoR.SetPulse(20 * 1000 * 1000, 2000 * 1000); // right servo reverse          
            }

            public static void RotateRight()
            {
                Debug.Print("Right");
                servoL.SetPulse(20 * 1000 * 1000, 1000 * 1000); // left servo reverse
                servoR.SetPulse(20 * 1000 * 1000, 1000 * 1000); // right servo forward
            }

            public static void Stop()
            {
                Debug.Print("Stop");
                servoL.SetPulse(20 * 1000 * 1000, 1500 * 1000); // left servo reverse
                servoR.SetPulse(20 * 1000 * 1000, 1500 * 1000); // right servo forward
            }
        }
    }*/
}
/*
 The End!
*/
