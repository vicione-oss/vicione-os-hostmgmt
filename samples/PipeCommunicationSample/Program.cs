using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.NamedPipe.Client;
using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

internal sealed class Program
{
    private const ConsoleColor ErrorColor = ConsoleColor.Red;
    private const ConsoleColor HighlightColor = ConsoleColor.DarkCyan;

    private static readonly ILogger<PipeClient> s_logger =
        new ConsoleLogger(LogLevel.Information).As<PipeClient>();

    private static async Task Main()
    {
        PrintHeader();

        Console.ForegroundColor = HighlightColor;
        Console.Write("Choice: ");
        Console.ResetColor();

        var choice = Console.ReadLine() ?? string.Empty;

        switch (choice.ToLower())
        {
            case "1":
                Console.WriteLine();
                await SendPipeRequest();
                break;

            case "2":
                Console.WriteLine();
                await SetJsonConfig();
                break;

            case "3":
                Console.WriteLine();
                await ClientSays();
                break;

            default:
                Console.ForegroundColor = ErrorColor;
                Console.WriteLine("Unknown mode provided.");
                Console.ResetColor();
                break;
        }

        Console.WriteLine();
    }

    private static void PrintHeader()
    {
        Console.WriteLine("1 - Send pipe request");
        Console.WriteLine("2 - Set JSON config");
        Console.WriteLine("3 - Client says...");
        Console.WriteLine();
    }

    private static async Task SendPipeRequest()
    {
        Console.ForegroundColor = HighlightColor;
        Console.WriteLine("Available topics");
        Console.ResetColor();

        var allTopics = Topics.GetAllTopics().ToList();

        PrintAllAvailableTopics(allTopics);

        var topic = AskForTopic();

        if (!allTopics.Contains(topic))
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine("No valid topic provided.");
            Console.ResetColor();
            return;
        }

        var topicsWithRequiredContent = Topics.GetTopicsWithRequiredContent().ToList();
        var content = string.Empty;

        if (topicsWithRequiredContent.Contains(topic))
            content = AskForContent();

        Console.WriteLine();

        await DoSendPipeRequest(topic, content);
    }

    private static void PrintAllAvailableTopics(List<string> allTopics)
    {
        var columnCount = 4;
        var maximumTopicLength = allTopics.Max(topic => topic.Length);

        for (var i = 0; i < allTopics.Count; i++)
        {
            Console.Write(allTopics[i].PadRight(maximumTopicLength + 2));

            if ((i + 1) % columnCount == 0 ||
                (i + 1) == allTopics.Count)
            {
                Console.WriteLine();
            }
        }

        Console.WriteLine();
    }

    private static string AskForTopic()
    {
        Console.ForegroundColor = HighlightColor;
        Console.Write("Topic: ");
        Console.ResetColor();

        return Console.ReadLine() ?? string.Empty;
    }

    private static string AskForContent()
    {
        Console.ForegroundColor = HighlightColor;
        Console.Write("Content: ");
        Console.ResetColor();

        return Console.ReadLine() ?? string.Empty;
    }

    private static async Task SetJsonConfig()
    {
        var validTopics = Topics.GetAllTopics();
        var topic = "SetSystemConfiguration";

        if (!validTopics.Contains(topic))
            throw new InvalidOperationException($"Topic '{topic}' is not supported.");

        Console.ForegroundColor = HighlightColor;
        Console.Write("JSON config path: ");
        Console.ResetColor();
        var configPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(configPath))
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine("No system configuration path provided.");
            Console.ResetColor();
            return;
        }

        if (!configPath.EndsWith("SystemConfiguration.json"))
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine("No compatible system configuration provided (expected '*SystemConfiguration.json').");
            Console.ResetColor();
            return;
        }

        if (!File.Exists(configPath))
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine("No system configuration found.");
            Console.ResetColor();
            return;
        }

        var content = await File.ReadAllTextAsync(configPath).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(content))
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine("JSON config is empty.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine();
        await DoSendPipeRequest(topic, content).ConfigureAwait(false);
    }

    private static async Task DoSendPipeRequest(string topic, string content)
    {
        try
        {
            await using var client = new PipeClient("HostPipe", s_logger);
            _ = await client.TryConnectAsync();

            var response = await client.SendRequestAsync(topic, content);

            Console.WriteLine();
            Console.WriteLine(response.Content);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static async Task ClientSays()
    {
        try
        {
            var registry = new CallbackHandlerRegistry();
            registry.RegisterHandler(new TestCallbackHandler());

            await using var client = new PipeClient("HostPipe", s_logger, registry);

            _ = await client.TryConnectAsync();

            Console.WriteLine("Client says: 'ping'");
            var response = await client.SendRequestAsync(Topics.Echo, "ping");
            Console.WriteLine($"Server says: '{response.Content}'");
            Console.WriteLine();
            await Task.Delay(200);

            Console.WriteLine("Client says: 'love'");
            var response2 = await client.SendRequestAsync(Topics.Echo, "love");
            Console.WriteLine($"Server says: '{response2.Content}'");
            Console.WriteLine();
            await Task.Delay(200);

            Console.WriteLine("Client says: 'trigger'");
            var response3 = await client.SendRequestAsync(Topics.Echo, "trigger");
            Console.WriteLine($"Server says: '{response3.Content}'");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
