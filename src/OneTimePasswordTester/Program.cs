using System;
using System.Threading.Tasks;
using OneTimePassword;

namespace OneTimePasswordTester
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Program().Run().Wait();
        }

        private async Task Run()
        {
            var otp = new TimeBasedOneTimePassword("7XPNLIZJIMZMUV7XOO7MLVDYJU");

            string previous = null;
            bool isRunning = true;

            Console.CancelKeyPress += (ss, ee) => { ee.Cancel = true; isRunning = false; };

            while (isRunning)
            {
                var result = otp.GenerateOneTimePassword();
                result = OneTimePasswordUtility.Truncate(result, 6);
                if (result != previous)
                    Console.WriteLine($"{result.Substring(0, 3)} {result.Substring(3)}");
                previous = result;

                for (int i = 0; i < 10 && isRunning; i++)
                    await Task.Delay(100);
            }
        }
    }
}
