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

namespace NetMq.Rpc.Demo.Client
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private IDemoContract rpcClient;
        private bool workerIdle = true;
        private ObservableCollection<string> log;

        public event PropertyChangedEventHandler PropertyChanged;

        public ClientViewModel()
        {
            RunCommand = new DelegateCommand(ExecuteRemoteCall);
            log = new ObservableCollection<string>();
        }

        public async void ExecuteRemoteCall()
        {
            if (rpcClient == null)
            {
                rpcClient = new DemoClient(Constants.ZeroMqEndpoint, new UILogger(log));
            }
            await rpcClient.AddAsync(new List<int> { 2, 2 });
        }

        public ICommand RunCommand { get; private set; }

        public IEnumerable<string> Log => log;

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
