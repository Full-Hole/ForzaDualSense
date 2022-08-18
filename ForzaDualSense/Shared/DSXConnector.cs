using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ForzaDualSense.Shared
{
    public static class DSXConnector
    {
        static bool _verbose;
        static UdpClient _senderClient;
        static IPEndPoint _endPoint;
        static Settings _settings;
        public static IPAddress localhost = new IPAddress(new byte[] { 127, 0, 0, 1 });

        public static void Config(Settings settings)
        {
            _verbose = _settings.VERBOSE;
            _settings = settings;
        }

        public static void Close()
        {
            if (_senderClient != null)
            {
                _senderClient.Close();
                _senderClient.Dispose();
            }
        }
        //Connect to DualSenseX
        public static void Connect()
        {
            _senderClient = new UdpClient();
            var portNumber = File.ReadAllText(@"C:\Temp\DualSenseX\DualSenseX_PortNumber.txt");
            Console.WriteLine("DSX is using port " + portNumber + ". Attempting to connect..");
            int portNum = _settings.DSX_PORT;
            if (portNumber != null)
            {
                try
                {
                    portNum = Convert.ToInt32(portNumber);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"DSX provided a non numerical Port! Using configured default({_settings.DSX_PORT}).");
                    portNum = _settings.DSX_PORT;
                }
            }
            else
            {
                Console.WriteLine($"DSX did not provided a port value. Using configured default({_settings.DSX_PORT})");
            }

            _endPoint = new IPEndPoint(localhost, Convert.ToInt32(portNumber));
            try
            {
                _senderClient.Connect(_endPoint);
            }
            catch (Exception e)
            {
                Console.Write("Error Connecting: ");

                if (e is SocketException)
                {
                    Console.WriteLine("Couldn't Access Port. " + e.Message);
                }
                else if (e is ObjectDisposedException)
                {
                    Console.WriteLine("Connection Object Closed. Restart the Application.");
                }
                else
                {
                    Console.WriteLine("Unknown Error: " + e.Message);
                }
                throw e;
            }
        }
        //Send Data to DualSenseX
        public static void Send(DSXInstructions data)
        {
            if (_verbose)
            {
                Console.WriteLine($"Converting Message to JSON");
            }
            byte[] RequestData = Encoding.ASCII.GetBytes(PacketConverter.PacketToJson(data));
            if (_verbose)
            {
                Console.WriteLine($"{Encoding.ASCII.GetString(RequestData)}");
            }
            try
            {
                if (_verbose)
                {
                    Console.WriteLine($"Sending Message to DSX...");
                }
                _senderClient.Send(RequestData, RequestData.Length);
                if (_verbose)
                {
                    Console.WriteLine($"Message sent to DSX");
                }
            }
            catch (Exception e)
            {
                Console.Write("Error Sending Message: ");

                if (e is SocketException)
                {
                    Console.WriteLine("Couldn't Access Port. " + e.Message);
                    throw e;
                }
                else if (e is ObjectDisposedException)
                {
                    Console.WriteLine("Connection closed. Restarting...");
                    Connect();
                }
                else
                {
                    Console.WriteLine("Unknown Error: " + e.Message);

                }

            }
        }
    }
}
