using System;
using System.IO;
using System.Threading;

namespace ModManager.Services
{
    /// <summary>
    /// Lightweight watcher that monitors the Paks directory (and its three mod roots
    /// Mods / ~mods / LogicMods) for external changes made outside the app — e.g. manual
    /// creations, deletions, or folder renames performed in Windows Explorer.
    ///
    /// Raw file-system notifications are noisy (a single Explorer action can raise several
    /// events), so all activity is debounced onto a single trailing-edge callback. The
    /// <see cref="Changed"/> event is raised at most once per quiet window, letting the UI
    /// perform one asynchronous F5-style refresh instead of many.
    /// </summary>
    public sealed class ModFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly System.Threading.Timer _debounceTimer;
        private readonly int _debounceMilliseconds;
        private readonly object _sync = new();
        private int _suppressionDepth;
        private bool _disposed;

        /// <summary>
        /// Raised (on a thread-pool thread) after external file-system activity settles.
        /// Handlers must marshal to the UI thread themselves before touching controls.
        /// </summary>
        public event EventHandler? Changed;

        /// <param name="paksRoot">Absolute path of the Paks directory to watch.</param>
        /// <param name="debounceMilliseconds">Quiet window before a batch of events fires once.</param>
        public ModFileWatcher(string paksRoot, int debounceMilliseconds = 400)
        {
            if (string.IsNullOrWhiteSpace(paksRoot))
                throw new ArgumentException("Paks root path is required.", nameof(paksRoot));
            if (!Directory.Exists(paksRoot))
                throw new DirectoryNotFoundException($"Paks directory does not exist: '{paksRoot}'.");

            _debounceMilliseconds = debounceMilliseconds > 0 ? debounceMilliseconds : 1;
            _debounceTimer = new System.Threading.Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);

            _watcher = new FileSystemWatcher(paksRoot)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Size
            };

            _watcher.Created += OnFileSystemEvent;
            _watcher.Deleted += OnFileSystemEvent;
            _watcher.Renamed += OnFileSystemEvent;
            _watcher.Changed += OnFileSystemEvent;
            _watcher.Error += OnWatcherError;
        }

        /// <summary>Begins raising debounced change notifications.</summary>
        public void Start()
        {
            ThrowIfDisposed();
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>Stops notifications without disposing the underlying watcher.</summary>
        public void Stop()
        {
            if (_disposed)
                return;

            _watcher.EnableRaisingEvents = false;
            ScheduleReset(Timeout.Infinite);
        }

        /// <summary>
        /// Suppresses change notifications for the lifetime of the returned scope. Wrap the
        /// app's OWN file-system mutations (toggle, delete, rename, install) in this so our
        /// writes under the watched Paks root don't re-trigger a self-inflicted refresh —
        /// or, worse, a refresh that observes a half-completed multi-file rename.
        /// Re-entrant: nested scopes are counted; notifications resume only when the
        /// outermost scope is disposed. Any pending debounce timer is cleared on entry.
        /// </summary>
        public IDisposable SuppressNotifications()
        {
            lock (_sync)
            {
                _suppressionDepth++;
                // Drop any queued callback so events that slipped in just before suppression
                // don't fire while our own operation is running.
                _debounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            return new SuppressionScope(this);
        }

        private void EndSuppression()
        {
            lock (_sync)
            {
                if (_suppressionDepth > 0)
                    _suppressionDepth--;
            }
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            // Any activity restarts the quiet window so a burst collapses into one callback.
            ScheduleReset(_debounceMilliseconds);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            // Buffer overflow or a transient failure: coalesce into a single refresh so the
            // UI resynchronises with whatever the disk now contains.
            ScheduleReset(_debounceMilliseconds);
        }

        private void ScheduleReset(int dueTime)
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                // While our own operation is in flight, ignore the events it generates.
                if (dueTime != Timeout.Infinite && _suppressionDepth > 0)
                    return;

                _debounceTimer.Change(dueTime, Timeout.Infinite);
            }
        }

        private void OnDebounceElapsed(object? state)
        {
            lock (_sync)
            {
                // A suppression scope opened after the timer was queued: swallow this tick.
                if (_disposed || _suppressionDepth > 0)
                    return;
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModFileWatcher));
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            _watcher.Created -= OnFileSystemEvent;
            _watcher.Deleted -= OnFileSystemEvent;
            _watcher.Renamed -= OnFileSystemEvent;
            _watcher.Changed -= OnFileSystemEvent;
            _watcher.Error -= OnWatcherError;
            _watcher.Dispose();
            _debounceTimer.Dispose();
        }

        private sealed class SuppressionScope : IDisposable
        {
            private ModFileWatcher? _owner;

            public SuppressionScope(ModFileWatcher owner) => _owner = owner;

            public void Dispose()
            {
                // Guard against double-dispose so the depth counter stays balanced.
                var owner = Interlocked.Exchange(ref _owner, null);
                owner?.EndSuppression();
            }
        }
    }
}
