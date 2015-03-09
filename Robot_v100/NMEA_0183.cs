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

namespace NMEA_0183_GPS
{
    class GPS
    {

        /*
        As an example, a waypoint arrival alarm has the form:
        $GPAAM,A,A,0.10,N,WPTNME*32
         * 
        where:
        GP	Talker ID (GP for a GPS unit, GL for a GLONASS)
        AAM	Arrival alarm
        A	Arrival circle entered
        A	Perpendicular passed
        0.10	Circle radius
        N	Nautical miles
        WPTNME	Waypoint name
        *32	Checksum data
        */
        /* Token will point to the data between comma "'", returns the data in the order received 
         * 
        /*THE GPRMC order is: UTC, UTC status , Lat, N/S indicator, Lon, E/W indicator, speed, course, date, mode, checksum
        token = strtok_r(gps_buffer, search, &brkb); //Contains the header GPRMC, not used

        token = strtok_r(NULL, search, &brkb); //UTC Time, not used
        //time=	atol (token);
        token = strtok_r(NULL, search, &brkb); //Valid UTC data? maybe not used... 


        //Longitude in degrees, decimal minutes. (ej. 4750.1234 degrees decimal minutes = 47.835390 decimal degrees)
        //Where 47 are degrees and 50 the minutes and .1234 the decimals of the minutes.
        //To convert to decimal degrees, devide the minutes by 60 (including decimals), 
        //Example: "50.1234/60=.835390", then add the degrees, ex: "47+.835390 = 47.835390" decimal degrees
        token = strtok_r(NULL, search, &brkb); //Contains Latitude in degrees decimal minutes... 

        //taking only degrees, and minutes without decimals, 
        //strtol stop parsing till reach the decimal point "."	result example 4750, eliminates .1234
        temp = strtol (token, &pEnd, 10);

        //takes only the decimals of the minutes
        //result example 1234. 
        temp2 = strtol (pEnd + 1, NULL, 10);

        //joining degrees, minutes, and the decimals of minute, now without the point...
        //Before was 4750.1234, now the result example is 47501234...
        temp3 = (temp * 10000) + (temp2);


        //modulo to leave only the decimal minutes, eliminating only the degrees.. 
        //Before was 47501234, the result example is 501234.
        temp3 = temp3 % 1000000;


        //Dividing to obtain only the de degrees, before was 4750 
        //The result example is 47 (4750/100 = 47)
        temp /= 100;

        //Joining everything and converting to float variable... 
        //First i convert the decimal minutes to degrees decimals stored in "temp3", example: 501234/600000 =.835390
        //Then i add the degrees stored in "temp" and add the result from the first step, example 47+.835390 = 47.835390 
        //The result is stored in "lat" variable... 
        //lat = temp + ((float)temp3 / 600000);
        current_loc.lat	= (temp * t7) + ((temp3 *100) / 6);

        token = strtok_r(NULL, search, &brkb); //lat, north or south?
        //If the char is equal to S (south), multiply the result by -1.. 
        if(*token == 'S'){
        current_loc.lat *= -1;
        }

        //This the same procedure use in lat, but now for Lon....
        token = strtok_r(NULL, search, &brkb);
        temp = strtol (token,&pEnd, 10); 
        temp2 = strtol (pEnd + 1, NULL, 10); 
        temp3 = (temp * 10000) + (temp2);
        temp3 = temp3%1000000; 
        temp/= 100;
        //lon = temp+((float)temp3/600000);
        current_loc.lng		= (temp * t7) + ((temp3 * 100) / 6);

        token = strtok_r(NULL, search, &brkb); //lon, east or west?
        if(*token == 'W'){
        current_loc.lng *= -1;
        }

        token = strtok_r(NULL, search, &brkb); //Speed overground?
        ground_speed = atoi(token) * 100;

        token = strtok_r(NULL, search, &brkb); //Course?
        ground_course = atoi(token) * 100l;
						
        GPS_new_data = true;
         * */

        //-----------------------------------------------------
        //
        // Class	: 
        //
        // Purpose	: 
        //
        //-----------------------------------------------------
        public class GPSfix
        {

            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            public GPSfix()
            {

            }

            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            //$GPRMC,055626,V,4334.3306,S,17241.9391,E,000.3,155.4,03,,,N
            public GPSfix(string Digits, string Heading)
            {
                int dl = Digits.Length;

                if ((Heading == "W") || (Heading == "S"))
                    deg = "-" + Digits.Substring(0, dl - 7);
                else
                    deg = "+" + Digits.Substring(0, dl - 7);

                //Longitude in degrees, decimal minutes. (ej. 4750.1234 degrees decimal minutes = 47.835390 decimal degrees)
                //Where 47 are degrees and 50 the minutes and .1234 the decimals of the minutes.
                //To convert to decimal degrees, devide the minutes by 60 (including decimals), 
                //Example: "50.1234/60=.835390", then add the degrees, ex: "47+.835390 = 47.835390" decimal degrees
                min = Digits.Substring(dl - 7, dl - (dl - 7));

                double d = double.Parse(min);

                if ((Heading == "W") || (Heading == "S"))
                    d *= -1.0;

                decimal_deg = double.Parse(deg) + (d / 60.0);

                heading = Heading;

            }

