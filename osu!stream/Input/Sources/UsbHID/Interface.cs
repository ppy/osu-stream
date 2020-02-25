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
using System.Threading;
using osum.Input.Sources.UsbHID.USB;

namespace osum.Input.Sources.UsbHID
{
    /// <summary>
    /// Interface for the HID USB Driver.
    /// </summary>
    public class USBInterface
    {
        private readonly string usbVID;
        private readonly string usbPID;
        private bool isConnected;

        private readonly HIDUSBDevice usbdevice;

        //USB LIST BUFFER
        /// <summary>
        /// Buffer for incomming data.
        /// </summary>
        public static ListWithEvent usbBuffer = new ListWithEvent();

        /// <summary>
        /// Initializes a new instance of the <see cref="USBInterface"/> class.
        /// </summary>
        /// <param name="vid">The vendor id of the USB device (e.g. vid_06ba)</param>
        /// <param name="pid">The product id of the USB device (e.g. pid_ffff)</param>
        public USBInterface(string vid, string pid)
        {
            usbVID = vid;
            usbPID = pid;
            usbdevice = new HIDUSBDevice(usbVID, usbPID);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="USBInterface"/> class.
        /// </summary>
        /// <param name="vid">The vendor id of the USB device (e.g. vid_06ba).</param>
        public USBInterface(string vid)
        {
            usbVID = vid;
            usbdevice = new HIDUSBDevice(usbVID, "");
        }

        /// <summary>
        /// Establishes a connection to the USB device. 
        /// You can only establish a connection to a device if you have used the construct with vendor AND product id. 
        /// Otherwise it will connect to a device which has the same vendor id is specified, 
        /// this means if more than one device with these vendor id is plugged in, 
        /// you can't be determine to which one you will connect. 
        /// </summary>
        /// <returns>false if an error occures</returns>
        public bool Connect()
        {
            isConnected = usbdevice.connectDevice();
            return isConnected;
        }

        /// <summary>
        /// Disconnects the device
        /// </summary>
        public void Disconnect()
        {
            if (isConnected)
            {
                usbdevice.disconnectDevice();
            }
        }

        /// <summary>
        /// Returns a list of devices with the vendor id (or vendor and product id) 
        /// specified in the constructor.
        /// This function is needed if you want to know how many (and which) devices with the specified
        /// vendor id are plugged in.
        /// </summary>
        /// <returns>String list with device paths</returns>
        public string[] getDeviceList()
        {
            return (string[])usbdevice.getDevices().ToArray(typeof(string));
        }

        /// <summary>
        /// Writes the specified bytes to the USB device.
        /// If the array length exceeds 64, the array while be divided into several 
        /// arrays with each containing 64 bytes.
        /// The 0-63 byte of the array is sent first, then the 64-127 byte and so on.
        /// </summary>
        /// <param name="bytes">The bytes to send.</param>
        /// <returns>Returns true if all bytes have been written successfully</returns>
        public bool write(byte[] bytes)
        {
            int byteCount = bytes.Length;
            int bytePos = 0;

            bool success = true;

            //build hid reports with 64 bytes
            while (bytePos <= byteCount - 1)
            {
                if (bytePos > 0)
                {
                    Thread.Sleep(5);
                }

                byte[] transfByte = new byte[64];
                for (int u = 0; u < 64; u++)
                {
                    if (bytePos < byteCount)
                    {
                        transfByte[u] = bytes[bytePos];
                        bytePos++;
                    }
                    else
                    {
                        transfByte[u] = 0;
                    }
                }

                //send the report
                if (!usbdevice.writeData(transfByte))
                {
                    success = false;
                }

                Thread.Sleep(5);
            }

            return success;
        }

        /// <summary>
        /// Starts reading. 
        /// If you execute this command a thread is started which listens to the USB device and waits for data.
        /// </summary>
        public void startRead()
        {
            usbdevice.readData();
        }

        /// <summary>
        /// Stops the read thread.
        /// By executing this command the read data thread is stopped and now data will be received.
        /// </summary>
        public void stopRead()
        {
            usbdevice.readData();
        }

        /// <summary>
        /// Enables the usb buffer event.
        /// Whenever a dataset is added to the buffer (and so received from the usb device)
        /// the event handler method will be called.
        /// </summary>
        /// <param name="eHandler">The event handler method.</param>
        public void enableUsbBufferEvent(EventHandler eHandler)
        {
            usbBuffer.Changed += eHandler;
        }
    }
}