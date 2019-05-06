using NetMq.Rpc.Contracts;
using NetMq.Rpc.Demo.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetMq.Rpc.Demo.Broker
{
    class BrokerViewModel : INotifyPropertyChanged
    {
        private IRpcMajordomo broker;
        private bool brokerIdle = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public BrokerViewModel()
        {
            StartCommand = new DelegateCommand(Start);
        }

        public void Start()
        {
            BrokerIdle = false;
            broker = new RpcMajordomo(Constants.ZeroMqEndpoint);
        }

        public ICommand StartCommand { get; private set; }

        public bool BrokerIdle
        {
            get => brokerIdle;
            set
            {
                brokerIdle = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
