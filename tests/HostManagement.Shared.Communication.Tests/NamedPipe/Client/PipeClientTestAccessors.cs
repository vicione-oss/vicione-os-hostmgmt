using System.Collections.Concurrent;
using System.Reflection;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe;
using HostManagement.Shared.Communication.NamedPipe.Client;

namespace HostManagement.Shared.Communication.Tests.NamedPipe.Client;

internal static class PipeClientTestAccessors
{
    #region Cached reflection members
    private static readonly Type s_clientType = typeof(PipeClient);

    private static readonly FieldInfo s_expectedResponsesFieldInfo =
        s_clientType.GetField("_expectedResponses", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException("PipeClient._expectedResponses not found");

    private static readonly FieldInfo s_pipeStateRawFieldInfo =
        s_clientType.GetField("_pipeStateRaw", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException("PipeClient._pipeStateRaw not found");

    private static readonly FieldInfo s_pipeStreamWrapperFieldInfo =
        s_clientType.GetField("_pipeStreamWrapper", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException("PipeClient._pipeStreamWrapper not found");

    private static readonly MethodInfo s_processAsyncMethodInfo =
        s_clientType.GetMethod("ProcessAsync", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingMethodException("PipeClient.ProcessAsync not found");

    private static readonly FieldInfo s_processingTaskFieldInfo =
        s_clientType.GetField("_processingTask", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException("PipeClient._processingTask not found");
    #endregion

    public static ConcurrentDictionary<Guid, ExpectedResponse> GetExpectedResponses(PipeClient client)
        => (ConcurrentDictionary<Guid, ExpectedResponse>)s_expectedResponsesFieldInfo.GetValue(client)!;

    public static Task InvokeProcessAsync(PipeClient client, CancellationToken token)
        => (Task)s_processAsyncMethodInfo.Invoke(client, [token])!;

    public static void SetProcessingTask(PipeClient client, Task processingTask)
        => s_processingTaskFieldInfo.SetValue(client, processingTask);

    public static void SetStateConnected(PipeClient client)
        => s_pipeStateRawFieldInfo.SetValue(client, (int)PipeState.Connected);

    public static void SetWrapper(PipeClient client, IPipeStreamWrapper wrapper)
        => s_pipeStreamWrapperFieldInfo.SetValue(client, wrapper);
}
