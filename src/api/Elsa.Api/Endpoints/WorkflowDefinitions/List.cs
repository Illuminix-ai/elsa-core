using System.Threading;
using System.Threading.Tasks;
using Elsa.Persistence.Models;
using Elsa.Persistence.Services;
using Elsa.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Elsa.Api.Endpoints.WorkflowDefinitions;

public static partial class WorkflowDefinitions
{
    public static async Task<IResult> ListAsync(
        IWorkflowDefinitionStore workflowDefinitionStore,
        WorkflowSerializerOptionsProvider serializerOptionsProvider,
        CancellationToken cancellationToken,
        [FromQuery] string? versionOptions,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var serializerOptions = serializerOptionsProvider.CreateApiOptions();
        var pageArgs = new PageArgs(page, pageSize);
        var parsedVersionOptions = versionOptions != null ? VersionOptions.FromString(versionOptions) : default;
        var definitionSummaries = await workflowDefinitionStore.ListSummariesAsync(parsedVersionOptions, pageArgs, cancellationToken);

        return Results.Json(definitionSummaries, serializerOptions, statusCode: StatusCodes.Status200OK);
    }
}