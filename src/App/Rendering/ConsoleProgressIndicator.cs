using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine.Rendering;

using App.Extensions;
using App.Inspection;

namespace App.Rendering
{
    public class ConsoleProgressIndicator : IInspectionProgress, IAsyncDisposable
    {
        private readonly ITerminal _terminal;
        private readonly int _width;
        
        private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(500);
        private readonly Timer _thread;
        private readonly Stopwatch _stopwatch;
        private readonly CancellationTokenSource _cts;
        private readonly ManualResetEventSlim _completionBlock = new ManualResetEventSlim(false);
        
        private int _stepsTotal;
        private int _stepsIncremented;
        private string _stepTitle = "";
        
        private bool _isDisposed;
        
        public ConsoleProgressIndicator(ITerminal terminal, int width)
        {
            _terminal = terminal;
            _width = width;
            
            _cts = new CancellationTokenSource();
            _stopwatch = new Stopwatch();
            _thread = new Timer(Handle, null, Timeout.InfiniteTimeSpan, _interval);
        }

        public void Begin(int count, string title)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ConsoleProgressIndicator), "Cannot begin the progress indicator after it has been disposed.");
            }
            
            _stepsTotal = count;
            _stepsIncremented = 0;
            
            BeginTask(title);

            _terminal.HideCursor();
            
            _stopwatch.Start();
            _thread.Change(TimeSpan.Zero, _interval);
        }

        public void BeginTask(string title)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ConsoleProgressIndicator), "Cannot begin a task after the progress indicator has been disposed.");
            }
            
            Interlocked.Exchange(ref _stepTitle, title);
        }

        public void CompleteTask()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ConsoleProgressIndicator), "Cannot complete a task after the progress indicator has been disposed.");
            }

            Interlocked.Increment(ref _stepsIncremented);
        }

        public void Complete()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ConsoleProgressIndicator), "Cannot complete the progress indicator after it has been disposed.");
            }
            
            _cts.Cancel();
            _completionBlock.Wait(TimeSpan.FromSeconds(5));
            
            _terminal.ShowCursor();
        }
        
        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ConsoleProgressIndicator));
            }
            
            await _thread.DisposeAsync();

            _isDisposed = true;
        }

        private void Handle(object? state)
        {
            ClearLine();
            
            if (_cts.IsCancellationRequested)
            {
                _completionBlock.Set();
                
                return;
            }

            const string format = "[{0}/{1}] {2}{3}{4,3}% [{5:mm\\:ss}]";
            
            var progress = Math.Max(0, Math.Min(1, (double) _stepsIncremented / _stepsTotal));
            var percentage = (int) (progress * 100);
            
            var stepsTotalDigitCount = GetDigitCount(_stepsTotal);
            var stepsIncrementedDigitCount = GetDigitCount(_stepsIncremented);
            var percentageDigitCount = GetDigitCount(percentage);
            
            // Add 15 from the hard-coded characters in the format.
            // Add 10 characters buffer on the right of the windows to ensure the line isn't pushed to the next line.
            var whitespaceLength = _width - _stepTitle.Length - (stepsTotalDigitCount + stepsIncrementedDigitCount + percentageDigitCount + 15 + 10);
            var whitespace = new string(' ', whitespaceLength);

            _terminal.Write(string.Format(format, _stepsIncremented, _stepsTotal, _stepTitle, whitespace, percentage, _stopwatch.Elapsed));
        }

        private void ClearLine()
        {
            var currentLine = _terminal.CursorTop;
            
            _terminal.SetCursorPosition(0, _terminal.CursorTop);
            _terminal.Write(new string(' ', _width));
            _terminal.SetCursorPosition(0, currentLine);
        }
        
        private static int GetDigitCount(int value)
        {
            if (value >= 0)
            {
                if (value < 10) return 1;
                if (value < 100) return 2;
                if (value < 1000) return 3;
                if (value < 10000) return 4;

                return 5;
            }

            if (value > -10) return 1;
            if (value > -100) return 2;
            if (value > -1000) return 3;
            if (value > -10000) return 4;

            return 5;
        }
    }
}
