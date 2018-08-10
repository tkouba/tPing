using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace tPing
{

    class CommandLineException : ApplicationException
    {
        public CommandLineException(string message) : base(message) { /* NOP */ }
        public CommandLineException(string message, object arg) : base(String.Format(message, arg)) { /* NOP */ }
        public CommandLineException(string message, object arg0, object arg1, object arg2) : base(String.Format(message, arg0, arg1, arg2)) { /* NOP */ }
    }


    class Arguments
    {
        private string host; // Destination host name or IP address from command line
        private IPAddress address;
        public string HostName { get; private set; }
        public int Port { get; private set; } = 0;
        public ProtocolType ProtocolType { get; private set; } = ProtocolType.Tcp;
        public IPAddress Address { get => address; }
        public bool ResolveAddress { get; private set; } = false;
        public int Timeout { get; private set; } = 1000;
        public int Count { get; private set; } = 5;
        public bool UseIpV4 { get; private set; }
        public bool UseIpV6 { get; private set; }
        public bool ShowHelp { get; private set; } = false;

        public SocketType SocketType { get => ProtocolType == ProtocolType.Tcp ? SocketType.Stream : ProtocolType == ProtocolType.Udp ? SocketType.Dgram : SocketType.Unknown; }

        public void Process(string[] args)
        {
            if (args == null || args.Length == 0)
                throw new CommandLineException(Properties.Resources.MissingOption);

            IEnumerator enumerator = args.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ProcessArgument(enumerator);
            }

            if (!ShowHelp)
            {
                if (IPAddress.TryParse(host, out address))
                {
                    if (ResolveAddress)
                    {
                        try
                        {
                            HostName = Dns.GetHostEntry(address).HostName;
                        }
                        catch
                        {
                            HostName = address.ToString();
                        }
                    }
                    else
                        HostName = address.ToString();
                }
                else
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(host);
                    HostName = hostEntry.HostName;
                    if (UseIpV4)
                        address = hostEntry.AddressList.FirstOrDefault(o => o.AddressFamily == AddressFamily.InterNetwork);
                    else if (UseIpV6)
                        address = hostEntry.AddressList.FirstOrDefault(o => o.AddressFamily == AddressFamily.InterNetworkV6);
                    else
                        address = hostEntry.AddressList[0];
                }
                if (address == null ||
                    UseIpV4 && address.AddressFamily != AddressFamily.InterNetwork ||
                    UseIpV6 && address.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    throw new CommandLineException(Properties.Resources.CannotFindHost);
                }
            }
        }

        private void ProcessArgument(IEnumerator enumerator)
        {
            string arg = enumerator.Current as string;
            if (String.IsNullOrWhiteSpace(arg))
                throw new CommandLineException(Properties.Resources.MissingOption);

            if (arg.StartsWith("-") || arg.StartsWith("/"))
            {
                arg = arg.StartsWith("--") ? arg.Substring(2) : arg.Substring(1);
                switch (arg.ToLower())
                {
                    case "h":
                    case "?":
                    case "help":
                        ShowHelp = true;
                        break;
                    case "t":
                    case "infinite":
                        Count = Int32.MaxValue;
                        break;
                    case "a":
                    case "resolve":
                        ResolveAddress = true;
                        break;
                    case "n":
                    case "count":
                        Count = GetIntValue(enumerator.Current, enumerator, 1, Int32.MaxValue);
                        break;
                    case "p":
                    case "port":
                        Port = GetIntValue(enumerator.Current, enumerator, 1, 65535);
                        break;
                    case "w":
                    case "timeout":
                        Timeout = GetIntValue(enumerator.Current, enumerator, 1, Int32.MaxValue);
                        break;
                    case "4":
                        UseIpV4 = true;
                        break;
                    case "6":
                        UseIpV6 = true;
                        break;
                    default:
                        throw new CommandLineException(Properties.Resources.BadOption, enumerator.Current);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(host))
                    host = arg;
                else
                    throw new CommandLineException(Properties.Resources.BadParameter, arg);
            }
        }
        private int GetIntValue(object arg, IEnumerator enumerator, int min, int max)
        {
            if (enumerator.MoveNext())
            {
                string str = enumerator.Current as string;
                int value;
                if (Int32.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                    && min <= value && value <= max)
                    return value;
                throw new CommandLineException(Properties.Resources.BadValueForOption, arg, min, max);
            }
            else
                throw new CommandLineException(Properties.Resources.MissingValueForOption, arg);
        }
    }
}
