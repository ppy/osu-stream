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
using System.Collections;
using System.Threading;

namespace osum.Input.Sources.UsbHID.USB
{
    /// <summary>
    ///
    /// </summary>
    public class HIDUSBDevice : IDisposable
    {
        private bool disposed;

        private Thread usbThread;

        /*Variables --------------------------------------------------------------------*/
        private string vendorID; //Vendor ID of the Device
        private string productID; //Product ID of the Device
        private string devicePath; //device path
        private int deviceCount; //device count

        private bool connectionState; //Connection Status true: connected, false: disconnected

        public int byteCount; //Recieved Bytes
        //recieve Buffer (Each report is one Element)
        //this one was replaced by the receive Buffer in the interface
        //public static ArrayList receiveBuffer = new ArrayList();

        //USB Object
        private readonly USBSharp myUSB = new USBSharp();

        //thread for read operations
        protected Thread dataReadingThread;

        /*Functions --------------------------------------------------------------------*/

        //---#+************************************************************************
        //---NOTATION:
        //-  HIDUSBDevice(int vID, int pID)
        //-
        //--- DESCRIPTION:
        //--  constructor
        //--  tries to establish a connection to the device
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Initializes a new instance of the <see cref="HIDUSBDevice"/> class.
        /// And tries to establish a connection to the device.
        /// </summary>
        /// <param name="vID">The vendor ID of the USB device.</param>
        /// <param name="pID">The product ID of the USB device.</param>
        public HIDUSBDevice(string vID, string pID)
        {
            //set vid and pid
            setDeviceData(vID, pID);
            //try to establish connection
            connectDevice();
            //create Read Thread
            dataReadingThread = new Thread(readDataThread) { Priority = ThreadPriority.Highest, IsBackground = true };
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  bool connectDevice()
        //-
        //--- DESCRIPTION:
        //--  tries to establish a connection to the device
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Connects the device.
        /// </summary>
        /// <returns>true if connection is established</returns>
        public bool connectDevice()
        {
            //searchDevice
            searchDevice();
            //return connection state
            return getConnectionState();
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  bool searchDevice()
        //-
        //--- DESCRIPTION:
        //--  tries to find the device with specified vendorID and productID 
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Searches the device with soecified vendor and product id an connect to it.
        /// </summary>
        /// <returns></returns>
        private bool searchDevice()
        {
            //no device found yet
            bool deviceFound = false;
            deviceCount = 0;
            devicePath = string.Empty;

            myUSB.CT_HidGuid();
            myUSB.CT_SetupDiGetClassDevs();

            int result = -1;
            int resultb = -1;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;

            //search the device until you have found it or no more devices in list
            while (result != 0)
            {
                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);
                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, 0);
                size = requiredSize;
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetailx(ref requiredSize, size);

                //is this the device i want?
                string deviceID = vendorID + "&" + productID;
                if (myUSB.DevicePathName.IndexOf(deviceID) > 0)
                {
                    //yes it is

                    //store device information
                    deviceCount = device_count;
                    devicePath = myUSB.DevicePathName;
                    deviceFound = true;

                    //init device
                    myUSB.CT_SetupDiEnumDeviceInterfaces(deviceCount);

                    size = 0;
                    requiredSize = 0;

                    resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, size);
                    resultb = 0;
                    resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetailx(ref requiredSize, size);
                    resultb = 0;
                    //create HID Device Handel
                    resultb = myUSB.CT_CreateFile(devicePath);

                    //we have found our device so stop searching
                    break;
                }

                device_count++;
            }

            //set connection state
            setConnectionState(deviceFound);
            //return state
            return getConnectionState();
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  bool getDevices()
        //-
        //--- DESCRIPTION:
        //--  returns the number of devices with specified vendorID and productID 
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// returns the number of devices with specified vendorID and productID 
        /// </summary>
        /// <returns>returns the number of devices with specified vendorID and productID</returns>
        public int getDevice()
        {
            //no device found yet
            bool deviceFound = false;
            deviceCount = 0;
            devicePath = string.Empty;

            myUSB.CT_HidGuid();
            myUSB.CT_SetupDiGetClassDevs();

            int result = -1;
            int resultb = -1;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;
            int numberOfDevices = 0;
            //search the device until you have found it or no more devices in list
            while (result != 0)
            {
                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);
                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, 0);
                size = requiredSize;
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetailx(ref requiredSize, size);

                //is this the device i want?
                string deviceID = vendorID + "&" + productID;
                if (myUSB.DevicePathName.IndexOf(deviceID) > 0)
                {
                    numberOfDevices++;
                }

                device_count++;
            }