            public double decimal_deg { get; set; }
            public string deg { get; set; }
            public string min { get; set; }
            public string heading { get; set; }
        };

        //-----------------------------------------------------
        //
        // Class	: 
        //
        // Purpose	: 
        //
        //-----------------------------------------------------
        public class NMEAdata
        {
            public bool NewData = false;
            public string Raw = string.Empty;

            /********************************************************************
        
           GGA - essential fix data which provide 3D location and accuracy data.

           $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47

            Where:
            GGA          Global Positioning System Fix Data
            123519       Fix taken at 12:35:19 UTC
            4807.038,N   Latitude 48 deg 07.038' N
            01131.000,E  Longitude 11 deg 31.000' E
                              Fix quality: 0 = invalid
                              1 = GPS fix (SPS)
                              2 = DGPS fix
                              3 = PPS fix
                              4 = Real Time Kinematic
                              5 = Float RTK
                              6 = estimated (dead reckoning) (2.3 feature)
                              7 = Manual input mode
                              8 = Simulation mode
            08           Number of satellites being tracked
            0.9          Horizontal dilution of position
            545.4,M      Altitude, Meters, above mean sea level
            46.9,M       Height of geoid (mean sea level) above WGS84 ellipsoid
            (empty field) time in seconds since last DGPS update
            (empty field) DGPS station ID number
            *47          the checksum data, always begins with *
        *
        ********************************************************************/

            //123519       Fix taken at 12:35:19 UTC
            public string UTCfixTaken;
            //4807.038,N   Latitude 48 deg 07.038' N
            public GPSfix Latitude;
            //01131.000,E  Longitude 11 deg 31.000' E
            public GPSfix Longitude;
            //1            Fix quality:   0 = invalid
            public string FixQuality;
            //                            1 = GPS fix (SPS)
            //                            2 = DGPS fix
            //                            3 = PPS fix
            //                            4 = Real Time Kinematic
            //                            5 = Float RTK
            //                            6 = estimated (dead reckoning) (2.3 feature)
            //                            7 = Manual input mode
            //                            8 = Simulation mode
            //08           Number of satellites being tracked
            public string Satellites;
            //545.4,M      Altitude, Meters, above mean sea level
            public string AltitudeMeters;
            //46.9,M       Height of geoid (mean sea level) above WGS84 ellipsoid
            public string HeightGeoidWGS84ellipsoid;




            /*********************************************************************
              RMC - NMEA has its own version of essential gps pvt (position, velocity, time) data. It is called RMC, The Recommended Minimum, which will look similar to:

              $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A

              Where:
                   RMC          Recommended Minimum sentence C
                   123519       Fix taken at 12:35:19 UTC
                   A            Status A=active or V=Void.
                   4807.038,N   Latitude 48 deg 07.038' N
                   01131.000,E  Longitude 11 deg 31.000' E
                   022.4        Speed over the ground in knots
                   084.4        Track angle in degrees True
                   230394       Date - 23rd of March 1994
                   003.1,W      Magnetic Variation
                   *6A          The checksum data, always begins with *
             *********************************************************************/

            //123519       Fix taken at 12:35:19 UTC


            //A            Status A=active or V=Void.
            public string Status;

            //Speed over the ground in knots
            public string Knots;

            //Track angle in degrees True
            public string TrackAngle;

            //Date - 23rd of March 1994
            public string Date;


            /*********************************************************************
            $GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39

            Where:
                 GSA      Satellite status
                 A        Auto selection of 2D or 3D fix (M = manual) 
                 3        3D fix - values include: 1 = no fix
                                                   2 = 2D fix
                                                   3 = 3D fix
                 04,05... PRNs of satellites used for fix (space for 12) 
                 2.5      PDOP (dilution of precision) 
                 1.3      Horizontal dilution of precision (HDOP) 
                 2.1      Vertical dilution of precision (VDOP)
                 *39      the checksum data, always begins with *
            *********************************************************************/

            public string AutoSelection;
            public string Fix3D;
            public string[] PRNs = new string[12];
            public string PDOP;
            public string HDOP;
            public string VDOP;

        };

