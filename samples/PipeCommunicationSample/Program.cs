using HostManagement.Shared.Communication;
using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

internal class Program
{
    private static async Task Main()
    {
        //TODO: Generate Event in HostManagement to call TestCallbackHandler 
        IPipeClient? client = null;
        try
        {
            client = new PipeClient("HostPipe", new ConsoleLogger(LogLevel.Information));
            await client.Connect();

            Console.WriteLine("Client says: 'ping'");
            var response = await client.SendRequest(Topics.Echo, "ping");
            Console.WriteLine($"Server says: '{response.Content}'");
            Console.WriteLine();
            await Task.Delay(200);

            Console.WriteLine("Client says: 'love'");
            var response2 = await client.SendRequest(Topics.Echo, "love");
            Console.WriteLine($"Server says: '{response2.Content}'");
            Console.WriteLine();
            await Task.Delay(200);

            Console.WriteLine("Client says: 'trigger'");
            var response3 = await client.SendRequest(Topics.Echo, "trigger");
            Console.WriteLine($"Server says: '{response3.Content}'");
            Console.WriteLine();

            Console.ReadKey();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            client?.Dispose();
        }
    }
}
