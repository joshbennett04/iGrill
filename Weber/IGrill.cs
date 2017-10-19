using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace Weber
{
    public class IGrill
    {
        private Guid GENERIC_ATTRIBUTE = Guid.Parse("00001801-0000-1000-8000-00805f9b34fb");
        private Guid BATTERY = Guid.Parse("0000180f-0000-1000-8000-00805f9b34fb");
        private Guid GENERIC_ACCESS = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");
        private Guid FIRMWARE_VERSION = Guid.Parse("64ac0001-4a4b-4b58-9f37-94d3c52ffdf7");
        private Guid APP_CHALLENGE = Guid.Parse("64AC0002-4A4B-4B58-9F37-94D3C52FFDF7");
        private Guid DEVICE_CHALLENGE = Guid.Parse("64ac0003-4a4b-4b58-9f37-94d3c52ffdf7");
        private Guid DEVICE_RESPONSE = Guid.Parse("64AC0004-4A4B-4B58-9F37-94D3C52FFDF7");
        private Guid CONFIG = Guid.Parse("06ef0002-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE1_TEMPERATURE = Guid.Parse("06EF0002-2E06-4B79-9E33-FCE2C42805EC");
        private Guid PROBE1_THRESHOLD = Guid.Parse("06ef0003-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE2_TEMPERATURE = Guid.Parse("06ef0004-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE2_THRESHOLD = Guid.Parse("06ef0005-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE3_TEMPERATURE = Guid.Parse("06ef0006-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE3_THRESHOLD = Guid.Parse("06ef0007-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE4_TEMPERATURE = Guid.Parse("06ef0008-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE4_THRESHOLD = Guid.Parse("06ef0009-2e06-4b79-9e33-fce2c42805ec");
        private Guid SERVICE_GUID = Guid.Parse("A5C50000-F186-4BD6-97F2-7EBACBA0D708");

        private Guid AUTH = Guid.Parse("64ac0000-4a4b-4b58-9f37-94d3c52ffdf7");

        private BluetoothLEDevice m_BleDevice;
        private const String m_DeviceName = "iGrill_V2";

        private string m_Probe1Temp = "0";
        private string m_Probe2Temp = "0";
        private string m_Probe3Temp = "0";
        private string m_Probe4Temp = "0";

        private string m_Probe1Name = "Probe 1";
        private string m_Probe2Name = "Probe 2";
        private string m_Probe3Name = "Probe 3";
        private string m_Probe4Name = "Probe 4";

        private GattCharacteristic m_Probe1Characteristic = null;
        private GattCharacteristic m_Probe2Characteristic = null;
        private GattCharacteristic m_Probe3Characteristic = null;
        private GattCharacteristic m_Probe4Characteristic = null;

        private BluetoothConnectionStatus m_ConnectionStatus = BluetoothConnectionStatus.Disconnected;
        private byte[] m_ChallengeBuffer;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler Toast;

        public IGrill()
        {
            m_ChallengeBuffer = new byte[16];
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyToast([CallerMemberName] string propertyName = "")
        {
            Toast?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            System.Diagnostics.Debug.WriteLine(propertyName);
        }

        public string Probe1Temp
        {
            get
            {
                return m_Probe1Temp;
            }
        }

        public string Probe2Temp
        {
            get
            {
                return m_Probe2Temp;
            }
        }

        public string Probe3Temp
        {
            get
            {
                return m_Probe3Temp;
            }
        }

        public string Probe4Temp
        {
            get
            {
                return m_Probe4Temp;
            }
        }

        public string Probe1Name
        {
            get
            {
                return m_Probe1Name;
            }

            set
            {
                if (m_Probe1Name != value)
                {
                    m_Probe1Name = value;
                }
            }
        }

        public string Probe2Name
        {
            get
            {
                return m_Probe2Name;
            }

            set
            {
                if (m_Probe2Name != value)
                {
                    m_Probe2Name = value;
                }
            }
        }

        public string Probe3Name
        {
            get
            {
                return m_Probe3Name;
            }

            set
            {
                if (m_Probe3Name != value)
                {
                    m_Probe3Name = value;
                }
            }
        }

        public string Probe4Name
        {
            get
            {
                return m_Probe4Name;
            }

            set
            {
                if (m_Probe4Name != value)
                {
                    m_Probe4Name = value;
                }
            }
        }

        private byte[] EncryptionKey
        {
            get
            {
                byte[] bufferTest = StringToByteArray("df33e089f4484e7392d4cfb946e785b6");
                return bufferTest;
            }
        }

        public BluetoothConnectionStatus ConnectionStatus
        {
            get
            {
                return m_ConnectionStatus;
            }
        }

        public async void Reset()
        {
            string deviceId = null;
            if (m_BleDevice != null)
            {
                deviceId = m_BleDevice.DeviceId;
                m_BleDevice.ConnectionStatusChanged -= BleDevice_ConnectionStatusChanged;
                m_BleDevice = null;
            }

            if (m_Probe1Characteristic != null)
            {
                m_Probe1Characteristic.ValueChanged -= Characteristics_ValueChanged;
                m_Probe1Characteristic = null;
            }

            if (m_Probe2Characteristic != null)
            {
                m_Probe2Characteristic.ValueChanged -= Characteristics_ValueChanged;
                m_Probe2Characteristic = null;
            }

            if (m_Probe3Characteristic != null)
            {
                m_Probe3Characteristic.ValueChanged -= Characteristics_ValueChanged;
                m_Probe3Characteristic = null;
            }

            if (m_Probe4Characteristic != null)
            {
                m_Probe4Characteristic.ValueChanged -= Characteristics_ValueChanged;
                m_Probe4Characteristic = null;
            }

            if (deviceId != null)
            {
                BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(deviceId);
                await Connect(device);
            }
            else
            {
                NotifyToast("Null Bluetooth Device");
            }
        }

        public async Task<Boolean> Connect(string deviceId)
        {
            BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(deviceId);
            return await Connect(device);
        }

        public async Task<Boolean> Connect(BluetoothLEDevice bluetoothLEDevice)
        {
            try
            {
                if (m_BleDevice == null)
                {
                    NotifyToast("iGrill not found.");
                    return false;
                }

                m_BleDevice.ConnectionStatusChanged += BleDevice_ConnectionStatusChanged;

                GattDeviceServicesResult service = await m_BleDevice.GetGattServicesForUuidAsync(AUTH);
                GattDeviceService authService = service.Services.First();
                GattCharacteristicsResult chars = await authService.GetCharacteristicsAsync();

                if (chars.Characteristics.Count() == 0)
                {
                    NotifyToast("iGrill Authentication Characteristic not found");

                    return false;
                }

                GattCharacteristicsResult appChallengeCharacteristicResult = await authService.GetCharacteristicsForUuidAsync(APP_CHALLENGE);
                GattCharacteristicsResult deviceChallengeCharacteristicResult = await authService.GetCharacteristicsForUuidAsync(DEVICE_CHALLENGE);
                GattCharacteristicsResult deviceResponseCharacteristicResult = await authService.GetCharacteristicsForUuidAsync(DEVICE_RESPONSE);

                if (appChallengeCharacteristicResult.Characteristics.Count < 1 ||
                    deviceChallengeCharacteristicResult.Characteristics.Count < 1 ||
                    deviceResponseCharacteristicResult.Characteristics.Count < 1)
                {
                    NotifyToast("iGrill Gatt Characteristics not found");
                    return false;
                }

                GattCharacteristic appChallenge = appChallengeCharacteristicResult.Characteristics.First();
                GattCharacteristic deviceChallenge = deviceChallengeCharacteristicResult.Characteristics.First();
                GattCharacteristic deviceResponse = deviceResponseCharacteristicResult.Characteristics.First();
                GattWriteResult appChallengeWriteResult = await WriteAppChallenge(appChallenge);

                if (appChallengeWriteResult.Status == GattCommunicationStatus.Success)
                {
                    GattReadResult deviceChallengeReadResult = await deviceChallenge.ReadValueAsync(BluetoothCacheMode.Uncached);
                    GattWriteResult deviceResponseResult = await WriteDeviceResponse(deviceResponse, deviceChallengeReadResult.Value.ToArray());

                    if (deviceResponseResult.Status != GattCommunicationStatus.Success)
                    {
                        NotifyToast("iGrill Device Response Failed");
                        return false;
                    }
                }
                else
                {
                    NotifyToast("iGrill App Challenge Failed");
                    return false;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                return false;
            }

            await LoadProbes();

            return true;
        }

        private async Task<GattWriteResult> WriteDeviceResponse(GattCharacteristic deviceResponse, byte[] request)
        {
            byte[] decrypted = Utilities.Decrypt(EncryptionKey, request);
            string test = System.Text.Encoding.UTF8.GetString(decrypted);

            if (decrypted == null)
            {
                return null;
            }
            else if (!Utilities.CompareMemory(m_ChallengeBuffer, decrypted, 8))
            {
                return null;
            }

            byte[] response = new byte[16];
            Utilities.CopyMemory(response, decrypted, 16);
            Utilities.SetMemory(response, (byte)0, 8);
            byte[] challenge_response = Utilities.Encrypt(EncryptionKey, response);

            return await deviceResponse.WriteValueWithResultAsync(challenge_response.AsBuffer());
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private async Task<GattWriteResult> WriteAppChallenge(GattCharacteristic appChallenge)
        {
            Utilities.SetMemory(m_ChallengeBuffer, 0, m_ChallengeBuffer.Length);
            byte[] tempBuffer = new byte[8];
            new Random().NextBytes(tempBuffer);
            Utilities.CopyMemory(m_ChallengeBuffer, tempBuffer, tempBuffer.Length);
            byte[] buffer = m_ChallengeBuffer.Clone() as byte[];

            return await appChallenge.WriteValueWithResultAsync(buffer.AsBuffer());
        }

        public async Task<bool> LoadProbes()
        {
            GattDeviceServicesResult service = await m_BleDevice.GetGattServicesForUuidAsync(SERVICE_GUID);
            int count = service.Services.Count();
            GattDeviceService deviceService = service.Services.First();
            var chars = await deviceService.GetCharacteristicsAsync();
            DeviceAccessStatus accessStatus = await deviceService.RequestAccessAsync();
            GattOpenStatus openStatus = await deviceService.OpenAsync(GattSharingMode.Exclusive);
            foreach (GattCharacteristic gatchar in chars.Characteristics)
            {
                if (gatchar.Uuid == PROBE1_TEMPERATURE)
                {
                    m_Probe1Characteristic = gatchar;
                    await m_Probe1Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    m_Probe1Characteristic.ValueChanged += Characteristics_ValueChanged;
                }
                else if (gatchar.Uuid == PROBE2_TEMPERATURE)
                {
                    m_Probe2Characteristic = gatchar;
                    await m_Probe2Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    m_Probe2Characteristic.ValueChanged += Characteristics_ValueChanged;
                }
                else if (gatchar.Uuid == PROBE3_TEMPERATURE)
                {
                    m_Probe3Characteristic = gatchar;
                    await m_Probe3Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    m_Probe3Characteristic.ValueChanged += Characteristics_ValueChanged;
                }
                else if (gatchar.Uuid == PROBE4_TEMPERATURE)
                {
                    m_Probe4Characteristic = gatchar;
                    await m_Probe4Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    m_Probe4Characteristic.ValueChanged += Characteristics_ValueChanged;
                }
            }
            return true;
        }

        private void BleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            m_ConnectionStatus = sender.ConnectionStatus;
            System.Diagnostics.Debug.WriteLine(sender.ConnectionStatus);
        }

        public async Task<int> GetBatteryLevel()
        {
            var immediateAlertService = await m_BleDevice.GetGattServicesForUuidAsync(GattServiceUuids.Battery);
            var characteristics = await immediateAlertService.Services.First().GetCharacteristicsForUuidAsync(GattCharacteristicUuids.BatteryLevel);

            if (characteristics.Characteristics.Count > 0)
            {
                GattReadResult x = await characteristics.Characteristics.First().ReadValueAsync(BluetoothCacheMode.Uncached);
                byte[] byteArray = new byte[x.Value.Length];
                DataReader.FromBuffer(x.Value).ReadBytes(byteArray);
                int batterylevel = byteArray[0];
                return batterylevel;
            }
            else
            {
                return 0;
            }
        }

        private void Characteristics_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data = args.CharacteristicValue.ToArray();
            ProbeTemp probeTemp = null;

            if (sender.Uuid == PROBE1_TEMPERATURE)
            {
                if (data[1].ToString().Equals("248"))
                {
                    probeTemp = new ProbeTemp()
                    {
                        Name = "Probe 1",
                        PendingRemoval = true
                    };
                    m_Probe1Temp = "0";
                }
                else
                {
                    m_Probe1Temp = data[0].ToString();
                    probeTemp = new ProbeTemp()
                    {
                        Name = "Probe 1",
                        Temp = data[0].ToString()
                    };
                }
            }
            else if (sender.Uuid == PROBE2_TEMPERATURE)
            {
                if (data[1].ToString().Equals("248"))
                {
                    probeTemp = new ProbeTemp()
                    {
                        Name = "Probe 2",
                        PendingRemoval = true
                    };
                    m_Probe2Temp = "0";
                }
                else
                {
                    m_Probe2Temp = data[0].ToString();
                    probeTemp = new ProbeTemp()
                    {
                        Name = "Probe 2",
                        Temp = data[0].ToString()
                    };
                }
            }
            else if (sender.Uuid == PROBE3_TEMPERATURE)
            {
                if (data[1].ToString().Equals("248"))
                {
                    probeTemp = new ProbeTemp()
                    {
                        Name = "Probe 3",
                        PendingRemoval = true
                    };
                    m_Probe3Temp = "0";
                }
                else
                {
                    m_Probe3Temp = data[0].ToString();
                    probeTemp = new ProbeTemp()
                    {
                        Name = "Probe 3",
                        Temp = data[0].ToString()
                    };
                }
            }
            else if (sender.Uuid == PROBE4_TEMPERATURE)
            {
                if (data[1].ToString().Equals("248"))
                {
                    probeTemp = new ProbeTemp()
                    {
                        Name = "Probe 4",
                        PendingRemoval = true
                    };
                    m_Probe4Temp = "0";
                }
                else
                {
                    m_Probe4Temp = data[0].ToString();
                    probeTemp = new ProbeTemp()
                    {
                        Name = "Probe 4",
                        Temp = data[0].ToString()
                    };
                }
            }

            if (probeTemp != null)
            {
                NotifyPropertyChanged();
            }
        }
    }
}
