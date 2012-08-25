using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace osu_Tencho
{
    internal static class Broadcaster
    {
        static UdpClient client;
        static byte[] data;
        internal static void Initialize()
        {

            client = new UdpClient();

            string dataString = "Tenchooooooo";
            data = Encoding.UTF8.GetBytes(dataString);

            Thread t = new Thread(Run);
            t.IsBackground = true;
            t.Start();
        }

        static void Run()
        {
            while (true)
            {
                IPHostEntry iphostentry = Dns.GetHostEntry(Dns.GetHostName());

                foreach (IPAddress addr in iphostentry.AddressList)
                {
                    try
                    {
                        string bc = addr.ToString();
                        bc = bc.Remove(bc.LastIndexOf('.') + 1) + "255";
                        
                        IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Parse(bc), Tencho.PortTencho);
                        client.Send(data, data.Length, sendEndPoint);
                    }
                    catch { }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
