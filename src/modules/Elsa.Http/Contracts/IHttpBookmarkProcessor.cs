using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Results;
using Elsa.Workflows.State;

namespace Elsa.Http.Contracts;

/// <summary>
/// A helper service that can process <see cref="TriggerWorkflowsResult"/>s within the current HTTP context.
/// </summary>
public interface IHttpBookmarkProcessor
{
    /// <summary>
    /// Processes the specified <see cref="executionResults"/> by resuming each HTTP bookmark while we are in an HTTP context.
    /// </summary>
    Task<IEnumerable<WorkflowState>> ProcessBookmarks(
        IEnumerable<WorkflowExecutionResult> executionResults,
        string? correlationId = default,
        IDictionary<string, object>? input = default,
        CancellationTokens cancellationToken = default);
}