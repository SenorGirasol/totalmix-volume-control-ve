﻿using System.Diagnostics.CodeAnalysis;

namespace TotalMixVC;

/// <summary>
/// Provides various useful extensions to async tasks.
/// </summary>
[SuppressMessage(
    "Usage",
    "VSTHRD003:Avoid awaiting foreign Tasks",
    Justification = "This conflicts with the accepted pattern for such task extensions."
)]
[SuppressMessage(
    "Style",
    "VSTHRD200:Use \"Async\" suffix for async methods",
    Justification = "The Async suffix is not added to conform to similar Task methods."
)]
public static class TaskExtensions
{
    /// <summary>
    /// Runs a given task but times out after a given period of time if the task does not
    /// complete.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
    /// <param name="cancellationTokenSource">
    /// A custom cancellation token source which may be cancelled by the caller before the
    /// timeout is exceeded.
    /// </param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">
    /// The task was cancelled using the provided cancellation token source.
    /// </exception>
    /// <exception cref="TimeoutException">The task timed out.</exception>
    public static async Task TimeoutAfter(
        this Task task,
        int millisecondsTimeout,
        CancellationTokenSource? cancellationTokenSource = null
    )
    {
        using var timeoutCancellationTokenSource = new CancellationTokenSource();

        // Create a list of cancellation tokens containing the timout token and optionally
        // a cancellation token provided by the caller.
        List<CancellationToken> cancellationTokens = [timeoutCancellationTokenSource.Token];

        if (cancellationTokenSource is not null)
        {
            cancellationTokens.Add(cancellationTokenSource.Token);
        }

        // Create a combined cancellation token source with all cancellation tokens and
        // build a task that will be cancelled when any of the tokens are.
        using var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            [.. cancellationTokens]
        );

        var cancellationTask = Task.Delay(
            millisecondsTimeout,
            combinedCancellationTokenSource.Token
        );

        // Wait until either the given task or the cancellation task completes and return
        // or throw exceptions appropriately.
        var completedTask = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);

        if (completedTask == cancellationTask)
        {
            if (cancellationTokenSource?.IsCancellationRequested == true)
            {
                throw new OperationCanceledException();
            }

            throw new TimeoutException();
        }

        await combinedCancellationTokenSource.CancelAsync().ConfigureAwait(false);
        await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Runs a given task but times out after a given period of time if the task does not
    /// complete.
    /// </summary>
    /// <typeparam name="TResult">The type that will be returned by the task.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
    /// <param name="cancellationTokenSource">
    /// A custom cancellation token source which may be cancelled by the caller before the
    /// timeout is exceeded.
    /// </param>
    /// <returns>
    /// The task object representing the asynchronous operation which contains the result of
    /// the task.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// The task was cancelled using the provided cancellation token source.
    /// </exception>
    /// <exception cref="TimeoutException">The task timed out.</exception>
    public static async Task<TResult> TimeoutAfter<TResult>(
        this Task<TResult> task,
        int millisecondsTimeout,
        CancellationTokenSource? cancellationTokenSource = null
    )
    {
        using var timeoutCancellationTokenSource = new CancellationTokenSource();

        // Create a list of cancellation tokens containing the timout token and optionally
        // a cancellation token provided by the caller.
        List<CancellationToken> cancellationTokens = [timeoutCancellationTokenSource.Token];

        if (cancellationTokenSource is not null)
        {
            cancellationTokens.Add(cancellationTokenSource.Token);
        }

        // Create a combined cancellation token source with all cancellation tokens and
        // build a task that will be cancelled when any of the tokens are.
        using var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            [.. cancellationTokens]
        );

        var cancellationTask = Task.Delay(
            millisecondsTimeout,
            combinedCancellationTokenSource.Token
        );

        // Wait until either the given task or the cancellation task completes and return
        // or throw exceptions appropriately.
        var completedTask = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);

        if (completedTask == cancellationTask)
        {
            if (cancellationTokenSource?.IsCancellationRequested == true)
            {
                throw new OperationCanceledException();
            }

            throw new TimeoutException();
        }

        await combinedCancellationTokenSource.CancelAsync().ConfigureAwait(false);
        return await task.ConfigureAwait(false);
    }
}
