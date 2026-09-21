namespace HostManagement.Tests;

/// <summary>
/// Provides extension members for <see cref="Result"/> used in unit tests.
/// </summary>
internal static class ResultExtensions
{
    private const string TestErrorMessage = "An error occurred.";
    private const string TestWarningMessage = "A warning occurred.";

    private static readonly Result s_testErrorResult = CreateErrorResult();
    private static readonly Result s_testWarningResult = CreateWarningResult();

    /// <summary>
    /// Provides extension members for the specified result.
    /// </summary>
    /// <param name="result">The result.</param>
    extension(Result result)
    {
        public static Result TestErrorResult => s_testErrorResult;
        public static Result TestWarningResult => s_testWarningResult;
        public static string TestErrorMessage => TestErrorMessage;
        public static string TestWarningMessage => TestWarningMessage;
    }

    private static Result CreateErrorResult()
    {
        var result = new Result();
        result.AddError(TestErrorMessage);
        return result;
    }

    private static Result CreateWarningResult()
    {
        var result = new Result();
        result.AddWarning(TestWarningMessage);
        return result;
    }
}
