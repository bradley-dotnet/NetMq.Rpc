using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NetMq.Rpc.Demo.Utilities
{
    public class UILogger : ILogger
    {
        private ObservableCollection<string> log;

        public UILogger(ObservableCollection<string> logTarget)
        {
            log = logTarget;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                log.Add($"{logLevel.ToString()} - {message}");
            }));
        }
    }
}
