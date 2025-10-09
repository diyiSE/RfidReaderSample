using System;
using System.Collections.Generic;
using System.Text;

namespace unitechRFIDSample
{
    interface IConstValue
    {
        const string Connect = "Connect";
        const string Disconnect = "Disconnect";

        const string Serial = "Serial";
        const string Bluetooth = "Bluetooth";
        const string Path = "Path:";
        const string MACAddress = "MAC Address:";

        const string FactoryReset = "FactoryReset";
        const string FirmwareUpdate = "FirmwareUpdate";
        const string ExecuteCommand = "ExecuteCommand";
        const string Display = "Display";

        const string Inventory = "Inventory";
        const string Stop = "Stop";
        const string Read = "Read";
        const string Write = "Write";
        const string Lock = "Lock";
        const string Kill = "Kill";
        const string Find = "Find";

        const string Reserved = "Reserved";
        const string EPC = "EPC";
        const string TID = "TID";
        const string User = "User";
        const string AccessPassword = "AccessPassword";
        const string Text = "Text";
        const string KillPassword = "KillPassword";

        const string Unlock = "Unlock";
        const string PermaLock = "PermaLock";

        const string DefaultOffset = "2";
        const string DefaultLength = "6";
        const string DefaultPassword = "00000000";

        const string Get = "Get";
        const string Set = "Set";

        const string DeviceName = "Device Name";
        const string SerialNumber = "Serial Number";
        const string FirmwareVersion = "Firmware Version";
        const string BluetoothMAC = "Bluetooth MAC";
        const string SKU = "SKU";
        const string SKUID = "SKUID";
        const string Battery = "Battery";
        const string Temperature = "Temperature";

        const string GlobalBand = "Global Band";
        const string Beeper = "Beeper";
        const string Vibrator = "Vibrator";
        const string AutoScreenOffTime = "Auto Screen Off Time";
        const string AutoPowerOffTime = "Auto Power Off Time";
        const string OperationMode = "Operation Mode";
        const string ReadMode = "Read Mode";
        const string DataFormat = "Data Format";
        const string DataTerminator = "Data Terminator";
        const string DataInfo = "Data Info";
        const string Region = "Region";
        const string AmbientTempProtection = "Ambient Temp Protection";
        const string PATempProtection = "PA Temp Protection";
        const string UartBaudRate = "Uart Baud Rate";
        const string CfgStorageMode = "Cfg Storage Mode";
        const string GpioPinsConfig = "Gpio Pins Config";
        const string GpioPinsState = "Gpio Pins State";
        const string BiStaticAntenna = "Bi Static Antenna";
        const string AntennaState = "Antenna State";
        const string AntennaSetting = "Antenna Setting";
        const string LbtSetting = "Lbt Setting";

        const string Power = "Power";
        const string InventoryTime = "Inventory Time";
        const string IdleTime = "Idle Time";
        const string Target = "Target";
        const string Session = "Session";
        const string Encoding = "Encoding";
        const string Algorithm = "Algorithm";
        const string StartQ = "Start Q";
        const string MaxQ = "Max Q";
        const string MinQ = "Min Q";
        const string TARI = "TARI";
        const string BLF = "BLF";
        const string ToggleTarget = "Toggle Target";
        const string FastMode = "Fast Mode";
        const string RfMode = "RF Mode";
        const string TagFocus = "Tag Focus";
        const string FastId = "Fast ID";
        const string ContinuousMode = "Continuous Mode";
        const string LbtCfgSetting = "Lbt Cfg Setting";
    }
}
