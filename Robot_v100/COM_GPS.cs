using System;
using Microsoft.SPOT;
using System.Text;
using System.IO.Ports;
using GHIElectronics.NETMF.System;

namespace COM_GPS
{
    public class COM_GPS
    {
        static readonly object _locker = new object();
        private SerialPort GPSHandle;
        private static string NMEA = "";
        static string GPRMC_sentence = "";
        static uint GPRM_count = 0;
        static uint GPRM_failed = 0;
        static bool OK = false;
        private DateTime Current_date;
        private double Current_Lat = 0, Current_Long = 0;
        private double Current_Course = 0;
        private double Current_Speed = 0;
        private double deg;
        private Int64 _fixTime = DateTime.Now.Ticks;

        public DateTime Date
        {
            get
            {
                return Current_date;
            }
        }

        public Int64 FixTime
        {
            get
            {
                return (DateTime.Now.Ticks - _fixTime) / 10000; //Returns mS
            }
        }

        public double Latitude
        {
            get
            {
                return Current_Lat;
            }
        }

        public double Longitude
        {
            get
            {
                return Current_Long;
            }
        }

        public bool Available
        {
            get { return OK; }
        }

        public uint Count
        {
            get { return GPRM_count; }
        }

        public uint Failed
        {
            get { return GPRM_failed; }
        }

        public double Cource
        {
            get { return Current_Course; }
        }

        public double Speed
        {
            get { return Current_Speed; }
        }

        public double DistanceTo(double WP_Lat, double WP_Long)
        {
            try
            {
                double tmp_Lat = Current_Lat;
                double tmp_Long = Current_Long;
                Trigonometry trig = new Trigonometry();
                double tempdistance = 0;
                double theta = tmp_Long - WP_Long;
                tempdistance = GHIElectronics.NETMF.System.MathEx.Sin(trig.DegreeToRadian(tmp_Lat)) *
                GHIElectronics.NETMF.System.MathEx.Sin(trig.DegreeToRadian(WP_Lat)) + GHIElectronics.NETMF.System.MathEx.Cos(trig.DegreeToRadian(Current_Lat)) *
                GHIElectronics.NETMF.System.MathEx.Cos(trig.DegreeToRadian(WP_Lat)) * GHIElectronics.NETMF.System.MathEx.Cos(trig.DegreeToRadian(theta));
                tempdistance = GHIElectronics.NETMF.System.MathEx.Acos(tempdistance);
                tempdistance = trig.RadianToDegree(tempdistance);
                tempdistance = tempdistance * 111189.57696;
                return tempdistance;
            }
            catch (Exception se)
            {
                Debug.Print(se.ToString());
                return -1;
            }
        }

        public double CourceTo(double WP_Lat, double WP_Long)
        {
            double tmp_Lat = Current_Lat;
            double tmp_Long = Current_Long;
            Trigonometry trig = new Trigonometry();
            double dlon = trig.DegreeToRadian(WP_Long - tmp_Long);
            tmp_Lat = trig.DegreeToRadian(tmp_Lat);
            WP_Lat = trig.DegreeToRadian(WP_Lat);
            double a1 = GHIElectronics.NETMF.System.MathEx.Sin(dlon) * GHIElectronics.NETMF.System.MathEx.Cos(WP_Lat);
            double a2 = GHIElectronics.NETMF.System.MathEx.Sin(tmp_Lat) * GHIElectronics.NETMF.System.MathEx.Cos(WP_Lat) * GHIElectronics.NETMF.System.MathEx.Cos(dlon);
            a2 = GHIElectronics.NETMF.System.MathEx.Cos(tmp_Lat) * GHIElectronics.NETMF.System.MathEx.Sin(WP_Lat) - a2;
            a2 = GHIElectronics.NETMF.System.MathEx.Atan2(a1, a2);
            if (a2 < 0.0)
            {
                a2 += GHIElectronics.NETMF.System.MathEx.PI * 2;
            }
            return trig.RadianToDegree(a2);
        }

        public COM_GPS(string comPort, int baudrate)
        {
            GPSHandle = new SerialPort(comPort, baudrate);
            GPSHandle.Open();
            GPSHandle.Flush();
            GPSHandle.DataReceived += new SerialDataReceivedEventHandler(GPS_DataReceived);
        }

