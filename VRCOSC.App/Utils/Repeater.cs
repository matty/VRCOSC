﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.Utils;

public class Repeater
{
    private readonly string name;
    private readonly Action action;
    private CancellationTokenSource? cancellationTokenSource;

    public Repeater(string name, Action action)
    {
        this.name = name;
        this.action = action;
    }

    public void Start(TimeSpan interval, bool runOnceImmediately = false)
    {
        if (cancellationTokenSource is not null) throw new InvalidOperationException($"{nameof(Repeater)}:{name} is already started");

        cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            if (runOnceImmediately)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Handle(e, $"{nameof(Repeater)}:{name} has experienced an exception");
                }
            }

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(interval, cancellationTokenSource.Token);
                    action.Invoke();
                }
                catch (TaskCanceledException)
                {
                    // Task was canceled, exit loop gracefully
                }
                catch (Exception e)
                {
                    ExceptionHandler.Handle(e, $"{nameof(Repeater)}:{name} has experienced an exception");
                }
            }
        }, cancellationTokenSource.Token);
    }

    public async Task StopAsync()
    {
        if (cancellationTokenSource is null)
            return;

        try
        {
            await cancellationTokenSource.CancelAsync();
        }
        catch (TaskCanceledException)
        {
            // Task was canceled as expected
        }

        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;
    }
}