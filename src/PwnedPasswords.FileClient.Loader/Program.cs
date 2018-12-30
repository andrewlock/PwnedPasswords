using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace PwnedPasswords.FileClient.Loader
{
    class Program
    {
        // Return codes
        public const int EXCEPTION = 2;
        public const int ERROR = 1;
        public const int OK = 0;

        public static async Task<int> Main(string[] args)
        {
            try
            {
                return await CommandLineApplication.ExecuteAsync<PwnedPasswordBloomFilterTool>(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Unexpected error: {ex}");
                Console.ResetColor();
                return EXCEPTION;
            }
        }
    }
}
