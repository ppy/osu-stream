/**
 * LookupService.cs
 *
 * Copyright (C) 2008 MaxMind Inc.  All Rights Reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General internal License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */


using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

internal class LookupService
{
    private FileStream file = null;
    private DatabaseInfo databaseInfo = null;
    byte databaseType = Convert.ToByte(DatabaseInfo.COUNTRY_EDITION);
    int[] databaseSegments;
    int recordLength;
    int dboptions;
    byte[] dbbuffer;

    String licenseKey;
    int dnsService = 0;
    private static Country UNKNOWN_COUNTRY = new Country("--", "N/A");
    private static int COUNTRY_BEGIN = 16776960;
    private static int STATE_BEGIN = 16700000;
    private static int STRUCTURE_INFO_MAX_SIZE = 20;
    private static int DATABASE_INFO_MAX_SIZE = 100;
    private static int FULL_RECORD_LENGTH = 100;//???
    private static int SEGMENT_RECORD_LENGTH = 3;
    private static int STANDARD_RECORD_LENGTH = 3;
    private static int ORG_RECORD_LENGTH = 4;
    private static int MAX_RECORD_LENGTH = 4;
    private static int MAX_ORG_RECORD_LENGTH = 1000;//???
    private static int FIPS_RANGE = 360;
    private static int STATE_BEGIN_REV0 = 16700000;
    private static int STATE_BEGIN_REV1 = 16000000;
    private static int US_OFFSET = 1;
    private static int CANADA_OFFSET = 677;
    private static int WORLD_OFFSET = 1353;
    internal static int GEOIP_STANDARD = 0;
    internal static int GEOIP_MEMORY_CACHE = 1;
    internal static int GEOIP_UNKNOWN_SPEED = 0;
    internal static int GEOIP_DIALUP_SPEED = 1;
    internal static int GEOIP_CABLEDSL_SPEED = 2;
    internal static int GEOIP_CORPORATE_SPEED = 3;

    private static String[] countryCode = {
        "--","AP","EU","AD","AE","AF","AG","AI","AL","AM","AN","AO","AQ","AR",
        "AS","AT","AU","AW","AZ","BA","BB","BD","BE","BF","BG","BH","BI","BJ",
        "BM","BN","BO","BR","BS","BT","BV","BW","BY","BZ","CA","CC","CD","CF",
        "CG","CH","CI","CK","CL","CM","CN","CO","CR","CU","CV","CX","CY","CZ",
        "DE","DJ","DK","DM","DO","DZ","EC","EE","EG","EH","ER","ES","ET","FI",
        "FJ","FK","FM","FO","FR","FX","GA","GB","GD","GE","GF","GH","GI","GL",
        "GM","GN","GP","GQ","GR","GS","GT","GU","GW","GY","HK","HM","HN","HR",
        "HT","HU","ID","IE","IL","IN","IO","IQ","IR","IS","IT","JM","JO","JP",
        "KE","KG","KH","KI","KM","KN","KP","KR","KW","KY","KZ","LA","LB","LC",
        "LI","LK","LR","LS","LT","LU","LV","LY","MA","MC","MD","MG","MH","MK",
        "ML","MM","MN","MO","MP","MQ","MR","MS","MT","MU","MV","MW","MX","MY",
        "MZ","NA","NC","NE","NF","NG","NI","NL","NO","NP","NR","NU","NZ","OM",
        "PA","PE","PF","PG","PH","PK","PL","PM","PN","PR","PS","PT","PW","PY",
        "QA","RE","RO","RU","RW","SA","SB","SC","SD","SE","SG","SH","SI","SJ",
        "SK","SL","SM","SN","SO","SR","ST","SV","SY","SZ","TC","TD","TF","TG",
        "TH","TJ","TK","TM","TN","TO","TL","TR","TT","TV","TW","TZ","UA","UG",
        "UM","US","UY","UZ","VA","VC","VE","VG","VI","VN","VU","WF","WS","YE",
        "YT","RS","ZA","ZM","ME","ZW","A1","A2","O1","AX","GG","IM","JE","BL",
	"MF"};

