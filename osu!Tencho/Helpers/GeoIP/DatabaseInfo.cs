/**
 * DatabaseInfo.java
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

internal class DatabaseInfo {

    internal static int COUNTRY_EDITION = 1;
    internal static int REGION_EDITION_REV0 = 7;
    internal static int REGION_EDITION_REV1 = 3;
    internal static int CITY_EDITION_REV0 = 6;
    internal static int CITY_EDITION_REV1 = 2;
    internal static int ORG_EDITION = 5;
    internal static int ISP_EDITION = 4;
    internal static int PROXY_EDITION = 8;
    internal static int ASNUM_EDITION = 9;
    internal static int NETSPEED_EDITION = 10;

    //private static SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd");

    private String info;
   /**
     * Creates a new DatabaseInfo object given the database info String.
     * @param info
     */

    internal DatabaseInfo(String info) {
        this.info = info;
    }

    internal int getType() {
        if ((info == null) | (info == "")) {
            return COUNTRY_EDITION;
        }
        else {
            // Get the type code from the database info string and then
            // subtract 105 from the value to preserve compatability with
            // databases from April 2003 and earlier.
            return Convert.ToInt32(info.Substring(4, 7)) - 105;
        }
    }

    /**
     * Returns true if the database is the premium version.
     *
     * @return true if the premium version of the database.
     */
    internal bool isPremium() {
        return info.IndexOf("FREE") < 0;
    }

    /**
     * Returns the date of the database.
     *
     * @return the date of the database.
     */
    internal DateTime getDate() {
        for (int i=0; i<info.Length-9; i++) {
            if (Char.IsWhiteSpace(info[i]) == true) {
               String dateString = info.Substring(i+1, i+9);
                try {
                    //synchronized (formatter) {
                        return DateTime.ParseExact(dateString,"yyyyMMdd",null);
                    //}
                }
                catch (Exception e) {
		  Console.Write(e.Message);
		}
                break;
            }
        }
        return DateTime.Now;
    }

    internal String toString() {
        return info;
    }
}


