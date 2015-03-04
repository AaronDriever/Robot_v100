using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;

namespace HC_SR04
{
    public class HC_SR04
    {
        private OutputPort portOut;
        private InterruptPort interIn;
        private long beginTick;
        private long endTick;
        private long minTicks;
        private double inchConversion;
        private double version;
        

        public HC_SR04(Cpu.Pin pinTrig, Cpu.Pin pinEcho)
        {
            portOut = new OutputPort(pinTrig, false);
            interIn = new InterruptPort(pinEcho, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
            interIn.OnInterrupt += new NativeEventHandler(interIn_OnInterrupt);
            minTicks = 6200L;
            inchConversion = 1440.0;
            version = 1.1;
        }

        public double Version
        {
            get
            {
                return version;
            }
        }

        public long Ping()
        {
            // Reset Sensor
            portOut.Write(true);
            Thread.Sleep(1); 

            // Start Clock
            endTick = 0L;
            beginTick = System.DateTime.Now.Ticks;
            // Trigger Sonic Pulse
            portOut.Write(false);
            Thread.Sleep(50);
            if (endTick > 0L)
            {
                long elapsed = endTick - beginTick;
                elapsed -= minTicks;
                if (elapsed < 0L)
                {
                    elapsed = 0L;
                }
                return elapsed;
            }
            return -1L;
        }

        void interIn_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            endTick = time.Ticks;
        }

        public double TicksToInches(long ticks)
        {
            return (double)ticks / inchConversion;
        }

        public double InchCoversionFactor
        {
            get
            {
                return inchConversion;
            }
            set
            {
                inchConversion = value;
            }
        }

        public long LatencyTicks
        {
            get
            {
                return minTicks;
            }
            set
            {
                minTicks = value;
            }
        }
    }
}

/*
 * 
 * put this in the main
 * 
            HC_SR04 sensor = new HC_SR04((Cpu.Pin)FEZ_Pin.Digital.Di8, (Cpu.Pin)FEZ_Pin.Digital.Di7);

            while (true)
            {
                Debug.Print("Ping");
                long ticks = sensor.Ping();
                if (ticks > 0L)
                {
                    double inches = sensor.TicksToInches(ticks);
                    double centimeters = inches * 2.54;
                    Debug.Print("Distance CM: " + centimeters);
                }
            }
*/