/*****************************************************************************
*                                                                            *
* HID USB DRIVER - FLORIAN LEITNER                                           *
* Copyright 2007 - Florian Leitner | http://www.florian-leitner.de           *
* mail@florian-leitner.de                                                    *
*                                                                            *   
* This file is part of HID USB DRIVER.                                       *
*                                                                            *
*   HID USB DRIVER is free software; you can redistribute it and/or modify   *
*   it under the terms of the GNU General Public License 3.0 as published by *
*   the Free Software Foundation;                                            *
*   HID USB DRIVER is distributed in the hope that it will be useful,        *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of           *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the            *
*   GNU General Public License for more details.                             *
*   You should have received a copy of the GNU General Public License        *
*   along with this program.  If not, see <http://www.gnu.org/licenses/>.    *
*                                                                            *
******************************************************************************/
//---------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace USBHIDDRIVER.USB
{
	
	sealed class HidApiDeclarations
	{
		
		// API Declarations for communicating with HID-class devices.
		
		// ******************************************************************************
		// API constants
		// ******************************************************************************
		
		// from hidpi.h
		// Typedef enum defines a set of integer constants for HidP_Report_Type
		public const short HidP_Input = 0;
		public const short HidP_Output = 1;
		public const short HidP_Feature = 2;
		
		// ******************************************************************************
		// Structures and classes for API calls, listed alphabetically
		// ******************************************************************************
		
		[StructLayout(LayoutKind.Sequential)]
    public struct HIDD_ATTRIBUTES
		{
			public int Size;
			public short VendorID;
			public short ProductID;
			public short VersionNumber;
		}
		
		[StructLayout(LayoutKind.Sequential)]
    public struct HIDP_CAPS
		{
			public short Usage;
			public short UsagePage;
			public short InputReportByteLength;
			public short OutputReportByteLength;
			public short FeatureReportByteLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=17)]
      public short[] Reserved;
			public short NumberLinkCollectionNodes;
			public short NumberInputButtonCaps;
			public short NumberInputValueCaps;
			public short NumberInputDataIndices;
			public short NumberOutputButtonCaps;
			public short NumberOutputValueCaps;
			public short NumberOutputDataIndices;
			public short NumberFeatureButtonCaps;
			public short NumberFeatureValueCaps;
			public short NumberFeatureDataIndices;
			
		}
		
		// If IsRange is false, UsageMin is the Usage and UsageMax is unused.
		// If IsStringRange is false, StringMin is the string index and StringMax is unused.
		// If IsDesignatorRange is false, DesignatorMin is the designator index and DesignatorMax is unused.
		
		[StructLayout(LayoutKind.Sequential)]public struct HidP_Value_Caps
		{
			public short UsagePage;
			public byte ReportID;
			public int IsAlias;
			public short BitField;
			public short LinkCollection;
			public short LinkUsage;
			public short LinkUsagePage;
			public int IsRange;
			public int IsStringRange;
			public int IsDesignatorRange;
			public int IsAbsolute;
			public int HasNull;
			public byte Reserved;
			public short BitSize;
			public short ReportCount;
			public short Reserved2;
			public short Reserved3;
			public short Reserved4;
			public short Reserved5;
			public short Reserved6;
			public int LogicalMin;
			public int LogicalMax;
			public int PhysicalMin;
			public int PhysicalMax;
			public short UsageMin;
			public short UsageMax;
			public short StringMin;
			public short StringMax;
			public short DesignatorMin;
			public short DesignatorMax;
			public short DataIndexMin;
			public short DataIndexMax;
		}
		
		// ******************************************************************************
		// API functions, listed alphabetically
		// ******************************************************************************
		
		[DllImport("hid.dll")]
    static public extern bool HidD_FlushQueue(int HidDeviceObject);
		
		[DllImport("hid.dll")]
    static public extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);
		
		[DllImport("hid.dll")]
    static public extern int HidD_GetAttributes(int HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);
		
		[DllImport("hid.dll")]
    static public extern bool HidD_GetFeature(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);
		
		[DllImport("hid.dll")]
    static public extern bool HidD_GetInputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);
		
		[DllImport("hid.dll")]
    static public extern void HidD_GetHidGuid (ref System.Guid HidGuid);
		
		[DllImport("hid.dll")]
    static public extern bool HidD_GetNumInputBuffers(int HidDeviceObject, ref int NumberBuffers);
		
		[DllImport("hid.dll")]
    static public extern bool HidD_GetPreparsedData(int HidDeviceObject, ref IntPtr PreparsedData);
		
		[DllImport("hid.dll")]
    static public extern bool HidD_SetFeature(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);
		
		[DllImport("hid.dll")]
    static public extern bool HidD_SetNumInputBuffers(int HidDeviceObject, int NumberBuffers);
		
		[DllImport("hid.dll")]
    static public extern bool HidD_SetOutputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);
		
		[DllImport("hid.dll")]
    static public extern int HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);
		
		[DllImport("hid.dll")]
    static public extern int HidP_GetValueCaps(short ReportType, ref byte ValueCaps, ref short ValueCapsLength, IntPtr PreparsedData);
		
	}
	
}