        //-----------------------------------------------------
        //
        // Class	: 
        //
        // Purpose	: 
        //
        //-----------------------------------------------------
        public class NMEA_0183
        {
            private string TalkerID = "";
            private string MsgType = "";
            static NMEAdata data = new NMEAdata();


            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            internal NMEAdata Decode(string buffer)
            {
                NMEAdata data = new NMEAdata();


                string[] splitted = buffer.Split(',');

                if (splitted[0].Length < 4)
                    return data;

                TalkerID = splitted[0].Substring(1, 2);

                string mtf = splitted[0];
                if (mtf.Length == 4)
                    MsgType = splitted[0].Substring(3, 1);
                else if (mtf.Length == 5)
                    MsgType = splitted[0].Substring(3, 2);
                else if (mtf.Length == 6)
                    MsgType = splitted[0].Substring(3, 3);

                switch (MsgType)
                {
                    //GSA - Overall Satellite data
                    //GPGSA
                    case "GSA":

                        data = ProcessGSA(splitted);

                        break;

                    //GLL - Geographic Latitude and Longitude
                    //GPGLL
                    case "GLL":

                        break;

                    //GSV - Detailed Satellite data
                    //GPGSV
                    case "GSV":

                        break;

                    //RMC - recommended minimum data for gps
                    //GPRMC
                    case "RMC":

                        data = ProcessRMC(splitted);

                        break;

                    //GGA - Fix information
                    //GPGGA
                    case "GGA":

                        data = ProcessGGA(splitted);

                        break;

                    default:

                        break;
                };

                data.Raw = buffer;

                return data;
            }

            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            /*
            There are many sentences in the NMEA standard for all kinds of devices that may be used in a Marine environment. Some of the ones that have applicability to gps receivers are listed below: (all message start with GP.)

            AAM - Waypoint Arrival Alarm
            ALM - Almanac data
            APA - Auto Pilot A sentence
            APB - Auto Pilot B sentence
            BOD - Bearing Origin to Destination
            BWC - Bearing using Great Circle route
            DTM - Datum being used.
            GGA - Fix information
            GLL - Lat/Lon data
            GRS - GPS Range Residuals
            GSA - Overall Satellite data
            GST - GPS Pseudorange Noise Statistics
            GSV - Detailed Satellite data
            MSK - send control for a beacon receiver
            MSS - Beacon receiver status information.
            RMA - recommended Loran data
            RMB - recommended navigation data for gps
            RMC - recommended minimum data for gps
            RTE - route message
            TRF - Transit Fix Data
            STN - Multiple Data ID
            VBW - dual Ground / Water Spped
            VTG - Vector track an Speed over the Ground
            WCV - Waypoint closure velocity (Velocity Made Good)
            WPL - Waypoint Location information
            XTC - cross track error
            XTE - measured cross track error
            ZTG - Zulu (UTC) time and time to go (to destination)
            ZDA - Date and Time
             */


