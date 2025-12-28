using System.Collections.Concurrent;

namespace TMS.Apps.FrontTube.Backend.Core.Tools;

public sealed class ThreadSafeContainer<T>
{
    private readonly object _gate = new();
    private ConcurrentQueue<T> _queue = new();

    public void Add(T item)
    {
        lock (_gate)
        {
            _queue.Enqueue(item);
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        lock (_gate)
        {
            foreach (var item in items)
            {
                _queue.Enqueue(item);
            }
        }
    }

    /// <summary>
    /// Atomically takes everything that is in the queue at the call moment,
    /// removing them from the shared queue, and returns them for local processing.
    /// </summary>
    public List<T> ExtractAll()
    {
        ConcurrentQueue<T> batchQueue;

        lock (_gate)
        {
            batchQueue = _queue;
            _queue = new ConcurrentQueue<T>();
        }

        var batch = new List<T>();
        while (batchQueue.TryDequeue(out var item))
        {
            batch.Add(item);
        }

        return batch;
    }

    /// <summary>
    /// Thread-safe, non-destructive snapshot projection.
    /// </summary>
    public List<T> TakeSnapshot()
    {
        T[] snapshot;

        lock (_gate)
        {
            snapshot = _queue.ToArray();
        }

        return snapshot.ToList();
    }
}
