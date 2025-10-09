using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using unitechRFID;
using unitechRFID.Device.Params;
using unitechRFID.Diagnositics;
using unitechRFID.Params;
using unitechRFID.Reader;
using unitechRFID.Transport;
using unitechRFID.UHF.Module;
using unitechRFID.UHF.Params;
using unitechRFID.Util.Diagnotics;
using unitechRFIDSample.Model;

namespace unitechRFIDSample.ViewModels
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        bool _isOnInventory = false;

        #region UI Parameters
        #region Command
        private DelegateCommand<string> _operationCmd;
        public DelegateCommand<string> OperationCommand => _operationCmd ??= new DelegateCommand<string>(OnOperation);
        #endregion

        #region Connection Type
        public List<ItemCollection> ConnectionTypes { get; set; }
        public ItemCollection SelectedConnectionType { get; set; }


        private int _connectionTypeIndex;
        public int SelectedConnectionTypeIndex
        {
            get => _connectionTypeIndex;

            set
            {
                _connectionTypeIndex = value;
                NotifyPropertyChanged(nameof(PathInfo));
                NotifyPropertyChanged(nameof(HintText));

                BaudRateVisibility = _connectionTypeIndex.Equals(0) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public List<ItemCollection> BaudRate { get; set; }
        public ItemCollection SelectedBaudRate { get; set; }

        private int _baudRateIndex;
        public int SelectedBaudRateIndex
        {
            get => _baudRateIndex;

            set
            {
                _baudRateIndex = value;
            }
        }

        private Visibility _baudRateVisibility = Visibility.Visible;
        public Visibility BaudRateVisibility
        {
            get { return _baudRateVisibility; }
            set
            {
                if (_baudRateVisibility != value)
                {
                    _baudRateVisibility = value;
                    NotifyPropertyChanged(nameof(BaudRateVisibility));
                }
            }
        }
        #endregion

        #region Device Settings
        private ObservableCollection<string> _mode;
        public ObservableCollection<string> Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                NotifyPropertyChanged(nameof(Mode));
            }
        }

        private string _selectedMode;
        public string SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                if (_selectedMode != value)
                {
                    _selectedMode = value;

                    if (_isConnected)
                    {
                        ValueVisibility = (_selectedMode == "Set") ? Visibility.Visible : Visibility.Collapsed;
                    }

                    UpdateItemList();
                    NotifyPropertyChanged(nameof(SelectedMode));
                }
            }
        }

        private bool _firmwareUpdateEnabled = false;
        public bool FirmwareUpdateEnabled
        {
            get { return _firmwareUpdateEnabled; }
            set
            {
                if (_firmwareUpdateEnabled != value)
                {
                    _firmwareUpdateEnabled = value;
                    NotifyPropertyChanged(nameof(FirmwareUpdateEnabled));
                }
            }
        }

        private bool _executeCommandEnabled = false;
        public bool ExecuteCommandEnabled
        {
            get { return _executeCommandEnabled; }
            set
            {
                if (_executeCommandEnabled != value)
                {
                    _executeCommandEnabled = value;
                    NotifyPropertyChanged(nameof(ExecuteCommandEnabled));
                }
            }
        }

        private ObservableCollection<string> _item;
        public ObservableCollection<string> Item
        {
            get { return _item; }
            set
            {
                _item = value;
                NotifyPropertyChanged(nameof(Item));
            }
        }

        private string _selectedItem;
        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;

                    if (_selectedItem != null)
                    {
                        UpdateValueList();
                    }

                    NotifyPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        private Visibility _itemVisibilityVisibility = Visibility.Collapsed;
        public Visibility ItemVisibility
        {
            get { return _itemVisibilityVisibility; }
            set
            {
                if (_itemVisibilityVisibility != value)
                {
                    _itemVisibilityVisibility = value;
                    NotifyPropertyChanged(nameof(ItemVisibility));
                }
            }
        }

        private ObservableCollection<string> _value;
        public ObservableCollection<string> Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyPropertyChanged(nameof(Value));
            }
        }

        private string _selectedValue;
        public string SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                if (_selectedValue != value)
                {
                    _selectedValue = value;
                    NotifyPropertyChanged(nameof(SelectedValue));
                }
            }
        }

        private Visibility _valueVisibility = Visibility.Collapsed;
        public Visibility ValueVisibility
        {
            get { return _valueVisibility; }
            set
            {
                if (_valueVisibility != value)
                {
                    _valueVisibility = value;
                    NotifyPropertyChanged(nameof(ValueVisibility));
                }
            }
        }
        #endregion

        #region Bank Type
        public List<ItemCollection> BankTypes { get; set; }

        public ItemCollection SelectedBankType { get; set; }

        public int SelectedBankTypeIndex { get; set; }
        #endregion

        #region Lock Bank Type
        public List<ItemCollection> LockBankTypes { get; set; }

        public ItemCollection SelectedLockBankType { get; set; }

        public int SelectedLockBankTypeIndex { get; set; }
        #endregion

        #region Lock Type
        public List<ItemCollection> LockTypes { get; set; }

        public ItemCollection SelectedLockType { get; set; }

        public int SelectedLockTypeIndex { get; set; }
        #endregion

        #region UI Text
        private string _connectText;
        public string ConnectText
        {
            get => _connectText;
            set
            {
                _connectText = value;
                NotifyPropertyChanged(nameof(ConnectText));
            }
        }

        private string _inventoryText;
        public string InventoryText
        {
            get => _inventoryText;
            set
            {
                _inventoryText = value;
                NotifyPropertyChanged(nameof(InventoryText));
            }
        }

        public string PathInfo
        {
            get
            {
                return SelectedConnectionTypeIndex == 0 ? IConstValue.Path : IConstValue.MACAddress;
            }
        }

        public string HintText
        {
            get
            {
                if (SelectedConnectionTypeIndex == 0)
                {
                    return "Ex: COM3";
                }
                else
                {
                    return "Ex: AABBCCDDEEFF";
                }
            }
        }
        #endregion

        #region Device Info
        public string Name { get; set; }
        public string Version { get; set; }
        public string Bluetooth { get; set; }
        public string Battery { get; set; }
        public string Temperature { get; set; }
        public string KeyStatus { get; set; }
        public string ConnectStatus { get; set; }
        public string ActionStateText { get; set; }
        #endregion

        #region Tag Info
        public string EPC { get; set; }
        public string TID { get; set; }
        public string RSSI { get; set; }
        public string AntennaID { get; set; }
        #endregion

        #region Operation Info
        public string Process { get; set; }

        public string Offset { get; set; }
        public string Length { get; set; }

        public string Data { get; set; }
        public string Result { get; set; }

        public string AccessPassword { get; set; }

        public string Text { get; set; }

        public string LockPassword { get; set; }
        public string KillPassword { get; set; }
        #endregion


        //<timmy> for UI control (但這不符合WPF MVVM原則)
        public MainWindow mainWindow;

        //<Timmy> bind list box for found tags
        public ObservableCollection<string> RfidTags { get; set; } = new ObservableCollection<string>();
        //public ObservableCollection<string> RfidTags  =
        //    new ObservableCollection<string>();
        //public List<string> RfidTags { get; set; } = new List<string>();

        public string ConnectPath { get; set; }
        //::<timmy> PS.在XAML中TextBox等UI元件bind屬性 

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                ConnectText = _isConnected ? IConstValue.Disconnect : IConstValue.Connect;
                NotifyPropertyChanged(nameof(IsConnected));
                NotifyPropertyChanged(nameof(IsDisconnected));
                NotifyPropertyChanged(nameof(IsFindable));
            }
        }

        public bool IsInventoried => IsConnected && !StringUtil.IsNullOrEmpty(EPC) && ActionStateText.Equals(ActionState.Stop.ToString());

        private bool _displayEnabled = false;
        public bool DisplayEnabled
        {
            get { return _displayEnabled; }
            set
            {
                _displayEnabled = value;
                NotifyPropertyChanged(nameof(DisplayEnabled));
            }
        }

        public bool IsDisconnected => !IsConnected;

        public bool IsFindable
        {
            get
            {
                if (IsConnected)
                {
                    var deviceType = _reader.DeviceType;

                    switch (deviceType)
                    {
                        case DeviceType.RM300P:
                            return false;
                        default:
                            break;
                    }

                    if (StringUtil.IsNullOrEmpty(ActionStateText))
                    {
                        return true;
                    }
                    
                    if (ActionStateText.Equals(ActionState.Stop.ToString()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        UHFReader _reader;
        BaseTransport _baseTransport;
        DeviceType _detectDeviceType;

        public MainViewModel()
        {
            InitUI();

            UHFReader.LogLevel = LogLevel.Info;
            UHFReader.LogEvent += OnLogEvent;
        }

        private void OnLogEvent(object sender, LogEventArg e)
        {
            Debug.WriteLine($"[{e.Tag}] {e.Log}");
        }

        private void InitUI()
        {
            ConnectText = IConstValue.Connect;
            InventoryText = IConstValue.Inventory;

            //::set default COM <timmy>
            ConnectPath = "COM3";

            ConnectionTypes = new List<ItemCollection>();
            ConnectionTypes.Add(new ItemCollection(IConstValue.Serial));
            ConnectionTypes.Add(new ItemCollection(IConstValue.Bluetooth));

            BaudRate = new List<ItemCollection>();
            BaudRate.Add(new ItemCollection("115200"));
            BaudRate.Add(new ItemCollection("460800"));
            BaudRate.Add(new ItemCollection("921600"));

            Mode = new ObservableCollection<string>
            {
                IConstValue.Get,
                IConstValue.Set
            };

            SelectedMode = Mode[0];

            BankTypes = new List<ItemCollection>();
            BankTypes.Add(new ItemCollection(IConstValue.Reserved));
            BankTypes.Add(new ItemCollection(IConstValue.EPC));
            BankTypes.Add(new ItemCollection(IConstValue.TID));
            BankTypes.Add(new ItemCollection(IConstValue.User));

            LockBankTypes = new List<ItemCollection>();
            LockBankTypes.Add(new ItemCollection(IConstValue.AccessPassword));
            LockBankTypes.Add(new ItemCollection(IConstValue.KillPassword));
            LockBankTypes.Add(new ItemCollection(IConstValue.EPC));
            LockBankTypes.Add(new ItemCollection(IConstValue.TID));
            LockBankTypes.Add(new ItemCollection(IConstValue.User));

            LockTypes = new List<ItemCollection>();
            LockTypes.Add(new ItemCollection(IConstValue.Unlock));
            LockTypes.Add(new ItemCollection(IConstValue.Lock));
            LockTypes.Add(new ItemCollection(IConstValue.PermaLock));

            SelectedBankTypeIndex = 1;
            SelectedLockBankTypeIndex = 1;
            SelectedLockTypeIndex = 1;
            Offset = IConstValue.DefaultOffset;
            Length = IConstValue.DefaultLength;

            AccessPassword = IConstValue.DefaultPassword;
            LockPassword = IConstValue.DefaultPassword;
            KillPassword = IConstValue.DefaultPassword;

            //<timmy>
            //RfidTags = new ObservableCollection<string>();
            //RfidTags.Add("Start test" + DateTime.Now);

            NotifyPropertyChanged(nameof(Offset));
            NotifyPropertyChanged(nameof(Length));
            NotifyPropertyChanged(nameof(AccessPassword));
            NotifyPropertyChanged(nameof(LockPassword));
            NotifyPropertyChanged(nameof(KillPassword));
        }

        private void ResetInfoUI()
        {
            Name = string.Empty;
            Version = string.Empty;
            Bluetooth = string.Empty;
            Battery = string.Empty;
            Temperature = string.Empty;
            KeyStatus = string.Empty;
            //ConnectStatus = string.Empty;
            ActionStateText = string.Empty;

            NotifyPropertyChanged(nameof(Name));
            NotifyPropertyChanged(nameof(Version));
            NotifyPropertyChanged(nameof(Bluetooth));
            NotifyPropertyChanged(nameof(Battery));
            NotifyPropertyChanged(nameof(Temperature));
            NotifyPropertyChanged(nameof(KeyStatus));
            //NotifyPropertyChanged(nameof(ConnectStatus));
            NotifyPropertyChanged(nameof(ActionStateText));
        }

        private void InitReader()
        {
            if (_reader != null)
            {
                _reader.ActionStateChangedEvent += OnActionStateChanged;
                _reader.ConnectStateChangedEvent += OnConnectStateChangedEvent;

                _reader.TemperatureEvent += OnTemperatureEvent;
                switch (_reader.DeviceType)
                {
                    case DeviceType.RM300P:
                        break;
                    default:
                        _reader.BatteryStateEvent += OnBatteryEvent;
                        _reader.KeyStateEvent += OnKeyEvent;
                        break;
                }
            }
        }

        private void InitUHF()
        {
            if (_reader != null && _reader.BaseUHF != null)
            {
                RFIDConfig rfidConfig = new RFIDConfig()
                {
                    ContinuousMode = true,
                    Power = 24,
                    Algorithm = AlgorithmType.DynamicQ,
                    StartQ = 4,
                    MinQ = 0,
                    MaxQ = 15,
                    Session = Session.S0,
                    Encoding = unitechRFID.Encoding.FM0,
                    Target = Target.A,
                    InventoryTime = 200,
                    IdleTime = 0
                };

                _reader.BaseUHF.RFIDConfig = rfidConfig;

                switch (_reader.DeviceType)
                {
                    case DeviceType.RM300P:
                        _reader.BaseUHF.ProgressRateEvent += OnProgressRateEvent;
                        break;
                    default:
                        break;
                }
            }
        }

        public void OnOperation(string param)
        {
            Show(param);
            if (param.Equals(IConstValue.Connect)) { OnConnect(); }
            else if (param.Equals(IConstValue.Disconnect)) { OnDisconnect(); }
            else if (param.Equals(IConstValue.FactoryReset)) { OnFactoryReset(); }
            else if (param.Equals(IConstValue.FirmwareUpdate)) { OnFirmwareUpdate(); }
            else if (param.Equals(IConstValue.ExecuteCommand)) { OnExecuteCommand(); }
            else if (param.Equals(IConstValue.Display)) { OnDisplay(); }
            else if (param.Equals(IConstValue.Inventory)) { OnInventory(); }
            else if (param.Equals(IConstValue.Stop)) { OnStop(); }
            else if (param.Equals(IConstValue.Read)) { OnRead(); }
            else if (param.Equals(IConstValue.Write)) { OnWrite(); }
            else if (param.Equals(IConstValue.Lock)) { OnLock(); }
            else if (param.Equals(IConstValue.Kill)) { OnKill(); }
            else if (param.Equals(IConstValue.Find)) { OnFind(); }
            else if (param.Equals("Clear"))
            {
                //<timmy>
                RfidTags.Clear();
                mainWindow.ButtonClear.Content = "Clear Result " + RfidTags.Count().ToString();
            }
        }

        private void OnConnect()
        {
            if (_reader == null & !string.IsNullOrEmpty(ConnectPath))
            {
                if (SelectedConnectionType.Name.Equals(IConstValue.Serial))
                {
                    //Detect device
                    _baseTransport = new TransportSerial(DeviceType.Unknown, ConnectPath, int.Parse(SelectedBaudRate.Name));
                    _reader = new UHFReader(_baseTransport);

                    _reader.DetectDeviceEvent += OnDetectDevice;
                    _reader.DetectFinishedEvent += OnDetectFinished;

                    _detectDeviceType = DeviceType.Unknown;
                    _reader.StartDetect();
                }
                else
                {
                    _baseTransport = new TransportBluetooth(DeviceType.RP902, ConnectPath);
                    _reader = new UHFReader(_baseTransport);

                    InitReader();

                    _reader.Connect();
                }

                if (_reader.DeviceType != DeviceType.RM300P) { _reader.DisplayTags = new DisplayTags(ReadOnceState.Off, BeepAndVibrateState.On); }  
            }
        }

        private void OnDisconnect()
        {
            OnStop();
            _reader.Disconnect();
        }

        #region Device Settings
        private void OnFactoryReset()
        {
            try
            {
                if (MessageBox.Show("Are you ready to FactoryReset Device?", "FactoryReset", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _reader.FactoryReset();
                }
            }
            catch
            {
                Show($"Set FactoryReset Command failed!");
            }
        }

        private void OnFirmwareUpdate()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "Binary Files (*.bin)|*.bin"
                };

                if (dialog.ShowDialog() == true)
                {
                    Task.Run(() =>_reader.FirmwareUpdate(dialog.FileName));
                }
            }
            catch
            {
                throw;
            }
        }

        private void OnExecuteCommand()
        {
            Result = string.Empty;

            if (_selectedMode == IConstValue.Get)
            {
                try
                {
                    switch (_reader.DeviceType)
                    {
                        case DeviceType.RP902:
                            {
                                var getCommands = new Dictionary<object, Func<string>>
                                {
                                    { IConstValue.DeviceName, () => _reader.DeviceName },
                                    { IConstValue.SerialNumber, () => _reader.SerialNumber },
                                    { IConstValue.FirmwareVersion, () => _reader.Version },
                                    { IConstValue.BluetoothMAC, () => _reader.MacAddress },
                                    { IConstValue.SKU, () => _reader.SKU.ToString() },
                                    { IConstValue.Battery, () => _reader.BatteryState.ToString() },
                                    { IConstValue.Temperature, () => _reader.Temperature.ToString() },

                                    { IConstValue.GlobalBand, () => _reader.BaseUHF.GlobalBand.ToString() },
                                    { IConstValue.Beeper, () => _reader.Beeper.ToString() },
                                    { IConstValue.Vibrator, () => _reader.Vibrator.ToString() },
                                    { IConstValue.AutoScreenOffTime, () => _reader.ScreenOffTime.ToString() },
                                    { IConstValue.AutoPowerOffTime, () => _reader.AutoOffTime.ToString() },
                                    { IConstValue.OperationMode, () => _reader.OperatingMode.ToString() },
                                    { IConstValue.ReadMode, () => _reader.ReadMode.ToString() },
                                    { IConstValue.DataFormat, () => _reader.DataFormat.ToString() },
                                    { IConstValue.DataTerminator, () => _reader.DataTerminator.ToString() },
                                    { IConstValue.DataInfo, () => _reader.DataInfo.ToString() },

                                    { IConstValue.Power, () => _reader.BaseUHF.Power.ToString() },
                                    { IConstValue.InventoryTime, () => _reader.BaseUHF.InventoryTime.ToString() },
                                    { IConstValue.IdleTime, () => _reader.BaseUHF.IdleTime.ToString() },
                                    { IConstValue.Target, () => _reader.BaseUHF.Target.ToString() },
                                    { IConstValue.Session, () => _reader.BaseUHF.Session.ToString() },
                                    { IConstValue.Encoding, () => _reader.BaseUHF.Encoding.ToString() },
                                    { IConstValue.Algorithm, () => _reader.BaseUHF.AlgorithmType.ToString() },
                                    { IConstValue.StartQ, () => _reader.BaseUHF.StartQ.ToString() },
                                    { IConstValue.MaxQ, () => _reader.BaseUHF.MaxQ.ToString() },
                                    { IConstValue.MinQ, () => _reader.BaseUHF.MinQ.ToString() },
                                    { IConstValue.TARI, () => _reader.BaseUHF.TARI.ToString() },
                                    { IConstValue.BLF, () => _reader.BaseUHF.BLF.ToString() },
                                    { IConstValue.ToggleTarget, () => _reader.BaseUHF.ToggleTarget.ToString() },
                                    { IConstValue.FastMode, () => _reader.BaseUHF.FastMode.ToString() }
                                };

                                if (getCommands.TryGetValue(SelectedItem, out var getValue))
                                {
                                    Result = getValue();
                                }
                            }
                            break;
                        case DeviceType.RM300P:
                            {
                                var getCommands = new Dictionary<object, Func<string>>
                                {
                                    { IConstValue.DeviceName, () => _reader.ModelName },
                                    { IConstValue.SerialNumber, () => _reader.SerialNumber },
                                    { IConstValue.FirmwareVersion, () => _reader.Version },
                                    { IConstValue.SKUID, () => _reader.SKUID.ToString() },
                                    { IConstValue.Temperature, () => _reader.Temperature.ToString() },

                                    { IConstValue.Region, () => _reader.Region.ToString() },
                                    { IConstValue.AmbientTempProtection, () => (int.Parse(_reader.AmbientTempProtection.ToString()) / 10.0).ToString() },
                                    { IConstValue.PATempProtection, () => (int.Parse(_reader.PATempProtection.ToString()) / 10.0).ToString() },
                                    { IConstValue.UartBaudRate, () => _reader.UartBaudRate.ToString() },
                                    { IConstValue.CfgStorageMode, () => _reader.CfgStorageMode.ToString() },
                                    { IConstValue.GpioPinsConfig, () =>
                                        {
                                            string gpio2 = _reader.GetGpioPinsConfig(2).ToString();
                                            string gpio3 = _reader.GetGpioPinsConfig(3).ToString();
                                            string gpio4 = _reader.GetGpioPinsConfig(4).ToString();
                                            return $"GPIO 2: {gpio2}, 3: {gpio3}, 4: {gpio4}";
                                        }
                                    },
                                    { IConstValue.GpioPinsState, () =>
                                        {
                                            string gpio2 = _reader.GetGpioPinsState(2).ToString();
                                            string gpio3 = _reader.GetGpioPinsState(3).ToString();
                                            string gpio4 = _reader.GetGpioPinsState(4).ToString();
                                            return $"GPIO 2: {gpio2}, 3: {gpio3}, 4: {gpio4}";
                                        }
                                    },
                                    { IConstValue.BiStaticAntenna, () => _reader.BiStaticAntenna.ToString() },
                                    { IConstValue.AntennaState, () =>
                                        {
                                            var antenna1 = _reader.GetAntennaState(1);
                                            var antenna2 = _reader.GetAntennaState(2);
                                            var antenna3 = _reader.GetAntennaState(3);
                                            var antenna4 = _reader.GetAntennaState(4);
                                            return $"Antenna 1: {antenna1.State}, 2: {antenna2.State}, 3: {antenna3.State}, 4: {antenna4.State}";
                                        }
                                    },
                                    { IConstValue.AntennaSetting, () =>
                                        {
                                            var antenna1 = _reader.GetAntennaSetting(1);
                                            var antenna2 = _reader.GetAntennaSetting(2);
                                            var antenna3 = _reader.GetAntennaSetting(3);
                                            var antenna4 = _reader.GetAntennaSetting(4);
                                            return $"Antenna 1: {antenna1.DwellTime}ms {antenna1.PowerLevel / 100}dbm, 2: {antenna2.DwellTime}ms {antenna2.PowerLevel / 100}dbm, 3: {antenna3.DwellTime}ms {antenna3.PowerLevel / 100}dbm, 4: {antenna4.DwellTime}ms {antenna4.PowerLevel / 100}dbm";
                                        }
                                    },
                                    { IConstValue.LbtSetting, () => _reader.LbtSetting.ToString() },

                                    { IConstValue.Session, () => _reader.BaseUHF.Session.ToString() },
                                    { IConstValue.Target, () => _reader.BaseUHF.Target.ToString() },
                                    { IConstValue.Algorithm, () => _reader.Algorithm.AlgorithmSingulation.ToString() },
                                    { IConstValue.StartQ, () => _reader.Algorithm.InitQ.ToString() },
                                    { IConstValue.MaxQ, () => _reader.Algorithm.MaxQ.ToString() },
                                    { IConstValue.MinQ, () => _reader.Algorithm.MinQ.ToString() },
                                    { IConstValue.ToggleTarget, () => _reader.Algorithm.DualTarget.ToString() },
                                    { IConstValue.RfMode, () => _reader.RfMode.ToString() },
                                    { IConstValue.TagFocus, () => _reader.TagFocus.ToString() },
                                    { IConstValue.FastId, () => _reader.FastId.ToString() },
                                    { IConstValue.ContinuousMode, () => _reader.OperationMode.ToString() },
                                    { IConstValue.LbtCfgSetting, () => (int.Parse(((short)_reader.LbtCfgSetting).ToString()) / 100).ToString() },
                                };

                                if (getCommands.TryGetValue(SelectedItem, out var getValue))
                                {
                                    Result = getValue();
                                }
                            }
                            break;
                    }                   
                }
                catch
                {
                    Result = "Get Device Parameter Failed!";
                }
            }
            else
            {
                try
                {
                    Result = ResultCode.NoError.ToString();

                    switch (_reader.DeviceType)
                    {
                        case DeviceType.RP902:
                            {
                                var setCommands = new Dictionary<object, Action<string>>
                                {
                                    { IConstValue.GlobalBand, value => _reader.BaseUHF.GlobalBand = Enum.Parse<GlobalBandType>(value) },
                                    { IConstValue.Beeper, value => _reader.Beeper = Enum.Parse<BeeperState>(value) },
                                    { IConstValue.Vibrator, value => _reader.Vibrator = Enum.Parse<VibratorState>(value) },
                                    { IConstValue.AutoScreenOffTime, value => {
                                        ScreenOffTime screenOffTime = new ScreenOffTime();
                                        switch (SelectedValue)
                                        {
                                            case "0.5":
                                                screenOffTime = new ScreenOffTime(0, 30);
                                                break;
                                            case "0":
                                            case "1":
                                            case "2":
                                            case "3":
                                            case "5":
                                            case "10":
                                                screenOffTime = new ScreenOffTime((byte)int.Parse(SelectedValue), 0);
                                                break;
                                        }
                                        _reader.ScreenOffTime = screenOffTime;
                                    }},
                                    { IConstValue.AutoPowerOffTime, value => _reader.AutoOffTime = int.Parse(value) },
                                    { IConstValue.OperationMode, value => _reader.OperatingMode = Enum.Parse<OperatingMode>(value) },
                                    { IConstValue.ReadMode, value => _reader.ReadMode = Enum.Parse<ReadMode>(value) },
                                    { IConstValue.DataFormat, value => _reader.DataFormat = Enum.Parse<unitechRFID.DataFormat>(value) },
                                    { IConstValue.DataTerminator, value => _reader.DataTerminator = Enum.Parse<DataTerminator>(value) },
                                    { IConstValue.DataInfo, value => _reader.DataInfo = Enum.Parse<DataInfo>(value) },

                                    { IConstValue.Power, value => _reader.BaseUHF.Power = int.Parse(value) },
                                    { IConstValue.InventoryTime, value => _reader.BaseUHF.InventoryTime = int.Parse(value) },
                                    { IConstValue.IdleTime, value => _reader.BaseUHF.IdleTime = int.Parse(value) },
                                    { IConstValue.Target, value => _reader.BaseUHF.Target = Enum.Parse<Target>(value) },
                                    { IConstValue.Session, value => _reader.BaseUHF.Session = Enum.Parse<Session>(value) },
                                    { IConstValue.Encoding, value => _reader.BaseUHF.Encoding = Enum.Parse<unitechRFID.Encoding>(value) },
                                    { IConstValue.Algorithm, value => _reader.BaseUHF.AlgorithmType = Enum.Parse<AlgorithmType>(value) },
                                    { IConstValue.StartQ, value => _reader.BaseUHF.StartQ = int.Parse(value) },
                                    { IConstValue.MaxQ, value => _reader.BaseUHF.MaxQ = int.Parse(value) },
                                    { IConstValue.MinQ, value => _reader.BaseUHF.MinQ = int.Parse(value) },
                                    { IConstValue.TARI, value => _reader.BaseUHF.TARI = Enum.Parse<TARIType>(value) },
                                    { IConstValue.BLF, value => _reader.BaseUHF.BLF = Enum.Parse<BLFType>(value) },
                                    { IConstValue.ToggleTarget, value => _reader.BaseUHF.ToggleTarget = value == "On" },
                                    { IConstValue.FastMode, value => _reader.BaseUHF.FastMode = value == "On" }
                                };

                                if (setCommands.TryGetValue(SelectedItem, out var setValue))
                                {
                                    setValue(SelectedValue);
                                }
                            }
                            break;
                        case DeviceType.RM300P:
                            {
                                var setCommands = new Dictionary<object, Action<string>>
                                {
                                    { IConstValue.Region, value => _reader.Region = Enum.Parse<Region>(value) },
                                    { IConstValue.AmbientTempProtection, value => _reader.AmbientTempProtection = (ushort)(int.Parse(value) * 10) },
                                    { IConstValue.PATempProtection, value => _reader.PATempProtection = (ushort)(int.Parse(value) * 10) },
                                    { IConstValue.UartBaudRate, value => _reader.UartBaudRate = uint.Parse(value) },
                                    { IConstValue.CfgStorageMode, value => _reader.CfgStorageMode = Enum.Parse<CfgStorageMode>(value) },
                                    { IConstValue.GpioPinsConfig, value => {
                                        var config = value.Split(',')
                                                          .Select(s => s.Trim())
                                                          .ToArray();

                                        _reader.SetGpioPinsConfig(int.Parse(config[0]), Enum.Parse<GpioPinDirection>(config[1]));
                                    }},
                                    { IConstValue.GpioPinsState, value => {
                                        var state = value.Split(',')
                                                         .Select(s => s.Trim())
                                                         .ToArray();

                                        _reader.SetGpioPinsState(int.Parse(state[0]), Enum.Parse<GpioPinState>(state[1]));
                                    }},
                                    { IConstValue.BiStaticAntenna, value => _reader.BiStaticAntenna = Enum.Parse<BiStaticAntennaSetting>(value) },
                                    { IConstValue.AntennaState, value => {
                                        var state = value.Split(',')
                                                         .Select(s => s.Trim())
                                                         .ToArray();

                                        unitechRFID.Device.Params.AntennaState parms = new unitechRFID.Device.Params.AntennaState((byte)int.Parse(state[0]), Enum.Parse<unitechRFID.AntennaState>(state[1]));

                                        _reader.SetAntennaState(parms);
                                    }},
                                    { IConstValue.AntennaSetting, value => {
                                        var setting = value.Split(',')
                                                           .Select(s => s.Trim())
                                                           .ToArray();

                                        AntennaSetting parms = new AntennaSetting((byte)int.Parse(setting[0]), ushort.Parse(setting[1]), ushort.Parse((int.Parse(setting[2]) * 100).ToString()));
 
                                        _reader.SetAntennaSetting(parms);
                                    }},
                                    { IConstValue.LbtSetting, value => _reader.LbtSetting = Enum.Parse<LbtSetting>(value) },

                                    { IConstValue.Session, value => _reader.BaseUHF.Session = Enum.Parse<Session>(value) },
                                    { IConstValue.Target, value => _reader.BaseUHF.Target = Enum.Parse<Target>(value) },
                                    { IConstValue.Algorithm, value => {
                                        Algorithm parms = _reader.Algorithm;
                                        parms.AlgorithmSingulation = Enum.Parse<AlgorithmType>(value);

                                        _reader.Algorithm = parms;
                                    }},
                                    { IConstValue.StartQ, value => {
                                        Algorithm parms = _reader.Algorithm;
                                        parms.InitQ = (byte)int.Parse(value);

                                        _reader.Algorithm = parms;
                                    }},
                                    { IConstValue.MaxQ, value => {
                                        Algorithm parms = _reader.Algorithm;
                                        parms.MaxQ = (byte)int.Parse(value);

                                        _reader.Algorithm = parms;
                                    }},
                                    { IConstValue.MinQ, value => {
                                        Algorithm parms = _reader.Algorithm;
                                        parms.MinQ = (byte)int.Parse(value);

                                        _reader.Algorithm = parms;
                                    }},
                                    { IConstValue.ToggleTarget, value => {
                                        Algorithm parms = _reader.Algorithm;
                                        parms.DualTarget = Enum.Parse<AlgorithmDualTarget>(value);

                                        _reader.Algorithm = parms;
                                    }},
                                    { IConstValue.RfMode, value => _reader.RfMode = Enum.Parse<RfMode>(value) },
                                    { IConstValue.TagFocus, value => _reader.TagFocus = Enum.Parse<TagFocusSetting>(value) },
                                    { IConstValue.FastId, value => _reader.FastId = Enum.Parse<FastIdSetting>(value) },
                                    { IConstValue.ContinuousMode, value => _reader.OperationMode = Enum.Parse<OperationMode>(value) },
                                    { IConstValue.LbtCfgSetting, value => _reader.LbtCfgSetting = (ushort)(int.Parse(value) * 100) }
                                };

                                if (setCommands.TryGetValue(SelectedItem, out var setValue))
                                {
                                    setValue(SelectedValue);
                                }
                            }
                            break;
                    }                            
                }
                catch (ReaderException e)
                {
                    Result = e.ResultCode.ToString();
                }
            }

            NotifyPropertyChanged(nameof(Result));
        }

        private void UpdateItemList()
        {
            if (_reader == null) { return; }

            if (Item == null) { Item = new ObservableCollection<string>(); }
            Item.Clear();

            switch (_reader.DeviceType)
            {
                case DeviceType.RP902:
                    if (_selectedMode == IConstValue.Get)
                    {
                        Item.Add(IConstValue.DeviceName);
                        Item.Add(IConstValue.SerialNumber);
                        Item.Add(IConstValue.FirmwareVersion);
                        Item.Add(IConstValue.BluetoothMAC);
                        Item.Add(IConstValue.SKU);
                        Item.Add(IConstValue.Battery);
                        Item.Add(IConstValue.Temperature);
                    }

                    Item.Add(IConstValue.GlobalBand);
                    Item.Add(IConstValue.Beeper);
                    Item.Add(IConstValue.Vibrator);
                    Item.Add(IConstValue.AutoScreenOffTime);
                    Item.Add(IConstValue.AutoPowerOffTime);
                    Item.Add(IConstValue.OperationMode);
                    Item.Add(IConstValue.ReadMode);
                    Item.Add(IConstValue.DataFormat);
                    Item.Add(IConstValue.DataTerminator);
                    Item.Add(IConstValue.DataInfo);

                    Item.Add(IConstValue.Power);
                    Item.Add(IConstValue.InventoryTime);
                    Item.Add(IConstValue.IdleTime);
                    Item.Add(IConstValue.Target);
                    Item.Add(IConstValue.Session);
                    Item.Add(IConstValue.Encoding);
                    Item.Add(IConstValue.Algorithm);
                    Item.Add(IConstValue.StartQ);
                    Item.Add(IConstValue.MaxQ);
                    Item.Add(IConstValue.MinQ);
                    Item.Add(IConstValue.TARI);
                    Item.Add(IConstValue.BLF);
                    Item.Add(IConstValue.ToggleTarget);
                    Item.Add(IConstValue.FastMode);
                    break;
                case DeviceType.RM300P:
                    if (_selectedMode == IConstValue.Get)
                    {
                        Item.Add(IConstValue.DeviceName);
                        Item.Add(IConstValue.SerialNumber);
                        Item.Add(IConstValue.FirmwareVersion);
                        Item.Add(IConstValue.SKUID);
                        Item.Add(IConstValue.Temperature);
                    }

                    Item.Add(IConstValue.Region);
                    Item.Add(IConstValue.AmbientTempProtection);
                    Item.Add(IConstValue.PATempProtection);
                    Item.Add(IConstValue.UartBaudRate);
                    Item.Add(IConstValue.CfgStorageMode);
                    Item.Add(IConstValue.GpioPinsConfig);
                    Item.Add(IConstValue.GpioPinsState);
                    Item.Add(IConstValue.BiStaticAntenna);
                    Item.Add(IConstValue.AntennaState);
                    Item.Add(IConstValue.AntennaSetting);
                    Item.Add(IConstValue.LbtSetting);

                    Item.Add(IConstValue.Session);
                    Item.Add(IConstValue.Target);
                    Item.Add(IConstValue.Algorithm);
                    Item.Add(IConstValue.StartQ);
                    Item.Add(IConstValue.MaxQ);
                    Item.Add(IConstValue.MinQ);
                    Item.Add(IConstValue.ToggleTarget);
                    Item.Add(IConstValue.RfMode);
                    Item.Add(IConstValue.TagFocus);
                    Item.Add(IConstValue.FastId);
                    Item.Add(IConstValue.ContinuousMode);
                    Item.Add(IConstValue.LbtCfgSetting);
                    break;
            }

            SelectedItem = Item[0];
        }

        private void UpdateValueList()
        {
            if (_reader == null) { return; }

            if (Value == null) { Value = new ObservableCollection<string>(); }
            Value.Clear();

            if (_selectedMode == IConstValue.Set)
            {
                switch (SelectedItem)
                {
                    case IConstValue.GlobalBand:
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            var supportedGlobalBand = _reader.BaseUHF.SupportedGlobalBand;
                            foreach (var band in supportedGlobalBand)
                            {
                                Value.Add(band.ToString());
                            }
                        });
                        break;
                    case IConstValue.Beeper:
                        Value.Add(BeeperState.Off.ToString());
                        Value.Add(BeeperState.Low.ToString());
                        Value.Add(BeeperState.Medium.ToString());
                        Value.Add(BeeperState.High.ToString());
                        break;
                    case IConstValue.Vibrator:
                    case IConstValue.ToggleTarget:
                    case IConstValue.FastMode:
                    case IConstValue.BiStaticAntenna:
                    case IConstValue.LbtSetting:
                    case IConstValue.TagFocus:
                    case IConstValue.FastId:
                        Value.Add("Off");
                        Value.Add("On");
                        break;
                    case IConstValue.AutoScreenOffTime:
                        Value.Add("0");
                        Value.Add("0.5");
                        Value.Add("1");
                        Value.Add("2");
                        Value.Add("3");
                        Value.Add("5");
                        Value.Add("10");
                        break;
                    case IConstValue.AutoPowerOffTime:
                        Value.Add("0");
                        Value.Add("1");
                        Value.Add("2");
                        Value.Add("3");
                        Value.Add("5");
                        Value.Add("10");
                        break;
                    case IConstValue.OperationMode:
                        Value.Add(OperatingMode.USBSPP.ToString());
                        Value.Add(OperatingMode.BTSPP.ToString());
                        Value.Add(OperatingMode.BTHID.ToString());
                        Value.Add(OperatingMode.Buffer.ToString());
                        Value.Add(OperatingMode.BLEHID.ToString());
                        break;
                    case IConstValue.ReadMode:
                        Value.Add(ReadMode.SingleRead.ToString());
                        Value.Add(ReadMode.MultiRead.ToString());
                        Value.Add(ReadMode.CounterRead.ToString());
                        Value.Add(ReadMode.RapidRead.ToString());
                        break;
                    case IConstValue.DataFormat:
                        Value.Add(unitechRFID.DataFormat.ASCII.ToString());
                        Value.Add(unitechRFID.DataFormat.HEX.ToString());
                        break;
                    case IConstValue.DataTerminator:
                        Value.Add(DataTerminator.None.ToString());
                        Value.Add(DataTerminator.CR_LF.ToString());
                        Value.Add(DataTerminator.Tab.ToString());
                        break;
                    case IConstValue.DataInfo:
                        Value.Add(DataInfo.EPC.ToString());
                        Value.Add(DataInfo.TID.ToString());
                        Value.Add(DataInfo.User.ToString());
                        break;
                    case IConstValue.Power:
                        for (int i = 11; i <= 27; i++)
                        {
                            Value.Add(i.ToString());
                        }
                        break;
                    case IConstValue.InventoryTime:
                    case IConstValue.IdleTime:
                        for (int i = 0; i <= 400; i++)
                        {
                            Value.Add(i.ToString());
                        }
                        break;
                    case IConstValue.Target:
                        Value.Add(Target.A.ToString());
                        Value.Add(Target.B.ToString());
                        break;
                    case IConstValue.Session:
                        Value.Add(Session.S0.ToString());
                        Value.Add(Session.S1.ToString());
                        Value.Add(Session.S2.ToString());
                        Value.Add(Session.S3.ToString());
                        break;
                    case IConstValue.Encoding:
                        Value.Add(Encoding.FM0.ToString());
                        Value.Add(Encoding.M2.ToString());
                        Value.Add(Encoding.M4.ToString());
                        Value.Add(Encoding.M8.ToString());
                        break;
                    case IConstValue.Algorithm:
                        Value.Add(AlgorithmType.FixedQ.ToString());
                        Value.Add(AlgorithmType.DynamicQ.ToString());
                        break;
                    case IConstValue.StartQ:
                    case IConstValue.MaxQ:
                    case IConstValue.MinQ:
                        for (int i = 0; i <= 15; i++)
                        {
                            Value.Add(i.ToString());
                        }
                        break;
                    case IConstValue.TARI:
                        Value.Add(TARIType.T_6_25.ToString());
                        Value.Add(TARIType.T_12_50.ToString());
                        Value.Add(TARIType.T_25_00.ToString());
                        break;
                    case IConstValue.BLF:
                        Value.Add(BLFType.BLF_40.ToString());
                        Value.Add(BLFType.BLF_160.ToString());
                        Value.Add(BLFType.BLF_213.ToString());
                        Value.Add(BLFType.BLF_256.ToString());
                        Value.Add(BLFType.BLF_320.ToString());
                        Value.Add(BLFType.BLF_640.ToString());
                        break;
                    case IConstValue.Region:
                        var supportedRegion = _reader.BaseUHF.SupportedRegion;
                        foreach (var reg in supportedRegion)
                        {
                            Value.Add(reg.ToString());
                        }
                        break;
                    case IConstValue.AmbientTempProtection:
                    case IConstValue.PATempProtection:
                        for (int i = 0; i <= 85; i++)
                        {
                            Value.Add(i.ToString());
                        }
                        break;
                    case IConstValue.UartBaudRate:
                        Value.Add("115200");
                        Value.Add("460800");
                        Value.Add("921600");
                        break;
                    case IConstValue.CfgStorageMode:
                        Value.Add(CfgStorageMode.SaveToRamAndFlashMode.ToString());
                        Value.Add(CfgStorageMode.SaveToRamOnlyMode.ToString());
                        break;
                    case IConstValue.GpioPinsConfig:
                        Value.Add("2, Input".ToString());
                        Value.Add("2, Output".ToString());
                        Value.Add("3, Input".ToString());
                        Value.Add("3, Output".ToString());
                        Value.Add("4, Input".ToString());
                        Value.Add("4, Output".ToString());
                        break;
                    case IConstValue.GpioPinsState:
                        Value.Add("2, Low".ToString());
                        Value.Add("2, High".ToString());
                        Value.Add("3, Low".ToString());
                        Value.Add("3, High".ToString());
                        Value.Add("4, Low".ToString());
                        Value.Add("4, High".ToString());
                        break;
                    case IConstValue.AntennaState:
                        Value.Add("1, On".ToString());
                        Value.Add("1, Off".ToString());
                        Value.Add("2, On".ToString());
                        Value.Add("2, Off".ToString());
                        Value.Add("3, On".ToString());
                        Value.Add("3, Off".ToString());
                        Value.Add("4, On".ToString());
                        Value.Add("4, Off".ToString());
                        break;
                    case IConstValue.AntennaSetting:
                        Value.Add("1, 2000, 30".ToString());
                        Value.Add("1, 1000, 15".ToString());
                        Value.Add("2, 2000, 30".ToString());
                        Value.Add("2, 1000, 15".ToString());
                        Value.Add("3, 2000, 30".ToString());
                        Value.Add("3, 1000, 15".ToString());
                        Value.Add("4, 2000, 30".ToString());
                        Value.Add("4, 1000, 15".ToString());
                        break;
                    case IConstValue.RfMode:
                        Value.Add(RfMode.RF_103.ToString());
                        Value.Add(RfMode.RF_302.ToString());
                        Value.Add(RfMode.RF_120.ToString());
                        Value.Add(RfMode.RF_323.ToString());
                        Value.Add(RfMode.RF_202.ToString());
                        Value.Add(RfMode.RF_345.ToString());
                        Value.Add(RfMode.RF_344.ToString());
                        Value.Add(RfMode.RF_223.ToString());
                        Value.Add(RfMode.RF_222.ToString());
                        Value.Add(RfMode.RF_241.ToString());
                        Value.Add(RfMode.RF_244.ToString());
                        Value.Add(RfMode.RF_285.ToString());
                        Value.Add(RfMode.RF_203.ToString());
                        Value.Add(RfMode.RF_226.ToString());
                        Value.Add(RfMode.RF_225.ToString());
                        break;
                    case IConstValue.ContinuousMode:
                        Value.Add(OperationMode.Continuous.ToString());
                        Value.Add(OperationMode.NonContinuous.ToString());
                        break;
                    case IConstValue.LbtCfgSetting:
                        for (int i = -80; i <= 0; i++)
                        {
                            Value.Add(i.ToString());
                        }
                        break;
                    default:
                        break;
                }

                SelectedValue = Value[0];
            }
        }

        private void OnDisplay()
        {
            SetDisplayOutput(2, true, Text);
        }
        #endregion

        #region Detect Device
        private void OnDetectFinished(object sender, DetectDeviceEventArgs e)
        {
            if (_reader != null)
            {
                _reader.StopDetect();

                _reader.DetectDeviceEvent -= OnDetectDevice;
                _reader.DetectFinishedEvent -= OnDetectFinished;
                _reader.Dispose();
                _reader = null;
            }

            if (_baseTransport != null)
            {
                _baseTransport.Dispose();
                _baseTransport = null;
            }

            if (_detectDeviceType != DeviceType.Unknown)
            {
                _baseTransport = new TransportSerial(_detectDeviceType, ConnectPath, int.Parse(SelectedBaudRate.Name));
                _reader = new UHFReader(_baseTransport);

                InitReader();

                _reader.Connect();
            }
            else
            {
                MessageBox.Show("No device detected!");
            }
        }

        private void OnDetectDevice(object sender, DetectDeviceEventArgs e)
        {
            _detectDeviceType = e.DeviceType;
        }
        #endregion

        #region Tag Operation
        private void OnInventory()
        {
            if (!_isOnInventory)
            {
                switch (_reader.DeviceType)
                {
                    case DeviceType.RM300P:
                        _reader.BaseUHF.SetSelectMask6c(TagSelectMode.Clear, Mask6cTarget.SL, Mask6cAction.AB, TagSelectTruncate.EntireEPC, BankType.EPC, 0, 0, "");
                        break;
                    default:
                        _reader.DisplayTags = new DisplayTags(ReadOnceState.Off, BeepAndVibrateState.On);

                        _reader.BaseUHF.SetSelectMask6cEnabled(0, false);
                        break;
                }

                _reader.BaseUHF.Inventory6c();

                _isOnInventory = true;
            }
        }

        private void OnStop() 
        {
            if (_isOnInventory)
            {
                _reader.BaseUHF.Stop();

                _isOnInventory = false;
            }
        }

        private void OnRead()
        {
            ResetReadInfo();

            bool canRead = false;

            switch (_reader.DeviceType)
            {
                case DeviceType.RM300P:
                    var ret = _reader.BaseUHF.SetSelectMask6c(TagSelectMode.ClearAdd, Mask6cTarget.SL, Mask6cAction.AB, TagSelectTruncate.EntireEPC, BankType.EPC, 32, (byte)(EPC.Length * 4), EPC);
                    if (ret == ResultCode.NoError) { canRead = true; }
                    break;
                default:
                    _reader.DisplayTags = new DisplayTags(ReadOnceState.On, BeepAndVibrateState.On);

                    SelectTag(EPC);
                    canRead = true;
                    break;
            }

            if (canRead)
            {
                _reader.BaseUHF.ReadMemory6c((BankType)SelectedBankTypeIndex, int.Parse(Offset), int.Parse(Length), AccessPassword);
            }
        }

        private void OnWrite()
        {
            var data = Data;
            ResetReadInfo();

            switch (_reader.DeviceType)
            {
                case DeviceType.RM300P:
                    var ret = _reader.BaseUHF.SetSelectMask6c(TagSelectMode.ClearAdd, Mask6cTarget.SL, Mask6cAction.AB, TagSelectTruncate.EntireEPC, BankType.EPC, 32, (byte)(EPC.Length * 4), EPC);
                    if (ret == ResultCode.NoError)
                    {
                        _reader.BaseUHF.WriteMemory6c((BankType)SelectedBankTypeIndex, int.Parse(Offset), int.Parse(Length), data, AccessPassword);
                    }
                    break;
                default:
                    _reader.DisplayTags = new DisplayTags(ReadOnceState.On, BeepAndVibrateState.On);

                    SelectTag(EPC);
                    _reader.BaseUHF.WriteMemory6c((BankType)SelectedBankTypeIndex, int.Parse(Offset), data, AccessPassword);
                    break;
            }
        }

        private void OnLock()
        {
            ResetReadInfo();

            switch (_reader.DeviceType)
            {
                case DeviceType.RM300P:
                    var ret = _reader.BaseUHF.SetSelectMask6c(TagSelectMode.ClearAdd, Mask6cTarget.SL, Mask6cAction.AB, TagSelectTruncate.EntireEPC, BankType.EPC, 32, (byte)(EPC.Length * 4), EPC);

                    if (ret == ResultCode.NoError)
                    {
                        Lock6cIncPermaParam lockIncPermaParam = new Lock6cIncPermaParam();

                        LockStateIncPerma lockStateIncPerma = SelectedLockType.Name switch
                        {
                            var name when name.Equals(IConstValue.Lock) => LockStateIncPerma.Lock,
                            var name when name.Equals(IConstValue.Unlock) => LockStateIncPerma.Unlock,
                            var name when name.Equals(IConstValue.PermaLock) => LockStateIncPerma.PermaLock,
                            _ => default
                        };

                        switch (SelectedLockBankType.Name)
                        {
                            case IConstValue.AccessPassword:
                                lockIncPermaParam.AccessPassword = lockStateIncPerma;
                                break;
                            case IConstValue.KillPassword:
                                lockIncPermaParam.KillPassword = lockStateIncPerma;
                                break;
                            case IConstValue.EPC:
                                lockIncPermaParam.EPC = lockStateIncPerma;
                                break;
                            case IConstValue.TID:
                                lockIncPermaParam.TID = lockStateIncPerma;
                                break;
                            case IConstValue.User:
                                lockIncPermaParam.User = lockStateIncPerma;
                                break;
                        }

                        _reader.BaseUHF.Lock6cIncPerma(lockIncPermaParam, LockPassword);
                    }
                    break;
                default:
                    _reader.DisplayTags = new DisplayTags(ReadOnceState.On, BeepAndVibrateState.On);

                    SelectTag(EPC);

                    Lock6cParam lockParam = new Lock6cParam();
                    PermaLock6cParam permaLockState = new PermaLock6cParam();

                    LockState lockState = SelectedLockType.Name switch
                    {
                        var name when name.Equals(IConstValue.Lock) => LockState.Lock,
                        var name when name.Equals(IConstValue.Unlock) => LockState.Unlock,
                        var name when name.Equals(IConstValue.PermaLock) => LockState.Lock,
                        _ => default
                    };

                    switch (SelectedLockBankType.Name)
                    {
                        case IConstValue.AccessPassword:
                            lockParam.AccessPassword = lockState;
                            permaLockState.AccessPassword = lockState;
                            break;
                        case IConstValue.KillPassword:
                            lockParam.KillPassword = lockState;
                            permaLockState.KillPassword = lockState;
                            break;
                        case IConstValue.EPC:
                            lockParam.EPC = lockState;
                            permaLockState.EPC = lockState;
                            break;
                        case IConstValue.TID:
                            lockParam.TID = lockState;
                            permaLockState.TID = lockState;
                            break;
                        case IConstValue.User:
                            lockParam.User = lockState;
                            permaLockState.User = lockState;
                            break;
                    }

                    if (SelectedLockType.Name.Equals(IConstValue.PermaLock))
                    {
                        _reader.BaseUHF.PermaLock6c(permaLockState, LockPassword);
                    }
                    else
                    {
                        _reader.BaseUHF.Lock6c(lockParam, LockPassword);
                    }
                    break;
            }
        }

        private void OnKill()
        {
            ResetReadInfo();

            bool canKill = false;

            switch (_reader.DeviceType)
            {
                case DeviceType.RM300P:
                    var ret = _reader.BaseUHF.SetSelectMask6c(TagSelectMode.ClearAdd, Mask6cTarget.SL, Mask6cAction.AB, TagSelectTruncate.EntireEPC, BankType.EPC, 32, (byte)(EPC.Length * 4), EPC);
                    if (ret == ResultCode.NoError) { canKill = true; }
                    break;
                default:
                    _reader.DisplayTags = new DisplayTags(ReadOnceState.On, BeepAndVibrateState.On);

                    SelectTag(EPC);
                    canKill = true;
                    break;
            }

            if (canKill)
            {
                if (MessageBox.Show("Are you ready to kill tag?", "Kill Tag", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _reader.BaseUHF.Kill6c(KillPassword);
                }
            }
        }

        private void OnFind()
        {
            FindDevice findDevice = new FindDevice(FindDeviceMode.VibrateBeep, 10);

            _reader.FindDevice = findDevice;
        }

        private void SelectTag(string tagEPC)
        {
            SelectMask6cParam param = new SelectMask6cParam(
                    true,
                    Mask6cTarget.SL,
                    Mask6cAction.AB,
                    BankType.EPC,
                    0,
                    tagEPC,
                    tagEPC.Length * 4);
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    _reader.BaseUHF.SetSelectMask6cEnabled(i, false);
                }
                _reader.BaseUHF.SetSelectMask6c(0, param);
            }
            catch (ReaderException e)
            {
            }
        }
        #endregion

        #region Event Functions
        private void OnActionStateChanged(object sender, ActionStateEventArgs e)
        {
            InventoryText = e.ActionState == ActionState.Stop ? IConstValue.Inventory : IConstValue.Stop;
            _isOnInventory = e.ActionState == ActionState.Stop ? false : true;

            ActionStateText = e.ActionState.ToString();

            NotifyPropertyChanged(nameof(InventoryText));
            NotifyPropertyChanged(nameof(IsInventoried));
            NotifyPropertyChanged(nameof(IsFindable));
            NotifyPropertyChanged(nameof(ActionStateText));
        }

        private void OnConnectStateChangedEvent(object sender, ConnectStateEventArgs e)
        {
            ConnectStatus = e.ConnectState.ToString();
            NotifyPropertyChanged(nameof(ConnectStatus));
            switch (e.ConnectState)
            {
                case ConnectState.Disconnected:
                    IsConnected = false;
                    DisplayEnabled = false;
                    ReleaseReader();
                    break;
                case ConnectState.Listen:
                case ConnectState.Connecting:
                    IsConnected = false;
                    break;
                case ConnectState.Connected:
                    ResetInfoUI();

                    IsConnected = true;

                    switch (_reader.DeviceType)
                    {
                        case DeviceType.RP902:
                            DisplayEnabled = true;
                            break;
                        case DeviceType.RM300P:
                            FirmwareUpdateEnabled = true;
                            break;
                    }

                    Name = _reader.DeviceName;
                    if (string.IsNullOrEmpty(Name))
                    {
                        Name = _reader.DeviceType.ToString();
                    }

                    Version = _reader.Version;
                    Bluetooth = _reader.BaseTransport.MacAddress;

                    NotifyPropertyChanged(nameof(Name));
                    NotifyPropertyChanged(nameof(Version));
                    NotifyPropertyChanged(nameof(Bluetooth));

                    if (_isConnected)
                    {
                        ExecuteCommandEnabled = true;
                        ItemVisibility = Visibility.Visible;
                        ValueVisibility = (_selectedMode == IConstValue.Get) ? Visibility.Collapsed : Visibility.Visible;
                        UpdateItemList();
                    }

                    _reader.BaseUHF.TagReadEvent += OnTagRead;
                    _reader.BaseUHF.AccessResultEvent += OnAccessResult;

                    InitUHF();
                    break;
            }
        }

        private void OnKeyEvent(object sender, KeyEventArgs e)
        {
            KeyStatus = e.KeyState.ToString();
            if (KeyStatus == KeyState.KeyDown.ToString())
            {
                OnInventory();
            }
            else
            {
                OnStop();
            }

            NotifyPropertyChanged(nameof(KeyStatus));
        }

        private void OnTemperatureEvent(object sender, TemperatureEventArgs e)
        {
            Temperature = e.Temperature.ToString();
            NotifyPropertyChanged(nameof(Temperature));
        }

        private void OnBatteryEvent(object sender, BatteryStateEventArgs e)
        {
            Battery = e.Battery.ToString();
            NotifyPropertyChanged(nameof(Battery));
        }

        private void OnAccessResult(object sender, AccessResultEventArgs e)
        {

            if (e.ActionState == ActionState.Lock ||
                e.ActionState == ActionState.PermaLock ||
                e.ActionState == ActionState.Kill)
            {
                Result = $"{e.ActionState} {e.ResultCode}";
                NotifyPropertyChanged(nameof(Result));
                return;
            }

            Data = e.Data;
            Result = $"{e.ActionState} {e.ResultCode}";

            NotifyPropertyChanged(nameof(Data));
            NotifyPropertyChanged(nameof(Result));
        }

        private void OnTagRead(object sender, TagReadEventArgs e)
        {
            EPC = e.EPC;
            TID = e.TID;
            RSSI = e.RSSI.ToString();

            switch (_reader.DeviceType)
            {
                case DeviceType.RP902:
                    AntennaID = (e.Antenna + 1).ToString();
                    break;
                default:
                    AntennaID = e.Antenna.ToString();
                    break;
            }


            #region list box for tags
            //<timmy>
            if (EPC != null)
            {
                try
                {
                    //if(mainWindow != null)
                    {
                        //Console.WriteLine("count:" + mainWindow.lstTags.Items.Count);
                        //if(mainWindow.ListRfidTags.Items.Count < 20)
                        if (RfidTags.Count < 20)
                        {
                            //string temp = mainWindow.ListRfidTags.Items.Count + ": " + EPC;
                            //string temp = RfidTags.Count + ": " + EPC;
                            //Console.WriteLine(temp);                            
                            if (!RfidTags.Contains(EPC))
                            {
                                //<timmy> 不同執行緒不可直接變更UI; mainWindow UI物件應bind ViewModel屬性用Invoke變更
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    RfidTags.Add(EPC);
                                    //RfidTags.Add("test" + DateTime.Now);                                
                                    //mainWindow.ListRfidTags.Items.Add(EPC);
                                    //mainWindow.lstTags.Items.Refresh();
                                    mainWindow.ButtonClear.Content = "Clear Result " + RfidTags.Count().ToString();
                                });
                            }                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                             
            }
            #endregion            
            NotifyPropertyChanged(nameof(EPC));
            NotifyPropertyChanged(nameof(TID));
            NotifyPropertyChanged(nameof(RSSI));
            NotifyPropertyChanged(nameof(AntennaID));
            
            //<timmy>
            //NotifyPropertyChanged(nameof(RfidTags));
        }

        private void OnProgressRateEvent(object sender, double e)
        {
            Process = e.ToString() + "%";

            NotifyPropertyChanged(nameof(Process));
        }
        #endregion

        #region Sample code for calling function
        private ResultCode CWOff()
        {
            try
            {
                return _reader.CWOff();
            }
            catch (ReaderException e)
            {
                return e.ResultCode;
            }
        }

        private ResultCode CWOn(CarrierWave cw)
        {
            try
            {
                return _reader.CWOn(cw);
            }
            catch (ReaderException e)
            {
                return e.ResultCode;
            }
        }

        private void FirmwareUpdateCancel()
        {
            try
            {
                _reader.FirmwareUpdateCancel();
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private AntennaSetting GetAntennaSetting(int antennaId)
        {
            try
            {
                return _reader.GetAntennaSetting(antennaId);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private unitechRFID.Device.Params.AntennaState GetAntennaState(int antennaId)
        {
            try
            {
                return _reader.GetAntennaState(antennaId);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private GpioPinDirection GetGpioPinsConfig(int number)
        {
            try
            {
                return _reader.GetGpioPinsConfig(number);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private GpioPinState GetGpioPinsState(int number)
        {
            try
            {
                return _reader.GetGpioPinsState(number);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetAntennaSetting(AntennaSetting antennaSetting)
        {
            try
            {
                _reader.SetAntennaSetting(antennaSetting);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetAntennaState(unitechRFID.Device.Params.AntennaState antennaState)
        {
            try
            {
                _reader.SetAntennaState(antennaState);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetGpioPinsConfig(int number, GpioPinDirection direction)
        {
            try
            {
                _reader.SetGpioPinsConfig(number, direction);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetGpioPinsState(int number, GpioPinState state)
        {
            try
            {
                _reader.SetGpioPinsState(number, state);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private ResultCode TxRandomData(TxRandData parms)
        {
            try
            {
                return _reader.TxRandomData(parms);
            }
            catch (ReaderException e)
            {
                return e.ResultCode;
            }
        }

        private bool GetSelectMask6cEnabled(int index)
        {
            try
            {
                return _reader.BaseUHF.GetSelectMask6cEnabled(index);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private Mask6cTarget GetSelectMask6cTarget(int index)
        {
            try
            {
                return _reader.BaseUHF.GetSelectMask6cTarget(index);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetSelectMask6cTarget(int index, Mask6cTarget target)
        {
            try
            {
                _reader.BaseUHF.SetSelectMask6cTarget(index, target);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private Mask6cAction GetSelectMask6cAction(int index)
        {
            try
            {
                return _reader.BaseUHF.GetSelectMask6cAction(index);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetSelectMask6cAction(int index, Mask6cAction action)
        {
            try
            {
                _reader.BaseUHF.SetSelectMask6cAction(index, action);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private BankType GetSelectMask6cBank(int index)
        {
            try
            {
                return _reader.BaseUHF.GetSelectMask6cBank(index);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetSelectMask6cBank(int index, BankType bank)
        {
            try
            {
                _reader.BaseUHF.SetSelectMask6cBank(index, bank);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private int GetSelectMask6cOffset(int index)
        {
            try
            {
                return _reader.BaseUHF.GetSelectMask6cOffset(index);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetSelectMask6cOffset(int index, int offset)
        {
            try
            {
                _reader.BaseUHF.SetSelectMask6cOffset(index, offset);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private Mask6cPattern GetSelectMask6cPattern(int index)
        {
            try
            {
                return _reader.BaseUHF.GetSelectMask6cPattern(index);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private void SetSelectMask6cPattern(int index, Mask6cPattern pattern)
        {
            try
            {
                _reader.BaseUHF.SetSelectMask6cPattern(index, pattern);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private SelectMask6cParam GetSelectMask6c(int index)
        {
            try
            {
                return _reader.BaseUHF.GetSelectMask6c(index);
            }
            catch (ReaderException e)
            {
                throw;
            }
        }

        private ResultCode Inventory6c(BankType bankType, int offset, int length, string password)
        {
            try
            {
                return _reader.BaseUHF.Inventory6c(bankType, offset, length, password);
            }
            catch (ReaderException e)
            {
                return e.ResultCode;
            }
        }

        private ResultCode WriteBlockMemory6c(BankType bank, int offset, int length, string data, string password)
        {
            try
            {
                return _reader.BaseUHF.WriteBlockMemory6c(bank, offset, length, data, password);
            }
            catch (ReaderException e)
            {
                return e.ResultCode;
            }
        }
        #endregion

        public static void Show(string msg)
        {
            Debug.WriteLine(msg);
        }

        private void ResetReadInfo()
        {
            Data = string.Empty;
            Result = string.Empty;

            NotifyPropertyChanged(nameof(Data));
            NotifyPropertyChanged(nameof(Result));
        }

        private void ReleaseReader()
        {
            NotifyPropertyChanged(nameof(IsInventoried));

            FirmwareUpdateEnabled = false;
            ExecuteCommandEnabled = false;
            ItemVisibility = Visibility.Collapsed;
            ValueVisibility = Visibility.Collapsed;

            Item = null;

            Value = null;

            if (_reader != null)
            {
                _reader.ActionStateChangedEvent -= OnActionStateChanged;
                _reader.ConnectStateChangedEvent -= OnConnectStateChangedEvent;

                _reader.TemperatureEvent -= OnTemperatureEvent;
                switch (_reader.DeviceType)
                {
                    case DeviceType.RM300P:
                        _reader.BaseUHF.ProgressRateEvent -= OnProgressRateEvent;
                        break;
                    default:
                        _reader.BatteryStateEvent -= OnBatteryEvent;
                        _reader.KeyStateEvent -= OnKeyEvent;
                        break;
                }

                if (_reader.BaseUHF != null)
                {
                    _reader.BaseUHF.TagReadEvent -= OnTagRead;
                    _reader.BaseUHF.AccessResultEvent -= OnAccessResult;
                }

                _reader.Dispose();
                _reader = null;
            }
        }

        public void Dispose()
        {
            ReleaseReader();
        }

        private void SetDisplayOutput(int linePosition, bool clearScreen, string displayOutput)
        {
            try
            {
                int command = 0;

                switch (linePosition)
                {
                    case 1:
                        command += 64;
                        break;
                    case 2:
                        command += 128;
                        break;
                    case 3:
                        command += 192;
                        break;
                    default:
                        command += 128;
                        break;
                }

                if (clearScreen)
                {
                    command += 32;
                }

                int outputLength = displayOutput.ToString().Length;
                if (outputLength < 16)
                {
                    command += (16 - outputLength) / 2;
                }

                _reader.DisplayOutput = new DisplayOutput((byte)command, displayOutput.ToCharArray());
            }
            catch
            { }
        }
    }
}
