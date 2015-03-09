using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using System.Threading;
namespace HMC_6343
{
    public class HMC_6343
    {
        const ushort HMC6343_ADDRESS = 0x32 >> 1;
        const int CLOCK_FREQ = 100;
        const int DELAY = 100;

        //Write buffer
        byte[] headingCommand = new byte[] { 0x50 }; // Read bearing command

        //Read buffer
        private byte[] inBuffer = new byte[6]; // Six bytes, MSB followed by LSB for each heading, pitch and roll

        public float Heading // Heading in degrees
        {
            get;
            private set;
        }
        public float Pitch // Pitch angle in degrees
        {
            get;
            private set;
        }

        public float Roll // Roll angle in degrees
        {
            get;
            private set;
        }

        public float AccelX
        {
            get;
            private set;
        }
        public float AccelY
        {
            get;
            private set;
        }
        public float AccelZ
        {
            get;
            private set;
        }

        //Create Read & Write transactions
        readonly I2CDevice _deviceInterface;
        readonly I2CDevice.I2CTransaction[] _txnPostHeading;
        readonly I2CDevice.I2CTransaction[] _txnPostAccel;
        readonly I2CDevice.I2CTransaction[] _txnReadData;


        /// <summary>
        /// Initializes a new instance of the <see cref="HMC6343"/> class.
        /// </summary>
        public HMC_6343()
        {
       
            _deviceInterface = new I2CDevice(new I2CDevice.Configuration((ushort)HMC6343_ADDRESS, CLOCK_FREQ));
                 Thread.Sleep(500);
            _txnPostHeading = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(headingCommand) };
            _txnPostAccel = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new byte[] { 0x40 }) };
            _txnReadData = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(inBuffer) };
            byte[] opBuffer=new byte[1];

            
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            _deviceInterface.Dispose();
        }
        private void _decodeHeading(byte[] buffer)
        {
            int heading= (int)(((ushort)buffer[0]) << 8 | (ushort)buffer[1]);
            int pitch = (int)(((ushort)buffer[2]) << 8 | (ushort)buffer[3]);
            int roll = (int)(((ushort)buffer[4]) << 8 | (ushort)buffer[5]);
            Heading = heading / 10.0f;
            Pitch = (float)pitch / 10.0f;
            if (Pitch > 6400)
                Pitch = (Pitch - 6400 - 155);

            Roll = (float)roll / 10.0f;
            if (Roll > 6400)
                Roll = (Roll - 6400 - 155);
        }
        private void _decodeAccel(byte[] buffer)
        {
            int x = (int)(((ushort)buffer[0]) << 8 | (ushort)buffer[1]);
            int y = (int)(((ushort)buffer[2]) << 8 | (ushort)buffer[3]);
            int z = (int)(((ushort)buffer[4]) << 8 | (ushort)buffer[5]);
            AccelX = x;
            AccelY = y;
            AccelZ = z;
        }
        /// <summary>
        /// Reads heading, pitch and roll and converts to usable degrees.
        /// </summary>
        public void Update()
        {
            _deviceInterface.Execute(_txnPostHeading, DELAY);
            Thread.Sleep(1);  // Give the compass the requested 1ms delay before reading
            int i=_deviceInterface.Execute(_txnReadData, DELAY);
            _decodeHeading(inBuffer);

            _deviceInterface.Execute(_txnPostAccel, DELAY);
            Thread.Sleep(1);  // Give the compass the requested 1ms delay before reading
            _deviceInterface.Execute(_txnReadData, DELAY);
            _decodeAccel(inBuffer);

        }

    }
}
