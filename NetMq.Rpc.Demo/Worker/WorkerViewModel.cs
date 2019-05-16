using NetMq.Rpc.Contracts;
using NetMq.Rpc.Demo.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetMq.Rpc.Demo.Worker
{
    public class WorkerViewModel : INotifyPropertyChanged
    {
        private IRpcWorker<IDemoContract> worker;
        private bool workerIdle = true;
        private ObservableCollection<string> log;

        public event PropertyChangedEventHandler PropertyChanged;

        public WorkerViewModel()
        {
            StartCommand = new DelegateCommand(Start);
            log = new ObservableCollection<string>();
        }

        public void Start()
        {
            WorkerIdle = false;
            worker = new DemoWorker(Constants.ZeroMqEndpoint, new UILogger(log));
        }

        public ICommand StartCommand { get; private set; }

        public bool WorkerIdle
        {
            get => workerIdle;
            set
            {
                workerIdle = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> Log => log;

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
