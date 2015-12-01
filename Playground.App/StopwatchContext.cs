using Microsoft.Async.Transformations.Windows;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using static Microsoft.Async.Transformations.AsyncTransform;

namespace Playground.App
{
    public class StopwatchContext : INotifyPropertyChanged
    {
        private static readonly TimeSpan s_interval = TimeSpan.FromSeconds(1.0);

        private readonly Func<CancellationToken, Task> _tickAsync;
        private readonly Func<CancellationToken, Task> _resetAsync;

        private TimeSpan? _elapsed;

        public StopwatchContext()
        {
            _tickAsync = AsAsync(Tick).OnDispatcher();
            _resetAsync = AsAsync(Reset).OnDispatcher();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TimeSpan? Elapsed
        {
            get
            {
                return _elapsed ?? TimeSpan.Zero;
            }
            private set
            {
                _elapsed = value;
                OnPropertyChanged(nameof(Elapsed));
            }
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(s_interval, token);
                    await _tickAsync(token);
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken != token)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task ResetAsync(CancellationToken token)
        {
            await _resetAsync(token);
        }

        public async Task RestartAsync(CancellationToken token)
        {
            await ResetAsync(token);
            await RunAsync(token);
        }

        private void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Reset()
        {
            Elapsed = TimeSpan.Zero;
        }

        private void Tick()
        {
            Elapsed += s_interval;
        }
    }
}