    private static String[] countryName = {
            "N/A","Oceania","Europe","Andorra","UAE",
            "Afghanistan","Antigua","Anguilla","Albania","Armenia",
            "Netherlands Antilles","Angola","Antarctica","Argentina","American Samoa",
            "Austria","Australia","Aruba","Azerbaijan","Bosnia",
            "Barbados","Bangladesh","Belgium","Burkina Faso","Bulgaria","Bahrain",
            "Burundi","Benin","Bermuda","Brunei Darussalam","Bolivia","Brazil","Bahamas",
            "Bhutan","Bouvet Island","Botswana","Belarus","Belize","Canada",
            "Cocos Islands","Congo",
            "Central African Republic","Congo","Switzerland","Cote D'Ivoire",
            "Cook Islands","Chile","Cameroon","China","Colombia","Costa Rica","Cuba",
            "Cape Verde","Christmas Island","Cyprus","Czech Republic","Germany",
            "Djibouti","Denmark","Dominica","Dominican Republic","Algeria","Ecuador",
            "Estonia","Egypt","Western Sahara","Eritrea","Spain","Ethiopia","Finland",
            "Fiji","Falkland Islands","Micronesia, Federated States of",
            "Faroe Islands","France","France, Metropolitan","Gabon","United Kingdom",
            "Grenada","Georgia","French Guiana","Ghana","Gibraltar","Greenland","Gambia",
            "Guinea","Guadeloupe","Equatorial Guinea","Greece",
            "South Georgia","Guatemala","Guam",
            "Guinea-Bissau","Guyana","Hong Kong","Heard Island",
            "Honduras","Croatia","Haiti","Hungary","Indonesia","Ireland","Israel","India",
            "British Indian Ocean Territory","Iraq","Iran, Islamic Republic of",
            "Iceland","Italy","Jamaica","Jordan","Japan","Kenya","Kyrgyzstan","Cambodia",
            "Kiribati","Comoros","St. Kitts and Nevis",
            "Korea, Democratic People's Republic of","Korea","Kuwait",
            "Cayman Islands","Kazakhstan","Lao","Lebanon",
            "St. Lucia","Liechtenstein","Sri Lanka","Liberia","Lesotho","Lithuania",
            "Luxembourg","Latvia","Libyan Arab Jamahiriya","Morocco","Monaco",
            "Moldova, Republic of","Madagascar","Marshall Islands",
            "Macedonia, the Former Yugoslav Republic of","Mali","Myanmar","Mongolia",
            "Macau","Northern Mariana Islands","Martinique","Mauritania","Montserrat",
            "Malta","Mauritius","Maldives","Malawi","Mexico","Malaysia","Mozambique",
            "Namibia","New Caledonia","Niger","Norfolk Island","Nigeria","Nicaragua",
            "Netherlands","Norway","Nepal","Nauru","Niue","New Zealand","Oman","Panama",
            "Peru","French Polynesia","Papua New Guinea","Philippines","Pakistan",
            "Poland","St. Pierre","Pitcairn","Puerto Rico","" +
            "Palestinian Territory","Portugal","Palau","Paraguay","Qatar",
            "Reunion","Romania","Russian Federation","Rwanda","Saudi Arabia",
            "Solomon Islands","Seychelles","Sudan","Sweden","Singapore","St. Helena",
            "Slovenia","Svalbard and Jan Mayen","Slovakia","Sierra Leone","San Marino",
            "Senegal","Somalia","Suriname","Sao Tome and Principe","El Salvador",
            "Syrian Arab Republic","Swaziland","Turks and Caicos Islands","Chad",
            "French Southern Territories","Togo","Thailand","Tajikistan","Tokelau",
            "Turkmenistan","Tunisia","Tonga","Timor-Leste","Turkey","Trinidad and Tobago",
            "Tuvalu","Taiwan","Tanzania","Ukraine","Uganda",
            "US (Island)","United States","Uruguay","Uzbekistan",
            "Holy See","St. Vincent",
            "Venezuela","Virgin Islands, British","Virgin Islands, U.S.","Vietnam",
            "Vanuatu","Wallis and Futuna","Samoa","Yemen","Mayotte","Serbia",
            "South Africa","Zambia","Montenegro","Zimbabwe","Unknown",
            "Satellite Provider","Other",
	    "Aland Islands","Guernsey","Isle of Man","Jersey","St. Barthelemy",
	    "Saint Martin"};

    bool initialised;

    internal LookupService(String databaseFile, int options)
    {
        if (!File.Exists(databaseFile))
        {
            Console.Write("cannot open file " + databaseFile + "\n");
            return;
        }

        try
        {
            this.file = new FileStream(databaseFile, FileMode.Open, FileAccess.Read);
            dboptions = options;
            init();
        }
        catch (System.SystemException)
        {
        }

        initialised = true;
    }

