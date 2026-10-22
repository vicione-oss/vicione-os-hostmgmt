using System.Collections.Concurrent;
using System.Diagnostics;
using HostManagement.Shared;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe.Client;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

internal sealed class Program
{
    private const string PipeName = "HostPipe";

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

            case "4":
                Console.WriteLine();
                await StressTestTwoClients();
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
        Console.WriteLine("4 - Stress test (2 clients)");
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
        var columnCount = 3;
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
        await DoSetSystemConfiguration(content).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends the system configuration and reads the strongly typed result.
    /// </summary>
    private static async Task DoSetSystemConfiguration(string content)
    {
        try
        {
            var knownActionTracker = new KnownActionTracker(TimeSpan.FromMinutes(5));
            var registry = CreateCallbackHandlerRegistry(knownActionTracker);

            await using var client = new PipeClient(PipeName, s_logger, registry);
            _ = await client.TryConnectAsync();

            var configuration = MessageSerializer.Deserialize<SystemConfiguration>(content);

            // The topic carries its request and response contracts, so neither type is named here.
            var result = await client.SendOperationAsync(TopicContracts.SetSystemConfiguration, configuration);

            Console.WriteLine();
            Console.WriteLine($"{result.Status}: {result.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Creates a registry showing the three ways to handle a server event.
    /// </summary>
    private static CallbackHandlerRegistry CreateCallbackHandlerRegistry(KnownActionTracker knownActionTracker)
    {
        var registry = new CallbackHandlerRegistry();

        // A class handling the raw message content.
        registry.RegisterHandler(new TestCallbackHandler(knownActionTracker));

        // A class handling a deserialized message.
        registry.RegisterHandler(new SystemConfigurationChangedCallbackHandler());

        // A callback bound to a typed event topic, without implementing a handler class.
        // The contract of this topic is raw text, so the message arrives unchanged.
        registry.RegisterHandler(EventTopicContracts.PipeServerShutdown, (reason, _, _) =>
        {
            Console.WriteLine();
            Console.WriteLine($"Server is shutting down: {reason}");

            return Task.CompletedTask;
        });

        return registry;
    }

    private static async Task DoSendPipeRequest(string topic, string content)
    {
        try
        {
            var knownActionTracker = new KnownActionTracker(TimeSpan.FromMinutes(5));
            var registry = CreateCallbackHandlerRegistry(knownActionTracker);

            await using var client = new PipeClient(PipeName, s_logger, registry);
            _ = await client.TryConnectAsync();

            // The topic is only known at runtime here, so the raw response content is printed as-is.
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
            var knownActionTracker = new KnownActionTracker(TimeSpan.FromMinutes(5));
            var registry = CreateCallbackHandlerRegistry(knownActionTracker);

            await using var client = new PipeClient(PipeName, s_logger, registry);

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

            var correlationId = Guid.NewGuid();

            // Register before sending because correlated events may arrive
            // before the request response is received.
            knownActionTracker.Add(correlationId);

            var response3 = await client.SendRequestAsync(Topics.Echo, "trigger", correlationId);

            Console.WriteLine($"Server says: '{response3.Content}'");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static async Task StressTestTwoClients()
    {
        var requestsPerClient = AskForInt("Requests per client [default 1000]: ", 1000);
        var parallelismPerClient = AskForInt("Parallelism per client [default 16]: ", 16);
        Console.WriteLine("Press Ctrl+C to cancel...");
        Console.WriteLine();

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += OnCancelKeyPressed;

        var stats = new ConcurrentBag<long>();
        long success = 0;
        long failed = 0;

        var stopwatchTotal = Stopwatch.StartNew();

        try
        {
            // No client logger used here
            await using var client1 = new PipeClient(PipeName, null, new CallbackHandlerRegistry());
            await using var client2 = new PipeClient(PipeName, null, new CallbackHandlerRegistry());

            var connected1 = await client1.TryConnectAsync(timeoutMs: 3_000, cancellationToken: cts.Token);
            var connected2 = await client2.TryConnectAsync(timeoutMs: 3_000, cancellationToken: cts.Token);

            if (!connected1 || !connected2)
            {
                Console.ForegroundColor = ErrorColor;
                Console.WriteLine($"Failed to connect both clients to '{PipeName}'. Is the server running?");
                Console.ResetColor();
                return;
            }

            var task1 = RunLoadAsync(
                "C1",
                client1,
                requestsPerClient,
                parallelismPerClient,
                stats,
                () => Interlocked.Increment(ref success),
                () => Interlocked.Increment(ref failed),
                cts.Token);

            var task2 = RunLoadAsync(
                "C2",
                client2,
                requestsPerClient,
                parallelismPerClient,
                stats,
                () => Interlocked.Increment(ref success),
                () => Interlocked.Increment(ref failed),
                cts.Token);

            await Task.WhenAll(task1, task2);
        }
        catch (OperationCanceledException)
        {
            Console.ForegroundColor = HighlightColor;
            Console.WriteLine("Stress test cancelled by user.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine($"An error occurred: {ex}");
            Console.ResetColor();
        }
        finally
        {
            stopwatchTotal.Stop();
            Console.CancelKeyPress -= OnCancelKeyPressed;
        }

        // Results
        var total = success + failed;
        var duration = stopwatchTotal.Elapsed;
        var throughput = total > 0 ? total / Math.Max(0.001, duration.TotalSeconds) : 0.0;
        var latencies = stats.ToArray();
        Array.Sort(latencies);

        var avg = latencies.Length > 0
            ? TicksToMs((long)latencies.Average(x => (double)x))
            : 0;

        // The pN percentile is the value below which N % of all measured values lie.
        var p50 = GetMillisecondsForPercentile(latencies, 50); // p50 ~ Median: 50 % of requests are faster than this value
        var p95 = GetMillisecondsForPercentile(latencies, 95); // p95: 95 % are faster; the slowest 5% (tail) are above
        var p99 = GetMillisecondsForPercentile(latencies, 99); // p99: 99 % are faster ("tail latency")

        Console.ForegroundColor = HighlightColor;
        Console.WriteLine("Stress test summary");
        Console.ResetColor();
        Console.WriteLine($"Total:      {total} (success: {success}, failed: {failed})");
        Console.WriteLine($"Duration:   {duration}");
        Console.WriteLine($"Throughput: {throughput:F1} requests/s");
        Console.WriteLine($"Latency:    avg={avg:F2} ms | p50={p50:F2} ms | p95={p95:F2} ms | p99={p99:F2} ms");


        void OnCancelKeyPressed(object? sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            cts.Cancel();
        }

        double GetMillisecondsForPercentile(long[] latencies, int percentile)
        {
            if (latencies.Length == 0)
                return 0;

            var rank = Math.Ceiling(percentile / 100.0 * latencies.Length);
            var index = (int)Math.Clamp(rank - 1, 0, latencies.Length - 1);

            return TicksToMs(latencies[index]);
        }

        static double TicksToMs(long t) => t * 1000.0 / Stopwatch.Frequency;
    }

    private static async Task RunLoadAsync(
        string name,
        PipeClient client,
        int count,
        int parallelism,
        ConcurrentBag<long> latencyTicks,
        Action onSuccess,
        Action onFail,
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(parallelism, parallelism);
        var tasks = new Task[count];

        for (var i = 0; i < count; i++)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            var ix = i; // Needed because of the lambda to prevent closure issues

            tasks[ix] = Task.Run(async () =>
            {
                try
                {
                    using var perRequestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    perRequestCts.CancelAfter(TimeSpan.FromSeconds(5));

                    var stopwatch = Stopwatch.StartNew();

                    var response = await client
                        .SendRequestAsync(Topics.Echo, $"stress-{name}-{ix}", cancellationToken: perRequestCts.Token)
                        .ConfigureAwait(false);

                    stopwatch.Stop();

                    if (response.Type == MessageType.Response)
                    {
                        latencyTicks.Add(stopwatch.ElapsedTicks);
                        onSuccess();
                    }
                    else
                    {
                        onFail();
                    }
                }
                catch
                {
                    onFail();
                }
                finally
                {
                    semaphore.Release();
                }
            },
            cancellationToken);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static int AskForInt(string prompt, int fallback)
    {
        Console.ForegroundColor = HighlightColor;
        Console.Write(prompt);
        Console.ResetColor();

        var input = Console.ReadLine();
        return int.TryParse(input, out var value) && value > 0 ? value : fallback;
    }
}
