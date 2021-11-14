using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AudioUnfuck
{
    internal class LockManager
    {
        private Dictionary<MMDevice, AudioEndpointVolume> endpoints;
        private Dictionary<MMDevice, AudioEndpointVolumeCallback> callbacks;

        public LockManager()
        {
            endpoints = new Dictionary<MMDevice, AudioEndpointVolume>();
            callbacks = new Dictionary<MMDevice, AudioEndpointVolumeCallback>();
        }

        internal void Subscribe(MMDevice device)
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
            endpoints.Add(device, endpoint);
            callbacks.Add(device, callback);
        }

        internal void Unsubscribe(MMDevice device)
        {
            System.Diagnostics.Debug.WriteLine("Unregistering callback for " + device);
            if (endpoints.TryGetValue(device, out AudioEndpointVolume endpoint))
            {
                if (callbacks.TryGetValue(device, out AudioEndpointVolumeCallback callback))
                {
                    endpoint.UnregisterControlChangeNotify(callback);
                    endpoints.Remove(device);
                    callbacks.Remove(device);
                }
            }
        }
    }
}
