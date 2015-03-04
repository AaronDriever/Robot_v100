using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
//using HC_SR04;


namespace Robot_v100
{
    using HC_SR04; // this lets us use the HC_SR04.cs that is added.


    public class Program
    {
        // put pin assignments here.

        static PWM servoR = new PWM((PWM.Pin)FEZ_Pin.PWM.Di9); // Right servo control pin
        static PWM servoL = new PWM((PWM.Pin)FEZ_Pin.PWM.Di8); // Left servo control pin
        static HC_SR04 sensor = new HC_SR04((Cpu.Pin)FEZ_Pin.Digital.Di4, (Cpu.Pin)FEZ_Pin.Digital.Di5);

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

        // Functons here.


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
                Distance = centimeters;

            }
            return Distance; 
                             
        }


        public static void Forward()
        {
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
        public static void AlphaProtocol()
        {
            GetDistance();
            double CurrentDistance = GetDistance();
            
            if (CurrentDistance < 10)
            {
                Debug.Print("Distance CM: " + CurrentDistance);
                Stop();
                Thread.Sleep(1000);
                Reverse();
                Thread.Sleep(1000);
                RotateLeft();
                Thread.Sleep(1000);
            }
            else
            {
                Forward();
            }
        }
    }
}
/*
 
*/
