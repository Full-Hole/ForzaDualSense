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

namespace ForzaDualSense
{
    class Program
    {
        static Settings settings = new Settings();
        static bool verbose = false;
        static bool logToCsv = false;
        static String csvFileName = "";
        public const int CSV_BUFFER_LENGTH = 120;
                
        private static DataPacket data = new DataPacket();        

        //Main running thread of program.
        static async Task Main(string[] args)
        {
            UdpClient? client = null;
            StreamWriter? writer = null;
            CsvWriter? csv = null;
            try
            {
                ParseArgs(args);

                if (!SetConfig())
                    return;                
                
                if (!settings.DISABLE_APP_CHECK)
                {
                    CheckRunningProcess();
                }
                DSXDataBuilder.Config(verbose, logToCsv, settings);

                //Connect to DualSenseX
                DSXConnector.Config(verbose, settings);
                DSXConnector.Connect();

                //Connect to Forza
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, settings.FORZA_PORT);
                client = new UdpClient(settings.FORZA_PORT);

                Console.WriteLine($"The Program is running. Please set the Forza data out to 127.0.0.1, port {settings.FORZA_PORT} and verify the DualSenseX UDP Port is set to {settings.DSX_PORT}");
                UdpReceiveResult receive;
                if (logToCsv)
                {
                    try
                    {
                        writer = new StreamWriter(csvFileName);
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
                    
                    if (verbose)
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
                    if (verbose)
                    {
                        Console.WriteLine("Data Parsed");
                    }

                    //Process and send data to DualSenseX
                    DSXConnector.Send(DSXDataBuilder.GetInstructions(data, csv)); ;
                    if (logToCsv && count++ > CSV_BUFFER_LENGTH)
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
                if (verbose)
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

                if (verbose)
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
                settings = config.Get<Settings>();
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
                            verbose = true;
                            break;
                        }
                    case "--csv":
                        {
                            logToCsv = true;
                            i++;
                            if (i >= args.Length)
                            {
                                Console.WriteLine("No Path Entered for Csv file output!! Exiting");
                                return;
                            }
                            csvFileName = args[i];
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