    internal LookupService(String databaseFile)
        : this(databaseFile, GEOIP_STANDARD)
    {
    }

    private void init()
    {
        int i, j;
        byte[] delim = new byte[3];
        byte[] buf = new byte[SEGMENT_RECORD_LENGTH];
        databaseType = (byte)DatabaseInfo.COUNTRY_EDITION;
        recordLength = STANDARD_RECORD_LENGTH;
        //file.Seek(file.Length() - 3,SeekOrigin.Begin);
        file.Seek(-3, SeekOrigin.End);
        for (i = 0; i < STRUCTURE_INFO_MAX_SIZE; i++)
        {
            file.Read(delim, 0, 3);
            if (delim[0] == 255 && delim[1] == 255 && delim[2] == 255)
            {
                databaseType = Convert.ToByte(file.ReadByte());
                if (databaseType >= 106)
                {
                    // Backward compatibility with databases from April 2003 and earlier
                    databaseType -= 105;
                }
                // Determine the database type.
                if (databaseType == DatabaseInfo.REGION_EDITION_REV0)
                {
                    databaseSegments = new int[1];
                    databaseSegments[0] = STATE_BEGIN_REV0;
                    recordLength = STANDARD_RECORD_LENGTH;
                }
                else if (databaseType == DatabaseInfo.REGION_EDITION_REV1)
                {
                    databaseSegments = new int[1];
                    databaseSegments[0] = STATE_BEGIN_REV1;
                    recordLength = STANDARD_RECORD_LENGTH;
                }
                else if (databaseType == DatabaseInfo.CITY_EDITION_REV0 ||
                      databaseType == DatabaseInfo.CITY_EDITION_REV1 ||
                      databaseType == DatabaseInfo.ORG_EDITION ||
                      databaseType == DatabaseInfo.ISP_EDITION ||
                      databaseType == DatabaseInfo.ASNUM_EDITION)
                {
                    databaseSegments = new int[1];
                    databaseSegments[0] = 0;
                    if (databaseType == DatabaseInfo.CITY_EDITION_REV0 ||
                        databaseType == DatabaseInfo.CITY_EDITION_REV1)
                    {
                        recordLength = STANDARD_RECORD_LENGTH;
                    }
                    else
                    {
                        recordLength = ORG_RECORD_LENGTH;
                    }
                    file.Read(buf, 0, SEGMENT_RECORD_LENGTH);
                    for (j = 0; j < SEGMENT_RECORD_LENGTH; j++)
                    {
                        databaseSegments[0] += (unsignedByteToInt(buf[j]) << (j * 8));
                    }
                }
                break;
            }
            else
            {
                //file.Seek(file.getFilePointer() - 4);
                file.Seek(-4, SeekOrigin.Current);
                //file.Seek(file.position-4,SeekOrigin.Begin);
            }
        }
        if ((databaseType == DatabaseInfo.COUNTRY_EDITION) |
            (databaseType == DatabaseInfo.PROXY_EDITION) |
            (databaseType == DatabaseInfo.NETSPEED_EDITION))
        {
            databaseSegments = new int[1];
            databaseSegments[0] = COUNTRY_BEGIN;
            recordLength = STANDARD_RECORD_LENGTH;
        }
        if ((dboptions & GEOIP_MEMORY_CACHE) == 1)
        {
            int l = (int)file.Length;
            dbbuffer = new byte[l];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(dbbuffer, 0, l);
        }
    }
    internal void close()
    {
        try
        {
            file.Close();
            file = null;
        }
        catch (Exception) { }
    }
    internal Country getCountry(IPAddress ipAddress)
    {
        return getCountry(bytestoLong(ipAddress.GetAddressBytes()));
    }
    internal Country getCountry(String ipAddress)
    {
        IPAddress addr;
        try
        {
            addr = IPAddress.Parse(ipAddress);
        }
        //catch (UnknownHostException e) {
        catch (Exception e)
        {
            Console.Write(e.Message);
            return UNKNOWN_COUNTRY;
        }
        //  return getCountry(bytestoLong(addr.GetAddressBytes()));
        return getCountry(bytestoLong(addr.GetAddressBytes()));
    }
    internal Country getCountry(long ipAddress)
    {
        if (file == null)
        {
            //throw new IllegalStateException("Database has been closed.");
            throw new Exception("Database has been closed.");
        }
        if ((databaseType == DatabaseInfo.CITY_EDITION_REV1) |
        (databaseType == DatabaseInfo.CITY_EDITION_REV0))
        {
            Location l = getLocation(ipAddress);
            if (l == null)
            {
                return UNKNOWN_COUNTRY;
            }
            else
            {
                return new Country(l.countryCode, l.countryName);
            }
        }
        else
        {
            int ret = SeekCountry(ipAddress) - COUNTRY_BEGIN;
            if (ret == 0)
            {
                return UNKNOWN_COUNTRY;
            }
            else
            {
                return new Country(countryCode[ret], countryName[ret]);
            }
        }
    }