        void GPS_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            if (e.EventType == SerialData.Chars)
            {
                try
                {
                    byte[] ReadBuffer = new byte[GPSHandle.BytesToRead];
                    GPSHandle.Read(ReadBuffer, 0, ReadBuffer.Length);
                    NMEA = NMEA + new String(Encoding.UTF8.GetChars(ReadBuffer));
                }
                catch
                {
                    Debug.Print("Error");
                }

                if (NMEA.IndexOf("$GPRMC") >= 0)
                {
                    if (NMEA.IndexOf("$GPRMC") > 0) NMEA = NMEA.Substring(NMEA.IndexOf("$GPRMC")); //fjern resten av stetningen
                    GPRMC_sentence = NMEA.Substring(NMEA.IndexOf("$GPRMC"));

                    if (GPRMC_sentence.IndexOf('\r') > 0)
                    {
                        NMEA = GPRMC_sentence.Substring(GPRMC_sentence.IndexOf('\r') + 1);
                        GPRMC_sentence = GPRMC_sentence.Substring(0, GPRMC_sentence.IndexOf('\r') + 1);

                    }
                }


                if (GPRMC_sentence.IndexOf("$GPRMC") == 0 && GPRMC_sentence.IndexOf("\r") > 0)
                {
                    GPRM_count++;
                    GPRMC_sentence = GPRMC_sentence.Substring(0, GPRMC_sentence.Length - 1); //remove \r
                    if (ValidChecksum(GPRMC_sentence))
                    {

                        string[] words = GPRMC_sentence.Split(',');
                        if (words[2].IndexOf("A") == 0) //We have a valid fix
                        {
                            OK = true;
                            Current_date = new DateTime(
                                (Convert.ToInt32(words[9].Substring(4)) + 2000),
                                (Convert.ToInt32(words[9].Substring(2, 2))),
                                (Convert.ToInt32(words[9].Substring(0, 2))),
                                (Convert.ToInt32(words[1].Substring(0, 2))),
                                (Convert.ToInt32(words[1].Substring(2, 2))),
                                (Convert.ToInt32(words[1].Substring(4, 2))));

                            // Calculate Latitude
                            Current_Lat = Convert.ToDouble(words[3]);
                            Current_Lat = Current_Lat / 100;
                            deg = System.Math.Floor(Current_Lat);
                            Current_Lat = (100 * (Current_Lat - deg)) / 60;
                            Current_Lat += deg;
                            if (words[4].IndexOf("S") == 0)
                            {
                                Current_Lat = 0.0 - Current_Lat;
                            };

                            // Calculate Longitude
                            Current_Long = Convert.ToDouble(words[5]);
                            Current_Long = Current_Long / 100;
                            deg = System.Math.Floor(Current_Long);
                            Current_Long = (100 * (Current_Long - deg)) / 60;
                            Current_Long += deg;
                            if (words[6].IndexOf("W") == 0)
                            {
                                Current_Long = 0.0 - Current_Long;
                            };

                            Current_Speed = Convert.ToDouble(words[7]);
                            Current_Course = Convert.ToDouble(words[8]);
                            _fixTime = DateTime.Now.Ticks;
                        }
                    }
                }
            }
        }

        private bool ValidChecksum(string GPS_sentence)
        {
            bool Status = false;
            int Checksum = 0;
            int NMEA_Checksum = -1;
            bool ChecksumComplete = false;
            string[] cs = GPRMC_sentence.Split('*');
            if (cs.Length == 2)
            {
                NMEA_Checksum = Convert.ToInt32(cs[1], 16);
                foreach (byte Character in GPS_sentence)
                {
                    switch (Character)
                    {
                        case 36: // Ignore the dollar sign
                            break;
                        case 10: // Ignore LF
                            break;
                        case 13: // Ignore CR
                            break;
                        case 42: // Stop processing before the asterisk
                            ChecksumComplete = true;
                            break;
                        default:
                            if (!ChecksumComplete)
                            {
                                if (Checksum == 0)
                                {
                                    Checksum = Character;
                                }
                                else
                                {
                                    Checksum = Checksum ^ Character;
                                }
                            }

                            break;
                    }
                }

                if (NMEA_Checksum == Checksum) Status = true;
            }
            if (Status == false) GPRM_failed++;
            return Status;
        }

        public class Trigonometry
        {
            public double DegreeToRadian(double angle)
            {
                return GHIElectronics.NETMF.System.MathEx.PI * angle / 180.0;
            }
            public double RadianToDegree(double angle)
            {
                return angle * (180.0 / GHIElectronics.NETMF.System.MathEx.PI);
            }
        }

    }
}