            /********************************************************************
             * 
                GGA - essential fix data which provide 3D location and accuracy data.

                $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47

                 Where:
                 GGA          Global Positioning System Fix Data
                 123519       Fix taken at 12:35:19 UTC
                 4807.038,N   Latitude 48 deg 07.038' N
                 01131.000,E  Longitude 11 deg 31.000' E
                                   Fix quality: 0 = invalid
                                   1 = GPS fix (SPS)
                                   2 = DGPS fix
                                   3 = PPS fix
                                   4 = Real Time Kinematic
                                   5 = Float RTK
                                   6 = estimated (dead reckoning) (2.3 feature)
                                   7 = Manual input mode
                                   8 = Simulation mode
                 08           Number of satellites being tracked
                 0.9          Horizontal dilution of position
                 545.4,M      Altitude, Meters, above mean sea level
                 46.9,M       Height of geoid (mean sea level) above WGS84 ellipsoid
                 (empty field) time in seconds since last DGPS update
                 (empty field) DGPS station ID number
                 *47          the checksum data, always begins with *
             *
             ********************************************************************/
            private NMEAdata ProcessGGA(string[] split)
            {
                data.NewData = false;

                if (split.Length < 15)
                    return data;

                //GOT: $GPGGA,070116,4334.3368,S,17241.9407,E,0,00,31.8,-00013.9,M,008.9,M,, Checksum=77
                //123519       Fix taken at 12:35:19 UTC
                data.UTCfixTaken = (split[1].Substring(0, 2) + ":" + split[1].Substring(2, 2) + ":" + split[1].Substring(4, 2));

                //GOT: $GPGGA,070116,4334.3368,S,17241.9407,E,0,00,31.8,-00013.9,M,008.9,M,, Checksum=77
                //4807.038,N   Latitude 48 deg 07.038' N
                data.Latitude = new GPSfix(split[2], split[3]);

                //GOT: $GPGGA,070116,4334.3368,S,17241.9407,E,0,00,31.8,-00013.9,M,008.9,M,, Checksum=77
                //01131.000,E  Longitude 11 deg 31.000' E
                data.Longitude = new GPSfix(split[4], split[5]);

                //GOT: $GPGGA,070116,4334.3368,S,17241.9407,E,0,00,31.8,-00013.9,M,008.9,M,, Checksum=77
                //Fix quality: 0 = invalid
                data.FixQuality = split[6];

                //GOT: $GPGGA,070116,4334.3368,S,17241.9407,E,0,00,31.8,-00013.9,M,008.9,M,, Checksum=77
                //08           Number of satellites being tracked
                data.Satellites = split[7];

                //GOT: $GPGGA,070116,4334.3368,S,17241.9407,E,0,00,31.8,-00013.9,M,008.9,M,, Checksum=77
                // 0.9          Horizontal dilution of position
                data.HDOP = split[8];

                //GOT: $GPGGA,070116,4334.3368,S,17241.9407,E,0,00,31.8,-00013.9,M,008.9,M,, Checksum=77
                // 545.4,M      Altitude, Meters, above mean sea level
                data.AltitudeMeters = split[9];

                //GOT: $GPGGA,070116,4334.3368,S,17241.9407,E,0,00,31.8,-00013.9,M,008.9,M,, Checksum=77
                // 46.9,M       Height of geoid (mean sea level) above WGS84 ellipsoid
                data.HeightGeoidWGS84ellipsoid = split[11];


                data.NewData = true;

                return data;
            }



            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            /********************************************************************
            RMC - NMEA has its own version of essential gps pvt (position, velocity, time) data. It is called RMC, The Recommended Minimum, which will look similar to:

            $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A

            Where:
                 RMC          Recommended Minimum sentence C
                 123519       Fix taken at 12:35:19 UTC
                 A            Status A=active or V=Void.
                 4807.038,N   Latitude 48 deg 07.038' N
                 01131.000,E  Longitude 11 deg 31.000' E
                 022.4        Speed over the ground in knots
                 084.4        Track angle in degrees True
                 230394       Date - 23rd of March 1994
                 003.1,W      Magnetic Variation
                 *6A          The checksum data, always begins with *
        
            $GPRMC,055626,V,4334.3306,S,17241.9391,E,000.3,155.4,03,,,N
        
           ********************************************************************/
            private NMEAdata ProcessRMC(string[] split)
            {
                data.NewData = false;

                if (split.Length < 13)
                    return data;

                //GOT: $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
                //123519       Fix taken at 12:35:19 UTC
                data.UTCfixTaken = (split[1].Substring(0, 2) + ":" + split[1].Substring(2, 2) + ":" + split[1].Substring(4, 2));

                data.Status = split[2].Substring(0, 1);


                //GOT: $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A

                data.Latitude = new GPSfix(split[3], split[4]);

                //GOT: $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
                data.Longitude = new GPSfix(split[5], split[6]);

                //GOT: $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
                data.Knots = split[7];

                //GOT: $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
                data.TrackAngle = split[8];

                //GOT: $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
                data.Date = (split[9].Substring(0, 2) + "-" + split[9].Substring(2, 2) + "-" + split[9].Substring(4, 2));

                data.NewData = true;

                return data;
            }

            //-----------------------------------------------------
            //
            // Function	: 
            //
            // Purpose	: 
            //
            //-----------------------------------------------------
            /*********************************************************************
            $GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39

            Where:
                 GSA      Satellite status
                 A        Auto selection of 2D or 3D fix (M = manual) 
                 3        3D fix - values include: 1 = no fix
                                                   2 = 2D fix
                                                   3 = 3D fix
                 04,05... PRNs of satellites used for fix (space for 12) 
                 2.5      PDOP (dilution of precision) 
                 1.3      Horizontal dilution of precision (HDOP) 
                 2.1      Vertical dilution of precision (VDOP)
                 *39      the checksum data, always begins with *
             *********************************************************************/
            private NMEAdata ProcessGSA(string[] split)
            {
                data.NewData = false;

                if (split.Length < 18)
                    return data;

                data.AutoSelection = split[1];
                data.Fix3D = split[2];

                for (int i = 0; i < 12; i++)
                    data.PRNs[i] = split[3 + i];

                data.PDOP = split[15];
                data.HDOP = split[16];
                data.VDOP = split[17];

                data.NewData = true;

                return data;
            }


        }//End of class
    }
}
