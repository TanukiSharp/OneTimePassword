using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OneTimePassword;

namespace OneTimePasswordApp
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return new Program().Run(args).Result;
        }

        private async Task<int> Run(string[] args)
        {
            if (ParseArguments(args) == false)
                return -1;

            var otp = new TimeBasedOneTimePassword(secretValue);

            string previous = null;
            bool isRunning = true;

            if (onlyOnce == false)
            {
                Console.WriteLine("Press Ctrl+C key to exit");
                Console.WriteLine();
                Console.CancelKeyPress += (ss, ee) => { ee.Cancel = true; isRunning = false; };
            }

            int split = 0;
            if (truncate % 3 == 0)
                split = 3;
            else if (truncate % 4 == 0)
                split = 4;

            while (isRunning)
            {
                string result = otp.GenerateOneTimePassword(GetTimestamp(), hmacAlgorithmType);
                result = OneTimePasswordUtility.Truncate(result, truncate);

                if (result != previous)
                {
                    if (split == 0 || noInnerSpace)
                        Console.Write(result);
                    else
                        PrintSplit(result, split);

                    if (onlyOnce)
                        break;

                    Console.WriteLine();
                }

                previous = result;

                for (int i = 0; i < 10 && isRunning; i++)
                    await Task.Delay(100);
            }

            return 0;
        }

        private DateTime GetTimestamp()
        {
            if (isFixedTimestamp)
                return fixedTimestamp;

            return DateTime.UtcNow.AddSeconds(deltaSeconds);
        }

        private void PrintSplit(string value, int split)
        {
            int k = 0;

            while (k < value.Length)
            {
                if (k > 0)
                    Console.Write(" ");
                Console.Write(value.Substring(k, split));
                k += split;
            }
        }

        private bool onlyOnce;
        private bool noInnerSpace;
        private string secretValue;
        private int truncate = 6;
        private HmacAlgorithmType hmacAlgorithmType = HmacAlgorithmType.SHA1;
        private bool isFixedTimestamp = false;
        private DateTime fixedTimestamp;
        private int deltaSeconds = 0;

        private void PrintUsage()
        {
            AssemblyName asnName = Assembly.GetEntryAssembly().GetName();

            Console.WriteLine($"{asnName.Name} v{asnName.Version}");
            Console.WriteLine();
            Console.WriteLine("  --secretFile <file>            Absolute or relative file containing the secret text");
            Console.WriteLine("  --secret <secret>              Secret text (--secretFile takes precedence if given)");
            Console.WriteLine();
            Console.WriteLine("  Arguments --secretFile and --secret are mutually exclusive, but one of them must be given");
            Console.WriteLine();
            Console.WriteLine("  --hmac <hamc>                  HMAC algorithm to use, available values are SHA1, SHA256 and SHA512 (defaults to SHA1)");
            Console.WriteLine("  --time <offset> | <datetime>   Positive or negative offset in seconds from UTC now, or fixed UTC date and time");
            Console.WriteLine("  --trucate <number>             Truncates the OTP to <number> digits (defaults to 6)");
            Console.WriteLine("  --once                         Prints OTP only once and without line feed (defaults to unset)");
            Console.WriteLine("  --no-inner-spaces              OTP is printed without any spaces (defaults to unset)");
            Console.WriteLine();
            Console.WriteLine("  -h | -? | --help               Prints this help usage");
            Console.WriteLine();
        }

        private bool ParseArguments(string[] args)
        {
            int index;

            if (args.Length < 2)
            {
                PrintUsage();
                return false;
            }

            if (args.Contains("-h") || args.Contains("--help") || args.Contains("--herp") || args.Contains("-?"))
            {
                PrintUsage();
                return false;
            }

            onlyOnce = args.Contains("--once");
            noInnerSpace = args.Contains("--no-inner-space") || args.Contains("--no-inner-spaces");

            index = Array.IndexOf(args, "--hmac");
            if (index >= 0)
            {
                if (index < args.Length - 1)
                {
                    switch (args[index + 1].ToLower())
                    {
                        case "sha1":
                            hmacAlgorithmType = HmacAlgorithmType.SHA1;
                            break;
                        case "sha256":
                            hmacAlgorithmType = HmacAlgorithmType.SHA256;
                            break;
                        case "sha512":
                            hmacAlgorithmType = HmacAlgorithmType.SHA512;
                            break;
                        default:
                            Console.WriteLine($"Invalid 'hmac' value '{args[index + 1]}', fallback to default value '{hmacAlgorithmType}'");
                            break;
                    }
                }
                else
                    Console.WriteLine($"Missing value of 'hmac' argument");
            }

            index = Array.IndexOf(args, "--time");
            if (index >= 0)
            {
                if (index < args.Length - 1)
                {
                    string timeStr = args[index + 1];

                    void ParseTimeInt(string str)
                    {
                        if (int.TryParse(str, out int localDeltaSeconds) == false)
                            Console.WriteLine($"Invalid 'time' value '{timeStr}', fallback to default value '(current UTC time)'");
                        else
                            deltaSeconds = localDeltaSeconds;
                    }

                    if (timeStr.StartsWith('+') || timeStr.StartsWith('-'))
                        ParseTimeInt(timeStr);
                    else
                    {
                        if (DateTime.TryParse(timeStr, null, DateTimeStyles.AdjustToUniversal, out DateTime localFixedTimestamp) == false)
                            ParseTimeInt(timeStr);
                        else
                        {
                            fixedTimestamp = localFixedTimestamp;
                            isFixedTimestamp = true;
                        }
                    }
                }
                else
                    Console.WriteLine($"Missing value of 'time' argument");
            }

            index = Array.IndexOf(args, "--truncate");
            if (index >= 0)
            {
                if (index < args.Length - 1)
                {
                    if (int.TryParse(args[index + 1], out int localTruncate) == false)
                        Console.WriteLine($"Invalid 'truncate' value '{args[index + 1]}', fallback to default value '{truncate}'");
                    else if (localTruncate < 0)
                    {
                        Console.WriteLine($"Invalid 'truncate' value '{args[index + 1]}', fallback to '1'");
                        truncate = 1;
                    }
                    else if (localTruncate > 8)
                    {
                        Console.WriteLine($"Invalid 'truncate' value '{args[index + 1]}', fallback to '8'");
                        truncate = 8;
                    }
                    else
                        truncate = localTruncate;
                }
                else
                    Console.WriteLine($"Missing value of 'truncate' argument");
            }

            index = Array.IndexOf(args, "--secretFile");
            if (index >= 0)
            {
                if (index < args.Length - 1)
                {
                    string file = args[index + 1];
                    if (Path.IsPathRooted(file) == false)
                        file = Path.Combine(Environment.CurrentDirectory, file);

                    if (File.Exists(file) == false)
                    {
                        Console.WriteLine($"Could not find file '${file}'");
                        return false;
                    }

                    secretValue = File.ReadAllText(file).Trim();
                    return true;
                }
                else
                    Console.WriteLine($"Missing value of 'secretFile' argument");
            }

            index = Array.IndexOf(args, "--secret");
            if (index >= 0)
            {
                if (index < args.Length - 1)
                {
                    secretValue = args[index + 1];
                    return true;
                }
                else
                    Console.WriteLine($"Missing value of 'secret' argument");
            }

            Console.WriteLine("Missing mandatory 'secretFile' or 'secret' argument");

            return false;
        }
    }
}
