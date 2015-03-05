using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;


namespace Robot_v100
{
    using HC_SR04; // this lets us use the HC_SR04.cs that is added.


    public class Program
    {
        // put pin assignments here.

        static PWM servoR = new PWM((PWM.Pin)FEZ_Pin.PWM.Di9); // Right servo control pin
        static PWM servoL = new PWM((PWM.Pin)FEZ_Pin.PWM.Di8); // Left servo control pin
        static HC_SR04 sensor = new HC_SR04((Cpu.Pin)FEZ_Pin.Digital.Di4, (Cpu.Pin)FEZ_Pin.Digital.Di5);
        static int tock = 500;

        public static void Main()
        {
            //Declare threads here

            //Start threads here.


            //Start functions here.



            while (true)
            {

                AlphaProtocol();

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static double GetDistance() // Ping!
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
    }
}
/*
 The End!
*/
