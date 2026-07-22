namespace HostManagement.Tests;

/// <summary>
/// Provides extension members for <see cref="Result"/> used in unit tests.
/// </summary>
internal static class ResultExtensions
{
    private const string ErrorMessage = "An error occurred.";
    private const string WarningMessage = "A warning occurred.";

    private static readonly Result s_errorResult = CreateErrorResult();
    private static readonly Result s_warningResult = CreateWarningResult();

    /// <summary>
    /// Provides extension members for the specified result.
    /// </summary>
    /// <param name="result">The result.</param>
    extension(Result result)
    {
        public static Result ErrorResult => s_errorResult;
        public static Result WarningResult => s_warningResult;
        public static string ErrorMessage => ErrorMessage;
        public static string WarningMessage => WarningMessage;
    }

    private static Result CreateErrorResult()
    {
        var result = new Result();
        result.AddError(ErrorMessage);
        return result;
    }

    private static Result CreateWarningResult()
    {
        var result = new Result();
        result.AddWarning(WarningMessage);
        return result;
    }
}
