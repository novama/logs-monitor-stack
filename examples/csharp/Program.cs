
class Program
{
    static void Main(string[] args)
    {
        // Calling example that uses Serilog Sink package
        SerilogLoggingExample.RunExample();

        // Calling example that submits a direct API POST request
        PostLoggingExample.RunExample();
    }
}
