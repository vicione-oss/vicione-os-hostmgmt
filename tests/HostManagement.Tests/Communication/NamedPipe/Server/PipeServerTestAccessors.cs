using System.Reflection;
using HostManagement.Communication.NamedPipe.Server;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.NamedPipe;

namespace HostManagement.Tests.Communication.NamedPipe.Server;

internal static class PipeServerTestAccessors
{
    #region Cached reflection members
    private static readonly Type s_serverType = typeof(PipeServer);

    private static readonly Type s_messageQueueItemType =
        s_serverType.GetNestedType("MessageQueueItem", BindingFlags.NonPublic)
        ?? throw new MissingMemberException("PipeServer.MessageQueueItem not found");

    private const string ServerStateFullName = "HostManagement.Communication.NamedPipe.Server.ServerState";
    private static readonly Type s_serverStateType =
        s_serverType.Assembly.GetType(ServerStateFullName)
        ?? throw new MissingMemberException($"{ServerStateFullName} not found");

    private static readonly FieldInfo s_activeConnectionsFieldInfo =
        s_serverType.GetField("_activeConnections", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException("PipeServer._activeConnections not found");

    private static readonly FieldInfo s_backgroundTasksFieldInfo =
        s_serverType.GetField("_backgroundTasks", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException("PipeServer._backgroundTasks not found");

    private static readonly FieldInfo s_serverStateRawFieldInfo =
        s_serverType.GetField("_serverStateRaw", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException("PipeServer._serverStateRaw not found");

    private static readonly FieldInfo s_degradedListenerCountFieldInfo =
        s_serverType.GetField("_degradedListenerCount", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException("PipeServer._degradedListenerCount not found");

    private static readonly MethodInfo s_asClientMessageMethodInfo =
        s_messageQueueItemType.GetMethod("AsClientMessage", BindingFlags.Public | BindingFlags.Static)
        ?? throw new MissingMethodException("MessageQueueItem.AsClientMessage not found");

    private static readonly MethodInfo s_asServerMessageMethodInfo =
        s_messageQueueItemType.GetMethod("AsServerMessage", BindingFlags.Public | BindingFlags.Static)
        ?? throw new MissingMethodException("MessageQueueItem.AsServerMessage not found");

    private static readonly MethodInfo s_enqueueMessageMethodInfo =
        s_serverType.GetMethod("EnqueueMessage", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingMethodException("PipeServer.EnqueueMessage not found");

    private static readonly Func<PipeServer, TimeSpan, Task> s_enqueueShutdownBarrierAndWaitAsyncDelegate =
        (s_serverType
            .GetMethod("EnqueueShutdownBarrierAndWaitAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException("PipeServer.EnqueueShutdownBarrierAndWaitAsync not found"))
            .CreateDelegate<Func<PipeServer, TimeSpan, Task>>();

    private static readonly Func<PipeServer, CancellationToken, Task> s_processQueueAsyncDelegate =
        (s_serverType
            .GetMethod("ProcessQueueAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException("PipeServer.ProcessQueueAsync not found"))
            .CreateDelegate<Func<PipeServer, CancellationToken, Task>>();

    private static readonly Func<PipeServer, IPipeStreamWrapper, int, CancellationToken, Task> s_readRequestsAsyncDelegate =
        (s_serverType
            .GetMethod("ReadRequestsAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException("PipeServer.ReadRequestsAsync not found"))
            .CreateDelegate<Func<PipeServer, IPipeStreamWrapper, int, CancellationToken, Task>>();

    private static readonly Action<PipeServer> s_setOperationalStateDelegate =
        (s_serverType
            .GetMethod("SetOperationalState", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException("PipeServer.SetOperationalState not found"))
            .CreateDelegate<Action<PipeServer>>();
    #endregion

    public static void AddActiveConnection(PipeServer server, IPipeStreamWrapper wrapper)
    {
        var list = (List<IPipeStreamWrapper>)s_activeConnectionsFieldInfo.GetValue(server)!;
        list.Add(wrapper);
    }

    public static void AddBackgroundTask(PipeServer server, Task task)
    {
        var list = (List<Task>)s_backgroundTasksFieldInfo.GetValue(server)!;
        list.Add(task);
    }

    public static void IncrementDegradedListenerCount(PipeServer server)
    {
        var currentCount = (int)s_degradedListenerCountFieldInfo.GetValue(server)!;
        s_degradedListenerCountFieldInfo.SetValue(server, currentCount + 1);
    }

    public static void DecrementDegradedListenerCount(PipeServer server)
    {
        var currentCount = (int)s_degradedListenerCountFieldInfo.GetValue(server)!;
        s_degradedListenerCountFieldInfo.SetValue(server, Math.Max(0, currentCount - 1));
    }

    public static Task InvokeEnqueueShutdownBarrierAndWaitAsync(PipeServer server, TimeSpan timeout)
        => s_enqueueShutdownBarrierAndWaitAsyncDelegate(server, timeout);

    public static void InvokeEnqueueClientMessage(PipeServer server, NamedPipeMessage message, IPipeStreamWrapper wrapper)
    {
        var queueItem = s_asClientMessageMethodInfo.Invoke(null, [message, wrapper]);
        s_enqueueMessageMethodInfo.Invoke(server, [queueItem]);
    }

    public static void InvokeEnqueueServerMessage(PipeServer server, NamedPipeMessage message, IPipeStreamWrapper wrapper)
    {
        var queueItem = s_asServerMessageMethodInfo.Invoke(null, [message, wrapper]);
        s_enqueueMessageMethodInfo.Invoke(server, [queueItem]);
    }

    public static Task InvokeProcessQueueAsync(PipeServer server, CancellationToken token)
        => s_processQueueAsyncDelegate(server, token);

    public static Task InvokeReadRequestsAsync(PipeServer server, IPipeStreamWrapper wrapper, int instanceId, CancellationToken token)
        => s_readRequestsAsyncDelegate(server, wrapper, instanceId, token);

    public static void InvokeSetOperationalState(PipeServer server)
        => s_setOperationalStateDelegate(server);

    public static void SetServerState(PipeServer server, string stateName)
    {
        var enumVal = Enum.Parse(s_serverStateType, stateName);
        var raw = (int)enumVal;
        s_serverStateRawFieldInfo.SetValue(server, raw);
    }
}
