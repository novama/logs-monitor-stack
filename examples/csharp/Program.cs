
using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Calling example that submits a direct API POST request
            PostLoggingExample.RunExample();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred executing the PostLoggingExample: {ex.Message}");
        }

        try
        {
            // Calling example that uses Serilog Sink package
            SerilogLoggingExample.RunExample();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred executing the SerilogLoggingExample: {ex.Message}");
        }
    }
}
