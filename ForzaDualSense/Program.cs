using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.IO;
using CsvHelper;
using System.Globalization;
using ForzaDualSense.Shared;
using ForzaDualSense.Model;
using ForzaDualSense.Extensions;
using System.Runtime;

namespace ForzaDualSense
{
    class Program
    {
        static bool _verbose = false;
        static Settings _settings = new Settings();
        static bool _logToCsv = false;
        static String _csvPath = "";

        private static DataPacket data = new DataPacket();

        //Main running thread of program.
        static async Task Main(string[] args)
        {
            UdpClient? client = null;
            StreamWriter? writer = null;
            CsvWriter? csv = null;
            try
            {
                if (!SetConfig())
                    return;

                ParseArgs(args);

                if (!_settings.DISABLE_APP_CHECK)
                {
                    CheckRunningProcess();
                }
                DSXDataBuilder.Config(_settings);

                //Connect to DualSenseX
                DSXConnector.Config(_settings);
                DSXConnector.Connect();

                //Connect to Forza
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, _settings.FORZA_PORT);
                client = new UdpClient(_settings.FORZA_PORT);

                Console.WriteLine($"The Program is running. Please set the Forza data out to 127.0.0.1, port {_settings.FORZA_PORT} and verify the DualSenseX UDP Port is set to {_settings.DSX_PORT}");
                UdpReceiveResult receive;
                if (_logToCsv)
                {
                    try
                    {
                        writer = new StreamWriter(_csvPath);
                        csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                        csv.WriteHeader<CsvData>();
                        csv.NextRecord();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to open csv File output. Ensure it is a valid path!");
                        throw;
                    }
                }

                int count = 0;

                //Main loop, go until killed
                while (true)
                {
                    //If Forza sends an update
                    receive = await client.ReceiveAsync();

                    if (_verbose)
                    {
                        Console.WriteLine("recieved Message from Forza!");
                    }
                    //parse data
                    var resultBuffer = receive.Buffer;
                    if (!FMData.AdjustToBufferType(resultBuffer.Length))
                    {
                        //  return;
                    }
                    data = PacketConverter.ParseData(resultBuffer);
                    if (_verbose)
                    {
                        Console.WriteLine("Data Parsed");
                    }

                    //Process and send data to DualSenseX
                    DSXConnector.Send(DSXDataBuilder.GetInstructions(data, csv)); ;
                    if (_logToCsv && count++ > _settings.CSV_BUFFER_LENGTH)
                    {
                        writer?.Flush();
                        count = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Application encountered an exception: " + e.Message);
            }
            finally
            {
                if (_verbose)
                {
                    Console.WriteLine($"Cleaning Up");
                }
                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
                DSXConnector.Close();
                if (csv != null)
                {
                    csv.Dispose();
                }
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                }

                if (_verbose)
                {
                    Console.WriteLine($"Cleanup Finished. Exiting...");
                }
            }
            return;
        }

        private static void CheckRunningProcess()
        {
            int forzaProcesses = Process.GetProcessesByName("ForzaHorizon 5").Length;
            forzaProcesses += Process.GetProcessesByName("ForzaHorizon4").Length;
            forzaProcesses += Process.GetProcessesByName("ForzaMotorsport7").Length;
            Process[] DSX = Process.GetProcessesByName("DualSenseX");
            Process[] DSX_2 = Process.GetProcessesByName("DualsenseX");
            Process[] cur = Process.GetProcesses();
            while (forzaProcesses == 0 || DSX.Length + DSX_2.Length == 0)
            {
                if (forzaProcesses == 0)
                {
                    Console.WriteLine("No Running Instances of Forza found. Waiting... ");

                }
                else if (DSX.Length == 0)
                {
                    Console.WriteLine("No Running Instances of DualSenseX found. Waiting... ");
                }
                System.Threading.Thread.Sleep(1000);
                forzaProcesses += Process.GetProcessesByName("ForzaHorizon5").Length;
                forzaProcesses += Process.GetProcessesByName("ForzaHorizon4").Length; //Guess at name
                forzaProcesses += Process.GetProcessesByName("ForzaMotorsport7").Length; //Guess at name
                DSX_2 = Process.GetProcessesByName("DualsenseX");
                DSX = Process.GetProcessesByName("DualSenseX");
            }
            Console.WriteLine("Forza and DSX are running. Let's Go!");
        }

        // Build a config object, using env vars and JSON providers.
        private static bool SetConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                    .AddIniFile("appsettings.ini")
                    .Build();
            try
            {
                // Get values from the config given their key and their target type.
                _settings = config.Get<Settings>();
                _verbose = _settings.VERBOSE;
                _logToCsv = _settings.LOG_TO_CSV;
                _csvPath = _settings.CSV_PATH;
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid Configuration File!");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static void ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)

            {
                string arg = args[i];

                switch (arg)
                {
                    case "-v":
                        {
                            Console.WriteLine("ForzaDSX Version {0}", typeof(Program).Assembly.GetName().Version);
                            return;
                        }
                    case "--verbose":
                        {
                            Console.WriteLine("Verbose Mode Enabled!");
                            _verbose = true;
                            break;
                        }
                    case "--csv":
                        {
                            _logToCsv = true;
                            i++;
                            if (i >= args.Length)
                            {
                                Console.WriteLine("No Path Entered for Csv file output!! Exiting");
                                return;
                            }
                            _csvPath = args[i];
                            break;
                        }
                    default:
                        {

                            break;
                        }
                }
            }
        }

    }
}
