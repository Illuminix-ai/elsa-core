using System.Reflection;
using Elsa.Mediator.Contexts;
using Elsa.Mediator.Contracts;
using Elsa.Mediator.Middleware.Command.Contracts;

namespace Elsa.Mediator.Middleware.Command.Components;

/// <summary>
/// A command middleware that invokes the command.
/// </summary>
public class CommandHandlerInvokerMiddleware : ICommandMiddleware
{
    private static readonly MethodInfo _awaitOnGenericTaskMethod =
        typeof(CommandHandlerInvokerMiddleware).GetMethod(
            nameof(AwaitOnGenericTask),
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

    private readonly CommandMiddlewareDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<ICommandHandler> _commandHandlers;

    /// <summary>
    /// Constructor.
    /// </summary>
    public CommandHandlerInvokerMiddleware(CommandMiddlewareDelegate next, IEnumerable<ICommandHandler> commandHandlers, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _commandHandlers = commandHandlers.DistinctBy(x => x.GetType()).ToList();
    }

    /// <inheritdoc />
    public async ValueTask InvokeAsync(CommandContext context)
    {
        // Find all handlers for the specified command.
        var command = context.Command;
        var commandType = command.GetType();
        var resultType = context.ResultType;
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);
        var handlers = _commandHandlers.Where(x => handlerType.IsInstanceOfType(x)).ToArray();

        if (!handlers.Any())
            throw new InvalidOperationException($"There is no handler to handle the {commandType.FullName} command");

        if (handlers.Length > 1)
            throw new InvalidOperationException($"Multiple handlers were found to handle the {commandType.FullName} command");

        var handler = handlers.First();
        var strategyContext = new CommandStrategyContext(command, handler, _serviceProvider, context.CancellationToken);
        var strategy = context.CommandStrategy;
        var executeMethod = strategy.GetType().GetMethod(nameof(ICommandStrategy.ExecuteAsync))!;
        var executeMethodWithReturnType = executeMethod.MakeGenericMethod(resultType);

        // Execute command.
        var task = executeMethodWithReturnType.Invoke(strategy, new object[] { strategyContext });

        // Get result of task.
        // Get result of task.
        Task<object> resultingTask =
            (Task<object>)
                _awaitOnGenericTaskMethod
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new object[] { task! })!;

        context.Result = await resultingTask;

        // Invoke next middleware.
        await _next(context);
    }

    /// <summary>
    /// Simple method used to convert Task<T> to Task<object> by awaiting it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <returns></returns>
    private static async Task<object> AwaitOnGenericTask<T>(Task<T> task)
    {
        return await task;
    }
}
