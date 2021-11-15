using CSCore.CoreAudioAPI;
using Microsoft.Win32;

namespace AudioUnfuck
{
    internal class LockManager
    {
        private Dictionary<String, DeviceInfo> deviceInfos;

        public LockManager(MMDevice[] devices)
        {
            deviceInfos = new Dictionary<String, DeviceInfo>();
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AudioLock");
            String lockedStr = (string)(key.GetValue("LockedDevices") ?? "");
            key.Close();
            foreach (var device in devices)
            {
                if (lockedStr.Contains(device.DeviceID))
                {
                    Lock(device);
                }
            }
        }

        internal void Lock(MMDevice device)
        {
            System.Diagnostics.Debug.WriteLine("Registering callback for " + device);
            var endpoint = AudioEndpointVolume.FromDevice(device);
            var expectedVol = endpoint.GetMasterVolumeLevel();
            AudioEndpointVolumeCallback callback = new AudioEndpointVolumeCallback();
            callback.NotifyRecived += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Callback called for " + device + " with level " + e.MasterVolume);
                if (e.MasterVolume != expectedVol)
                {
                    endpoint.SetMasterVolumeLevel(expectedVol, Guid.Empty);
                }
            };
            endpoint.RegisterControlChangeNotify(callback);
            deviceInfos.Add(device.DeviceID, new DeviceInfo(endpoint, callback));
        }

        internal void Unlock(MMDevice device)
        {
            System.Diagnostics.Debug.WriteLine("Unregistering callback for " + device);
            if (deviceInfos.TryGetValue(device.DeviceID, out DeviceInfo deviceInfo))
            {
                deviceInfo.endpoint.UnregisterControlChangeNotify(deviceInfo.callback);
                deviceInfos.Remove(device.DeviceID);
            }
        }

        internal bool IsLocked(MMDevice device)
        {
            return deviceInfos.ContainsKey(device.DeviceID);
        }

        internal void Save()
        {
            var lockedDevices = String.Join("|", deviceInfos.Keys);
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AudioLock");
            key.SetValue("LockedDevices", lockedDevices);
            key.Close();
        }
    }

    internal class DeviceInfo
    {
        public AudioEndpointVolume endpoint;
        public AudioEndpointVolumeCallback callback;

        public DeviceInfo(AudioEndpointVolume endpoint, AudioEndpointVolumeCallback callback)
        {
            this.endpoint = endpoint;
            this.callback = callback;
        }
    }
}

