using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Robot_v100
{
    class HMC6343_alt
    {
        //Buffer for serial port
        const int bufferMax = 64;
        static byte[] buffer = new Byte[bufferMax];
        static int bufferLength = 0;

        //HMC6343 Address
        const ushort HMC6343_ADDRESS = 0x32 >> 1;   //7 address bits, read\write bit, Ack bit
        const int CLOCK_FREQ = 100;
        const int DELAY = 100;

        static byte[] headingCommand = new byte[] { 0x50 };             // Post Heading Data: HeadMSB, HeadLSB, PitchMSB, PitchLSB, RollMSB, RollLSB
        static byte[] accelCommand = new byte[] { 0x40 };               // Post Accel Data: AxMSB, AxLSB, AyMSB, AyLSB, AzMSB, AzLSB
        static byte[] magCommand = new byte[] { 0x45 };                 // Post Mag Data: MxMSB, MxLSB, MyMSB, MyLSB, MzMSB, MzLSB
        static byte[] tiltCommand = new byte[] { 0x55 };                // Post Tilt Data: PitchMSB, PitchLSB, RollMSB, RollLSB, TempMSB, TempLSB
        static byte[] op1Command = new byte[] { 0x65 };                 // Post OP Mode 1: Read the current value of OP Mode 1 register
        static byte[] enterCalCommand = new byte[] { 0x71 };            // Enter User Calibration Mode
        static byte[] exitCalCommand = new byte[] { 0x7E };             // Exit User Calibration Mode
        static byte[] levelCommand = new byte[] { 0x72 };               // Level Orientation (X=forward, +Z=up) (default)                     
        static byte[] uprightsidewaysCommand = new byte[] { 0x73 };     // Upright Sideways Orientation (X=forward, Y=up)   
        static byte[] uprightflatfrontCommand = new byte[] { 0x74 };    // Upright Flat Front Orientation (Z=forward, -X=up)
        static byte[] runCommand = new byte[] { 0x75 };                 // Enter Run Mode (from Standby Mode)
        static byte[] standbyCommand = new byte[] { 0x76 };             // Enter Standby Mode (from Run Mode)
        static byte[] resetCommand = new byte[] { 0x82 };               // Reset the Processor
        static byte[] entersleepCommand = new byte[] { 0x83 };          // Enter Sleep Mode (from Run Mode)
        static byte[] exitsleepCommand = new byte[] { 0x84 };           // Exit Sleep Mode (to Standby Mode)
        //
        static byte[] readEEPromCommand = new byte[] { 0xE1 };          //EEPROM Address  Response Bytes (Binary), Data (1 Byte) Read from EEPROM
        static byte[] writeEEPromCommand = new byte[] { 0xF1 };         //EEPROM Address  Argument 2 Byte (Binary), Data Write to EEPROM
        //
        static byte[] inBuffer = new byte[6]; // Six bytes, MSB followed by LSB


        static I2CDevice compass;   //The HMC6343 compass device
        //Read
        static I2CDevice.I2CTransaction[] readTrans;
        //Write
        static I2CDevice.I2CTransaction[] headingTrans;
        static I2CDevice.I2CTransaction[] accelTrans;
        static I2CDevice.I2CTransaction[] magTrans;
        static I2CDevice.I2CTransaction[] tiltTrans;
        static I2CDevice.I2CTransaction[] op1Trans;
        static I2CDevice.I2CTransaction[] enterCalTrans;
        static I2CDevice.I2CTransaction[] exitCalTrans;
        static I2CDevice.I2CTransaction[] levelTrans;
        static I2CDevice.I2CTransaction[] uprightsidewaysTrans;
        static I2CDevice.I2CTransaction[] uprightflatfrontTrans;
        static I2CDevice.I2CTransaction[] runTrans;
        static I2CDevice.I2CTransaction[] standbyTrans;
        static I2CDevice.I2CTransaction[] resetTrans;
        static I2CDevice.I2CTransaction[] entersleepTrans;
        static I2CDevice.I2CTransaction[] exitsleepTrans;
        //EEProm Read/Write
        static I2CDevice.I2CTransaction[] writeEEPromTrans;
        static I2CDevice.I2CTransaction[] readEEPromTrans;

        void started()
        {
            // HMC6343 Interface Commands/Responses

            // (SDA analog pin 4, SCL analog pin 5 ..For Netduino)
            // SDA Pin 8 (data), SCL Pin 9 (clock) ..For Fez Spider

            compass = new I2CDevice(new I2CDevice.Configuration((ushort)HMC6343_ADDRESS, CLOCK_FREQ));
            readTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(inBuffer) };

            //
            headingTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(headingCommand) };
            accelTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(accelCommand) };
            magTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(magCommand) };
            tiltTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(tiltCommand) };
            op1Trans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(op1Command) };
            enterCalTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(enterCalCommand) };
            exitCalTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(exitCalCommand) };
            levelTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(levelCommand) };
            uprightsidewaysTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(uprightsidewaysCommand) };
            uprightflatfrontTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(uprightflatfrontCommand) };
            runTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(runCommand) };
            standbyTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(standbyCommand) };
            resetTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(resetCommand) };
            entersleepTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(entersleepCommand) };
            exitsleepTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(exitsleepCommand) };
        }
    }
}
