using System.Diagnostics;
using System.Timers;

namespace WiFiMonitor
{
    class Program
    {
        private static System.Timers.Timer checkTimer;
        private static HashSet<string> knownMACAddresses = new HashSet<string>();

        static void Main()
        {
            checkTimer = new System.Timers.Timer(15000);
            checkTimer.Elapsed += CheckForNewDevices;
            checkTimer.Start();

            Console.WriteLine("Monitoring connected devices...");
            Console.ReadLine();
        }

        private static void CheckForNewDevices(object sender, ElapsedEventArgs e)
        {
            var currentDevices = GetConnectedDevices();
            foreach (var mac in currentDevices)
            {
                if (!knownMACAddresses.Contains(mac))
                {
                    Console.WriteLine($"New device detected: {mac}");
                    knownMACAddresses.Add(mac);
                }
            }
        }

        private static HashSet<string> GetConnectedDevices()
        {
            HashSet<string> macAddresses = new HashSet<string>();
            var arpStream = ExecuteCommandLine("arp", "-a");

            while (!arpStream.EndOfStream)
            {
                var line = arpStream.ReadLine()?.Trim();
                if (line != null && (line.StartsWith("192.168.") || line.StartsWith("10.")))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var mac = parts[1];
                        macAddresses.Add(mac);
                    }
                }
            }

            return macAddresses;
        }

        private static StreamReader ExecuteCommandLine(string file, string args = "")
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();

            return process.StandardOutput;
        }
    }
}
