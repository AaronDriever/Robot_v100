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
        byte[] headingCommand = new byte[] { 0x50 };             // Read bearing command
        // newly added
        byte[] accelCommand = new byte[] { 0x40 };               // Post Accel Data: AxMSB, AxLSB, AyMSB, AyLSB, AzMSB, AzLSB
        byte[] magCommand = new byte[] { 0x45 };                 // Post Mag Data: MxMSB, MxLSB, MyMSB, MyLSB, MzMSB, MzLSB
        byte[] tiltCommand = new byte[] { 0x55 };                // Post Tilt Data: PitchMSB, PitchLSB, RollMSB, RollLSB, TempMSB, TempLSB
        byte[] op1Command = new byte[] { 0x65 };                 // Post OP Mode 1: Read the current value of OP Mode 1 register
        byte[] enterCalCommand = new byte[] { 0x71 };            // Enter User Calibration Mode
        byte[] exitCalCommand = new byte[] { 0x7E };             // Exit User Calibration Mode
        byte[] levelCommand = new byte[] { 0x72 };               // Level Orientation (X=forward, +Z=up) (default)                     
        byte[] uprightsidewaysCommand = new byte[] { 0x73 };     // Upright Sideways Orientation (X=forward, Y=up)   
        byte[] uprightflatfrontCommand = new byte[] { 0x74 };    // Upright Flat Front Orientation (Z=forward, -X=up)
        byte[] runCommand = new byte[] { 0x75 };                 // Enter Run Mode (from Standby Mode)
        byte[] standbyCommand = new byte[] { 0x76 };             // Enter Standby Mode (from Run Mode)
        byte[] resetCommand = new byte[] { 0x82 };               // Reset the Processor
        byte[] entersleepCommand = new byte[] { 0x83 };          // Enter Sleep Mode (from Run Mode)
        byte[] exitsleepCommand = new byte[] { 0x84 };           // Exit Sleep Mode (to Standby Mode)
        //
        byte[] readEEPromCommand = new byte[] { 0xE1 };          // EEPROM Address  Response Bytes (Binary), Data (1 Byte) Read from EEPROM
        byte[] writeEEPromCommand = new byte[] { 0xF1 };         // EEPROM Address  Argument 2 Byte (Binary), Data Write to EEPROM
        //
        byte[] X_OffsetLSB = new byte[] { 0x0E };                // Hard-Iron Calibration Offset for the X-axis
        byte[] X_OffsetMSB = new byte[] { 0x0F };
        byte[] Y_OffsetLSB = new byte[] { 0x10 };
        byte[] Y_OffsetMSB = new byte[] { 0x11 };
        byte[] Z_OffsetLSB = new byte[] { 0x12 };
        byte[] Z_OffsetMSB = new byte[] { 0x13 };
        //
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
        //newly added
        readonly I2CDevice.I2CTransaction[] magTrans;
        readonly I2CDevice.I2CTransaction[] tiltTrans;
        readonly I2CDevice.I2CTransaction[] op1Trans;
        readonly I2CDevice.I2CTransaction[] enterCalTrans;
        readonly I2CDevice.I2CTransaction[] exitCalTrans;
        static I2CDevice.I2CTransaction[] levelTrans;
        readonly I2CDevice.I2CTransaction[] uprightsidewaysTrans;
        readonly I2CDevice.I2CTransaction[] uprightflatfrontTrans;
        readonly I2CDevice.I2CTransaction[] runTrans;
        readonly I2CDevice.I2CTransaction[] standbyTrans;
        readonly I2CDevice.I2CTransaction[] resetTrans;
        readonly I2CDevice.I2CTransaction[] entersleepTrans;
        readonly I2CDevice.I2CTransaction[] exitsleepTrans;
        //EEProm Read/Write
        static I2CDevice.I2CTransaction[] writeEEPromTrans;
        readonly I2CDevice.I2CTransaction[] readEEPromTrans;
        //
        static I2CDevice.I2CTransaction[] x_offsetmsb;
        static I2CDevice.I2CTransaction[] x_offsetlsb;
        static I2CDevice.I2CTransaction[] y_offsetmsb;
        static I2CDevice.I2CTransaction[] writeEEPromTrans;
        static I2CDevice.I2CTransaction[] writeEEPromTrans;
        static I2CDevice.I2CTransaction[] writeEEPromTrans;


        /// <summary>
        /// Initializes a new instance of the <see cref="HMC6343"/> class.
        /// </summary>
        public HMC_6343()
        {
            Thread.Sleep(500);
            _deviceInterface = new I2CDevice(new I2CDevice.Configuration((ushort)HMC6343_ADDRESS, CLOCK_FREQ));
            /*
            levelTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(levelCommand) };
            _txnPostHeading = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(headingCommand) };
            _txnPostAccel = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(accelCommand) };
            _txnReadData = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(inBuffer) };
            byte[] opBuffer = new byte[1];
            */
           // writeEEPromTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(writeEEPromCommand) };
        
        
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
            int heading = (int)(((ushort)buffer[0]) << 8 | (ushort)buffer[1]);
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
            int i = _deviceInterface.Execute(_txnReadData, DELAY);
            _decodeHeading(inBuffer);

            _deviceInterface.Execute(_txnPostAccel, DELAY);
            Thread.Sleep(1);  // Give the compass the requested 1ms delay before reading
            _deviceInterface.Execute(_txnReadData, DELAY);
            _decodeAccel(inBuffer);

        }
        public void ZeroOut()
        {
            
        }
    }
}
