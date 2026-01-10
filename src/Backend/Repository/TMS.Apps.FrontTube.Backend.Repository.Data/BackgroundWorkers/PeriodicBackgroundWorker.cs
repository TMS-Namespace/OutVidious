using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Tools;
    public class PeriodicBackgroundWorker  : IAsyncDisposable, IDisposable
    {
        private readonly TimeSpan _period;
    private readonly Func<CancellationToken, Task> _tickCallBackAsync;
    private readonly Action<Exception>? _onErrorCallBack;

    private CancellationTokenSource? _cts;
    private Task? _task;
    private int _started;

    public PeriodicBackgroundWorker(
        TimeSpan period, 
        Func<CancellationToken, Task> tickCallBackAsync, 
        Action<Exception>? onErrorCallBack = null)
    {
        _period = period;
        _tickCallBackAsync = tickCallBackAsync;
        _onErrorCallBack = onErrorCallBack;
    }

    public void Start(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
            throw new InvalidOperationException("Worker already started.");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _task = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(_period);

            try
            {
                while (await timer.WaitForNextTickAsync(_cts.Token).ConfigureAwait(false))
                    await _tickCallBackAsync(_cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _onErrorCallBack?.Invoke(ex);
                throw;
            }
        }, _cts.Token);
    }

     public async ValueTask StopAsync()
    {
        var cts = Interlocked.Exchange(ref _cts, null);
        if (cts is null)
            return;

        cts.Cancel();

        var task = Interlocked.Exchange(ref _task, null);
        if (task is not null)
            await task.ConfigureAwait(false);

        cts.Dispose();
    }

    public void Dispose()
        => StopAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
        => await StopAsync().ConfigureAwait(false);

    }