            return numberOfDevices;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  bool writeData(char[] cDataToWrite)
        //-
        //--- DESCRIPTION:
        //--  writes data to the device and returns true if no error occured
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="bDataToWrite">The b data to write.</param>
        /// <returns></returns>
        public bool writeData(byte[] bDataToWrite)
        {
            bool success = false;
            if (getConnectionState())
            {
                try
                {
                    //get output report length
                    int myPtrToPreparsedData = -1;
                    // myUSB.CT_HidD_GetPreparsedData(myUSB.HidHandle, ref myPtrToPreparsedData);
                    // int code = myUSB.CT_HidP_GetCaps(myPtrToPreparsedData);

                    int outputReportByteLength = 65;

                    int bytesSend = 0;
                    //if bWriteData is bigger then one report diveide into sevral reports
                    while (bytesSend < bDataToWrite.Length)
                    {
                        // Set the size of the Output report buffer.
                        // byte[] OutputReportBuffer = new byte[myUSB.myHIDP_CAPS.OutputReportByteLength - 1 + 1];
                        byte[] OutputReportBuffer = new byte[outputReportByteLength - 1 + 1];
                        // Store the report ID in the first byte of the buffer:
                        OutputReportBuffer[0] = 0;

                        // Store the report data following the report ID.
                        for (int i = 1; i < OutputReportBuffer.Length; i++)
                        {
                            if (bytesSend < bDataToWrite.Length)
                            {
                                OutputReportBuffer[i] = bDataToWrite[bytesSend];
                                bytesSend++;
                            }
                            else
                            {
                                OutputReportBuffer[i] = 0;
                            }
                        }

                        OutputReport myOutputReport = new OutputReport();
                        success = myOutputReport.Write(OutputReportBuffer, myUSB.HidHandle);
                    }
                }
                catch (AccessViolationException ex)
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  readDataThread()
        //-
        //--- DESCRIPTION:
        //--  ThreadMethod for reading Data
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        ///  ThreadMethod for reading Data
        /// </summary>
        public void readDataThread()
        {
            int receivedNull = 0;
            while (true)
            {
                int myPtrToPreparsedData = -1;

                if (myUSB.CT_HidD_GetPreparsedData(myUSB.HidHandle, ref myPtrToPreparsedData) != 0)
                {
                    int code = myUSB.CT_HidP_GetCaps(myPtrToPreparsedData);
                    int reportLength = myUSB.myHIDP_CAPS.InputReportByteLength;

                    while (true)
                    {
                        //read until thread is stopped
                        byte[] myRead = myUSB.CT_ReadFile(myUSB.myHIDP_CAPS.InputReportByteLength);
                        if (myRead != null)
                        {
                            byteCount += myRead.Length;
                            //lock (USBHIDDRIVER.USBInterface.usbBuffer.SyncRoot)
                            USBInterface.usbBuffer.Add(myRead);
                        }
                        else
                        {
                            //Recieved a lot of null bytes!
                            //mybe device disconnected?
                            if (receivedNull > 100)
                            {
                                receivedNull = 0;
                                Thread.Sleep(1);
                            }

                            receivedNull++;
                        }
                    }
                }
            }
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  readData()
        //-
        //--- DESCRIPTION:
        //--  handling of the read thread
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// controls the read thread
        /// </summary>
        public void readData()
        {
            if (dataReadingThread.ThreadState.ToString() == "Unstarted")
            {
                //start the thread
                dataReadingThread.Start();
                Thread.Sleep(0);
            }
            else if (dataReadingThread.ThreadState.ToString() == "Running")
            {
                //Stop the Thread
                dataReadingThread.Abort();
            }
            else
            {
                //create Read Thread
                dataReadingThread = new Thread(readDataThread) { Priority = ThreadPriority.Highest, IsBackground = true };
                ;
                //start the thread
                dataReadingThread.Start();
                Thread.Sleep(0);
            }
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  abortreadData()
        //-
        //--- DESCRIPTION:
        //--  handling of the read thread
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Aborts the read thread.
        /// </summary>
        public void abortreadData()
        {
            if (dataReadingThread.ThreadState.ToString() == "Running")
            {
                //Stop the Thread
                dataReadingThread.Abort();
            }
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  disconnectDevice()
        //-
        //--- DESCRIPTION:
        //--  disconnects the device and cleans up
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Disconnects the device.
        /// </summary>
        public void disconnectDevice()
        {
            usbThread.Abort();
            myUSB.CT_CloseHandle(myUSB.HidHandle);
        }

        /* GET AND SET Methods*/
        //---#+************************************************************************
        //---NOTATION:
        //-  setDeviceData(String vID, String pID)
        //-
        //--- DESCRIPTION:
        //--  set vendor and product ID
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Sets the device data.
        /// </summary>
        /// <param name="vID">The vendor ID.</param>
        /// <param name="pID">The product ID.</param>
        public void setDeviceData(string vID, string pID)
        {
            vendorID = vID;
            productID = pID;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  String getVendorID()
        //-
        //--- DESCRIPTION:
        //--  returns the vendor ID
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*

        /// <summary>
        /// Gets the vendor ID.
        /// </summary>
        /// <returns>the vendor ID</returns>
        public string getVendorID()
        {
            return vendorID;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  String getProductID()
        //-
        //--- DESCRIPTION:
        //--  returns the product ID
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Gets the product ID.
        /// </summary>
        /// <returns>the product ID</returns>
        public string getProductID()
        {
            return productID;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  setConnectionState(bool state)
        //-
        //--- DESCRIPTION:
        //--  set the connection state
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Sets the state of the connection.
        /// </summary>
        /// <param name="state">state</param>
        public void setConnectionState(bool state)
        {
            connectionState = state;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  bool getConnectionState()
        //-
        //--- DESCRIPTION:
        //--  returns the connection state
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Gets the state of the connection.
        /// </summary>
        /// <returns>true = connected; false = diconnected</returns>
        public bool getConnectionState()
        {
            return connectionState;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int getDeviceCount()
        //-
        //--- DESCRIPTION:
        //--  returns the device count
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Gets the device count.
        /// </summary>
        /// <returns></returns>
        public ArrayList getDevices()
        {
            ArrayList devices = new ArrayList();

            //no device found yet
            bool deviceFound = false;
            deviceCount = 0;
            devicePath = string.Empty;

            myUSB.CT_HidGuid();
            myUSB.CT_SetupDiGetClassDevs();

            int result = -1;
            int resultb = -1;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;
            int numberOfDevices = 0;
            //search the device until you have found it or no more devices in list

            while (result != 0)
            {
                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);
                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, 0);
                size = requiredSize;
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetailx(ref requiredSize, size);

                //is this the device i want?
                string deviceID = vendorID;
                if (myUSB.DevicePathName.IndexOf(deviceID) > 0)
                {
                    devices.Add(myUSB.DevicePathName);
                    numberOfDevices++;
                }

                device_count++;
            }

            return devices;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  getDevicePath()
        //-
        //--- DESCRIPTION:
        //--  returns the device path
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Gets the device path.
        /// </summary>
        /// <returns></returns>
        public string getDevicePath()
        {
            return devicePath;
        }

        internal abstract class HostReport
        {
            // For reports the host sends to the device.

            // Each report class defines a ProtectedWrite method for writing a type of report.

            protected abstract bool ProtectedWrite(int deviceHandle, byte[] reportBuffer);


            internal bool Write(byte[] reportBuffer, int deviceHandle)
            {
                bool Success = false;

                // Purpose    : Calls the overridden ProtectedWrite routine.
                //            : This method enables other classes to override ProtectedWrite
                //            : while limiting access as Friend.
                //            : (Directly declaring Write as Friend MustOverride causes the
                //            : compiler(warning) "Other languages may permit Friend
                //            : Overridable members to be overridden.")
                // Accepts    : reportBuffer - contains the report ID and report data.
                //            : deviceHandle - handle to the device.             '
                // Returns    : True on success. False on failure.

                try
                {
                    Success = ProtectedWrite(deviceHandle, reportBuffer);
                }
                catch (Exception ex)
                {
                }

                return Success;
            }
        }


        internal class OutputReport : HostReport
        {
            // For Output reports the host sends to the device.
            // Uses interrupt or control transfers depending on the device and OS.

            protected override bool ProtectedWrite(int hidHandle, byte[] outputReportBuffer)
            {
                // Purpose    : writes an Output report to the device.
                // Accepts    : HIDHandle - a handle to the device.
                //              OutputReportBuffer - contains the report ID and report to send.
                // Returns    : True on success. False on failure.

                int NumberOfBytesWritten = 0;
                int Result;
                bool Success = false;

                try
                {
                    // The host will use an interrupt transfer if the the HID has an interrupt OUT
                    // endpoint (requires USB 1.1 or later) AND the OS is NOT Windows 98 Gold (original version).
                    // Otherwise the the host will use a control transfer.
                    // The application doesn't have to know or care which type of transfer is used.

                    // ***
                    // API function: WriteFile
                    // Purpose: writes an Output report to the device.
                    // Accepts:
                    // A handle returned by CreateFile
                    // The output report byte length returned by HidP_GetCaps.
                    // An integer to hold the number of bytes written.
                    // Returns: True on success, False on failure.
                    // ***

                    Result = USBSharp.WriteFile(hidHandle, ref outputReportBuffer[0], outputReportBuffer.Length, ref NumberOfBytesWritten, 0);

                    Success = (Result == 0) ? false : true;
                }
                catch (Exception ex)
                {
                }

                return Success;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposeManagedResources)
        {
            if (!disposed)
            {
                if (disposeManagedResources)
                {
                    //only clear up managed stuff here
                }

                //clear up unmanaged stuff here
                if (myUSB.HidHandle != -1)
                {
                    myUSB.CT_CloseHandle(myUSB.HidHandle);
                }

                if (myUSB.hDevInfo != -1)
                {
                    myUSB.CT_SetupDiDestroyDeviceInfoList();
                }

                disposed = true;
            }
        }

        #endregion
    }
}