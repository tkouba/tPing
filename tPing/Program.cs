using System;
using System.Net.Sockets;
using System.Reflection;

namespace tPing
{
    class Program
    {
        private static bool breakSignalled = false;

        static int Main(string[] args)
        {

            Console.WriteLine(Properties.Resources.HeaderString, Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();
            try
            {
                Arguments arguments = new Arguments();
                arguments.Process(args);

                Console.WriteLine(Properties.Resources.ConnectingTo, arguments.HostName, arguments.Address, arguments.ProtocolType, arguments.Port);
                Console.WriteLine();

                if (arguments.ShowHelp)
                {
                    Console.WriteLine(Properties.Resources.Help);
                }
                else
                {
                    Console.CancelKeyPress += Console_CancelKeyPress;
                    Statistics statistics = new Statistics();
                    for (int i = 0; i < arguments.Count; i++)
                    {
                        using (Socket socket = new Socket(arguments.Address.AddressFamily, arguments.SocketType, arguments.ProtocolType))
                        {
                            try
                            {
                                var watch = System.Diagnostics.Stopwatch.StartNew();
                                //s.Connect(arguments.Address, arguments.Port);
                                IAsyncResult result = socket.BeginConnect(arguments.Address, arguments.Port, null, null);
                                bool success = result.AsyncWaitHandle.WaitOne(arguments.Timeout, true);
                                if (socket.Connected)
                                {
                                    socket.EndConnect(result);
                                }
                                else
                                {
                                    // NOTE, MUST CLOSE THE SOCKET
                                    socket.Close();
                                    throw new TimeoutException();
                                }
                                watch.Stop();
                                Console.WriteLine(Properties.Resources.ConnectedTo, socket.RemoteEndPoint, watch.ElapsedMilliseconds);
                                statistics.AddGood(watch.ElapsedMilliseconds);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(Properties.Resources.ConnectionFailed, ex.Message);
                                statistics.AddBad();
                            }
                        }
                        if (breakSignalled)
                            break;
                    }
                    Console.WriteLine();
                    Console.WriteLine(Properties.Resources.ConnectionStatistics);
                    Console.WriteLine(Properties.Resources.ConnectionStatisticsDetail,
                        statistics.Attempts, statistics.Good, statistics.Bad, (float)statistics.Bad / statistics.Attempts);
                    Console.WriteLine(Properties.Resources.ConnectionTime);
                    Console.WriteLine(Properties.Resources.ConnectionTimeDetail,
                        statistics.MinTime, statistics.MaxTime, statistics.AverageTime);
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Properties.Resources.Error, ex.Message);
                return 255;
            }
            return 0;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            breakSignalled = true;
        }
    }
}
