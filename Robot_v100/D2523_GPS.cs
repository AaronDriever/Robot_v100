using System;
using System.Text;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using System.IO;
using GHIElectronics.NETMF.IO;

namespace D2523_GPS
{
    using NMEA_0183_GPS;
    class D2523
    {

        public enum GPSsyncCHAR
        {
            SyncChar1 = 0xB5,
            SyncChar2 = 0x62,
        }
        public enum GPSclass1 // GPS Module Commands 
        {

            NAV = 0x01, // Position, Speed, Time, Acc, Heading, DOP, SVs used. Messages in the NAV Class output Navigation Data such as position, altitude and velocity in a number of formats. Additionally, status flags and accuracy figures are output.
            RXM = 0x02,
            INF = 0x04,
            ACK = 0x06,
            MON = 0x0A,
            TIM = 0xD
        }
        public enum GPSNAV
        {
            POSECEF = 0x01, // Position Solution in ECEF
            POSLLH = 0x02,  // Geodetic Position Solution
            STATUS = 0x03,  // Receiver Navigation Status
            DOP = 0x04,     // Dilution of precision
            SOL = 0x06,     // Navigation Solution Information
            VELECEF = 0x11, // Velocity Solution in ECEF
            VELNED = 0x12,  // Velocity Solution in NED
            TIMEGPS = 0x20, // GPS Time Solution
            TIMEUTC = 0x21, // UTC Time Solution
            CLOCK = 0x22,   // Clock Solution
            SVINFO = 0x30,
            SBAS = 0x32
        }
        public enum RXM
        {
            SVSI = 0x20     // SV Status Info
        }
        public enum TIM
        {
            TP = 0x01,      // Timepulse Timedata
            TM2 = 0x03,     // Time mark data
            SVIN = 0x04     // Survey-in data
        }
        public enum CFG
        {
            NAV5 = 0x24,
            NAV5 = 0x24,
            NAVX5 = 0x23,
            NAVX5 = 0x23,
            NMEA = 0x17,
            NMEA = 0x17,
            PRT = 0x00,
            PRT = 0x00,
            PRT = 0x00,
            PRT = 0x00,
            PRT = 0x00,
            PRT = 0x00,
            PRT = 0x00,
            RATE = 0x08,
            RATE = 0x08,
            RST = 0x04,
            RXM = 0x11,
            SBAS = 0x16,
            TMODE = 0x1D,
            TMODE = 0x1D,
            TP = 0x07,
            TP = 0x07,
            USB = 0x1B,
            USB = 0x1B,

        }
        //-----------------------------------------------------
        //
        // Class	: 
        //
        // Purpose	: 
        //
        //-----------------------------------------------------
        public class GPS
        {
            private int state = 0;
            string buffer = "";
            string chk = "";
            static SerialPort com;
            static NMEA_0183_GPS.GPS.NMEA_0183 NMEA = new NMEA_0183_GPS.GPS.NMEA_0183();
            public delegate void GPSmessageHandler(object o, NMEA_0183_GPS.GPS.NMEAdata dat);
            public static event GPSmessageHandler GPSmessage;
            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            private static void OnGPSmessage(NMEA_0183_GPS.GPS.NMEAdata data)
            {
                //if ( (GPSmessage != null) && (data.Latitude != null)  && (data.Longitude != null) )

                if (GPSmessage != null)
                    GPSmessage(data.GetType(), data);
            }
            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            public GPS(string port)
            {
                //Logger.Log("Init Serial");

                com = new SerialPort(port, 4800, Parity.None, 8, StopBits.One);
                com.ReadTimeout = 1000;
                com.DiscardInBuffer();
                com.Open();
                com.DataReceived += new SerialDataReceivedEventHandler(com_DataReceived);
            }
            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            void com_DataReceived(object Sender, SerialDataReceivedEventArgs e)
            {
                NMEA_0183_GPS.GPS.NMEAdata data = ExtractPackets(com);

                if (data.UTCfixTaken != null)
                    OnGPSmessage(data);

                //string data = ExtractData(com);

            }
            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            public NMEA_0183_GPS.GPS.NMEAdata ExtractPackets(SerialPort com)
            {
                NMEA_0183_GPS.GPS.NMEAdata dat = new NMEA_0183_GPS.GPS.NMEAdata();
                int bc = 1;// com.BytesToRead;
                byte[] bytes = new byte[bc];

                try
                {

                    do
                    {

                        com.Read(bytes, 0, bytes.Length);

                        char ch = (char)bytes[0];

                        if (ch == '$')
                        {
                            //Debug.Print("START SCENTENCE");
                            buffer = "";
                            chk = "";
                            state = 1;
                        }


                        if (state == 1)
                        {

                            //Found chcksum
                            if (ch == '*')
                            {
                                //Debug.Print("START CHECKSUM");
                                chk = "";
                                state = 2;
                            }
                            else
                                buffer += ch;

                        }
                        else if (state == 2)
                        {
                            //Get checksum
                            if ((ch == '\r') || (ch == '\n'))
                            {
                                //Debug.Print("END SCENTENCE");
                                state = 3;
                            }
                            else
                            {
                                chk += ch;

                                if (chk.Length > 2)
                                {
                                    //Debug.Print("CHECKSUM TOO LONG " + chk);
                                    state = 0;
                                }
                            }
                        }
                        else if (state == 3)
                        {
                            //Found EOL
                            if (ch == '\n')
                            {


                                //Calc checksum
                                byte localchk = 0;
                                foreach (char c in (buffer.Substring(1)).ToCharArray())
                                {
                                    localchk ^= (byte)c;
                                }

                                string lchex = ByteToHex(localchk);

                                if (chk == lchex)
                                {
                                    //Debug.Print("GOT: " + buffer + "\r\n RC=" + chk + " LC=" + lchex);

                                    dat = NMEA.Decode(buffer);


                                }

                                state = 0;

                                return dat;
                            }

                        }

                    } while (true);

                }
                catch { }

                return dat;

            }
            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            public string ByteToHex(byte bval)
            {
                string s = "";

                byte hn = (byte)((bval & 0xF0) >> 0x04);
                byte ln = (byte)(bval & 0x0F);

                s = ((hn == 10) ? "A" : (hn == 11) ? "B" : (hn == 12) ? "C" : (hn == 13) ? "D" : (hn == 14) ? "E" : (hn == 15) ? "F" : hn.ToString());
                s += ((ln == 10) ? "A" : (ln == 11) ? "B" : (ln == 12) ? "C" : (ln == 13) ? "D" : (ln == 14) ? "E" : (ln == 15) ? "F" : ln.ToString());

                return s;
            }
        }
    }
}