    internal int getID(String ipAddress)
    {
        IPAddress addr;
        try
        {
            addr = IPAddress.Parse(ipAddress);
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
            return 0;
        }
        return getID(bytestoLong(addr.GetAddressBytes()));
    }

    internal int getID(IPAddress ipAddress)
    {

        return getID(bytestoLong(ipAddress.GetAddressBytes()));
    }

    internal int getID(long ipAddress)
    {
        if (file == null)
        {
            throw new Exception("Database has been closed.");
        }
        int ret = SeekCountry(ipAddress) - databaseSegments[0];
        return ret;
    }
    internal DatabaseInfo getDatabaseInfo()
    {
        if (databaseInfo != null)
        {
            return databaseInfo;
        }
        try
        {
            // Synchronize since we're accessing the database file.
            lock (this)
            {
                bool hasStructureInfo = false;
                byte[] delim = new byte[3];
                // Advance to part of file where database info is stored.
                file.Seek(-3, SeekOrigin.End);
                for (int i = 0; i < STRUCTURE_INFO_MAX_SIZE; i++)
                {
                    file.Read(delim, 0, 3);
                    if (delim[0] == 255 && delim[1] == 255 && delim[2] == 255)
                    {
                        hasStructureInfo = true;
                        break;
                    }
                }
                if (hasStructureInfo)
                {
                    file.Seek(-3, SeekOrigin.Current);
                }
                else
                {
                    // No structure info, must be pre Sep 2002 database, go back to end.
                    file.Seek(-3, SeekOrigin.End);
                }
                // Find the database info string.
                for (int i = 0; i < DATABASE_INFO_MAX_SIZE; i++)
                {
                    file.Read(delim, 0, 3);
                    if (delim[0] == 0 && delim[1] == 0 && delim[2] == 0)
                    {
                        byte[] dbInfo = new byte[i];
                        char[] dbInfo2 = new char[i];
                        file.Read(dbInfo, 0, i);
                        for (int a0 = 0; a0 < i; a0++)
                        {
                            dbInfo2[a0] = Convert.ToChar(dbInfo[a0]);
                        }
                        // Create the database info object using the string.
                        this.databaseInfo = new DatabaseInfo(new String(dbInfo2));
                        return databaseInfo;
                    }
                    file.Seek(-4, SeekOrigin.Current);
                }
            }
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
            //e.printStackTrace();
        }
        return new DatabaseInfo("");
    }
    internal Region getRegion(IPAddress ipAddress)
    {
        return getRegion(bytestoLong(ipAddress.GetAddressBytes()));
    }
    internal Region getRegion(String str)
    {
        IPAddress addr;
        try
        {
            addr = IPAddress.Parse(str);
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
            return null;
        }

        return getRegion(bytestoLong(addr.GetAddressBytes()));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal Region getRegion(long ipnum)
    {
        Region record = new Region();
        int seek_region = 0;
        if (databaseType == DatabaseInfo.REGION_EDITION_REV0)
        {
            seek_region = SeekCountry(ipnum) - STATE_BEGIN_REV0;
            char[] ch = new char[2];
            if (seek_region >= 1000)
            {
                record.countryCode = "US";
                record.countryName = "United States";
                ch[0] = (char)(((seek_region - 1000) / 26) + 65);
                ch[1] = (char)(((seek_region - 1000) % 26) + 65);
                record.region = new String(ch);
            }
            else
            {
                record.countryCode = countryCode[seek_region];
                record.countryName = countryName[seek_region];
                record.region = "";
            }
        }
        else if (databaseType == DatabaseInfo.REGION_EDITION_REV1)
        {
            seek_region = SeekCountry(ipnum) - STATE_BEGIN_REV1;
            char[] ch = new char[2];
            if (seek_region < US_OFFSET)
            {
                record.countryCode = "";
                record.countryName = "";
                record.region = "";
            }
            else if (seek_region < CANADA_OFFSET)
            {
                record.countryCode = "US";
                record.countryName = "United States";
                ch[0] = (char)(((seek_region - US_OFFSET) / 26) + 65);
                ch[1] = (char)(((seek_region - US_OFFSET) % 26) + 65);
                record.region = new String(ch);
            }
            else if (seek_region < WORLD_OFFSET)
            {
                record.countryCode = "CA";
                record.countryName = "Canada";
                ch[0] = (char)(((seek_region - CANADA_OFFSET) / 26) + 65);
                ch[1] = (char)(((seek_region - CANADA_OFFSET) % 26) + 65);
                record.region = new String(ch);
            }
            else
            {
                record.countryCode = countryCode[(seek_region - WORLD_OFFSET) / FIPS_RANGE];
                record.countryName = countryName[(seek_region - WORLD_OFFSET) / FIPS_RANGE];
                record.region = "";
            }
        }
        return record;
    }
    internal Location getLocation(IPAddress addr)
    {
        if (!initialised) return null;
        return getLocation(bytestoLong(addr.GetAddressBytes()));
    }
    internal Location getLocation(String str)
    {
        if (!initialised) return null;
        IPAddress addr;
        try
        {
            addr = IPAddress.Parse(str);
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
            return null;
        }

        return getLocation(bytestoLong(addr.GetAddressBytes()));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal Location getLocation(long ipnum)
    {
        if (!initialised) return null;
        int record_pointer;
        byte[] record_buf = new byte[FULL_RECORD_LENGTH];
        char[] record_buf2 = new char[FULL_RECORD_LENGTH];
        int record_buf_offset = 0;
        Location record = new Location();
        int str_length = 0;
        int j, Seek_country;
        double latitude = 0, longitude = 0;

        try
        {
            Seek_country = SeekCountry(ipnum);
            if (Seek_country == databaseSegments[0])
            {
                return null;
            }
            record_pointer = Seek_country + ((2 * recordLength - 1) * databaseSegments[0]);
            if ((dboptions & GEOIP_MEMORY_CACHE) == 1)
            {
                Array.Copy(dbbuffer, record_pointer, record_buf, 0, Math.Min(dbbuffer.Length - record_pointer, FULL_RECORD_LENGTH));
            }
            else
            {
                file.Seek(record_pointer, SeekOrigin.Begin);
                file.Read(record_buf, 0, FULL_RECORD_LENGTH);
            }
            for (int a0 = 0; a0 < FULL_RECORD_LENGTH; a0++)
            {
                record_buf2[a0] = Convert.ToChar(record_buf[a0]);
            }
            // get country
            record.countryCode = countryCode[unsignedByteToInt(record_buf[0])];
            record.countryName = countryName[unsignedByteToInt(record_buf[0])];
            record.countryNum = record_buf[0];
            record_buf_offset++;

            // get region
            while (record_buf[record_buf_offset + str_length] != '\0')
                str_length++;
            if (str_length > 0)
            {
                record.region = new String(record_buf2, record_buf_offset, str_length);
            }
            record_buf_offset += str_length + 1;
            str_length = 0;

            // get region_name
            record.regionName = RegionName.getRegionName(record.countryCode, record.region);

            // get city
            while (record_buf[record_buf_offset + str_length] != '\0')
                str_length++;
            if (str_length > 0)
            {
                record.city = new String(record_buf2, record_buf_offset, str_length);
            }
            record_buf_offset += (str_length + 1);
            str_length = 0;

            // get postal code
            while (record_buf[record_buf_offset + str_length] != '\0')
                str_length++;
            if (str_length > 0)
            {
                record.postalCode = new String(record_buf2, record_buf_offset, str_length);
            }
            record_buf_offset += (str_length + 1);

            // get latitude
            for (j = 0; j < 3; j++)
                latitude += (unsignedByteToInt(record_buf[record_buf_offset + j]) << (j * 8));
            record.latitude = (float)latitude / 10000 - 180;
            record_buf_offset += 3;

            // get longitude
            for (j = 0; j < 3; j++)
                longitude += (unsignedByteToInt(record_buf[record_buf_offset + j]) << (j * 8));
            record.longitude = (float)longitude / 10000 - 180;

            record.metro_code = record.dma_code = 0;
            record.area_code = 0;
            if (databaseType == DatabaseInfo.CITY_EDITION_REV1)
            {
                // get metro_code
                int metroarea_combo = 0;
                if (record.countryCode == "US")
                {
                    record_buf_offset += 3;
                    for (j = 0; j < 3; j++)
                        metroarea_combo += (unsignedByteToInt(record_buf[record_buf_offset + j]) << (j * 8));
                    record.metro_code = record.dma_code = metroarea_combo / 1000;
                    record.area_code = metroarea_combo % 1000;
                }
            }
        }
        catch (IOException)
        {
            Console.Write("IO Exception while seting up segments");
        }
        return record;
    }
    internal String getOrg(IPAddress addr)
    {
        return getOrg(bytestoLong(addr.GetAddressBytes()));
    }
    internal String getOrg(String str)
    {
        IPAddress addr;
        try
        {
            addr = IPAddress.Parse(str);
        }
        //catch (UnknownHostException e) {
        catch (Exception e)
        {
            Console.Write(e.Message);
            return null;
        }
        return getOrg(bytestoLong(addr.GetAddressBytes()));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal String getOrg(long ipnum)
    {
        int Seek_org;
        int record_pointer;
        int str_length = 0;
        byte[] buf = new byte[MAX_ORG_RECORD_LENGTH];
        char[] buf2 = new char[MAX_ORG_RECORD_LENGTH];
        String org_buf;

        try
        {
            Seek_org = SeekCountry(ipnum);
            if (Seek_org == databaseSegments[0])
            {
                return null;
            }

            record_pointer = Seek_org + (2 * recordLength - 1) * databaseSegments[0];
            if ((dboptions & GEOIP_MEMORY_CACHE) == 1)
            {
                Array.Copy(dbbuffer, record_pointer, buf, 0, Math.Min(dbbuffer.Length - record_pointer, MAX_ORG_RECORD_LENGTH));
            }
            else
            {
                file.Seek(record_pointer, SeekOrigin.Begin);
                file.Read(buf, 0, MAX_ORG_RECORD_LENGTH);
            }
            while (buf[str_length] != 0)
            {
                buf2[str_length] = Convert.ToChar(buf[str_length]);
                str_length++;
            }
            buf2[str_length] = '\0';
            org_buf = new String(buf2, 0, str_length);
            return org_buf;
        }
        catch (IOException)
        {
            Console.Write("IO Exception");
            return null;
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private int SeekCountry(long ipAddress)
    {
        byte[] buf = new byte[2 * MAX_RECORD_LENGTH];
        int[] x = new int[2];
        int offset = 0;
        for (int depth = 31; depth >= 0; depth--)
        {
            try
            {
                if ((dboptions & GEOIP_MEMORY_CACHE) == 1)
                {
                    for (int i = 0; i < (2 * MAX_RECORD_LENGTH); i++)
                    {
                        buf[i] = dbbuffer[i + (2 * recordLength * offset)];
                    }
                }
                else
                {
                    file.Seek(2 * recordLength * offset, SeekOrigin.Begin);
                    file.Read(buf, 0, 2 * MAX_RECORD_LENGTH);
                }
            }
            catch (IOException)
            {
                Console.Write("IO Exception");
            }
            for (int i = 0; i < 2; i++)
            {
                x[i] = 0;
                for (int j = 0; j < recordLength; j++)
                {
                    int y = buf[(i * recordLength) + j];
                    if (y < 0)
                    {
                        y += 256;
                    }
                    x[i] += (y << (j * 8));
                }
            }

            if ((ipAddress & (1 << depth)) > 0)
            {
                if (x[1] >= databaseSegments[0])
                {
                    return x[1];
                }
                offset = x[1];
            }
            else
            {
                if (x[0] >= databaseSegments[0])
                {
                    return x[0];
                }
                offset = x[0];
            }
        }

        // shouldn't reach here
        Console.Write("Error Seeking country while Seeking " + ipAddress);
        return 0;

    }
    private static long swapbytes(long ipAddress)
    {
        return (((ipAddress >> 0) & 255) << 24) | (((ipAddress >> 8) & 255) << 16)
      | (((ipAddress >> 16) & 255) << 8) | (((ipAddress >> 24) & 255) << 0);
    }
    private static long bytestoLong(byte[] address)
    {
        long ipnum = 0;
        for (int i = 0; i < 4; ++i)
        {
            long y = address[i];
            if (y < 0)
            {
                y += 256;
            }
            ipnum += y << ((3 - i) * 8);
        }
        return ipnum;
    }
    private static int unsignedByteToInt(byte b)
    {
        return (int)b & 0xFF;
    }

}
