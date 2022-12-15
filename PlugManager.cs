//using Buttplug;
using ButtplugManaged;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GoodVibes
{
    public static class AsyncFAF
    {
        public static async void FireAndForget(this Task t, Action<string> logging)
        {
            try
            {
                await t;
            }
            catch (Exception e)
            {
                logging?.Invoke($"FAF Exception: {e.GetType()}; {e.Message}");
                if (e.InnerException != null) logging?.Invoke($"    Inner: {e.InnerException.GetType()}; {e.InnerException.Message}");
            }
        }
    }
    public class PlugManager
    {
        private ButtplugWebsocketConnectorOptions _connector;
        //private int _retries;

        private float _currentPower = 0;

        public ButtplugClient Client { get; private set; }

        public int Port { get; init; }
        public int RetryAmount { get; init; }

        public event Action<string> LogMessage;

        private void Log(string s) => LogMessage?.Invoke(s);

        private void SetupClient()
        {
            Client = new ButtplugClient("Plug Control");
            _triedToInitialize = false;
            Client.DeviceAdded += OnDeviceAdded;
            Client.DeviceRemoved += OnDeviceRemoved;
            Client.ServerDisconnect += ClientOnServerDisconnect;
            Client.ErrorReceived += ClientOnErrorReceived;
            Client.PingTimeout += ClientOnPingTimeout;
        }

        private void OnDeviceAdded(object sender, DeviceAddedEventArgs e)
        {
            Log($"Device Connected: {e.Device.Name}");
        }

        private void OnDeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {
            Log($"Device Disconnected: {e.Device.Name}");
        }
        bool _tryingToReconnect = false;
        private async void ClientOnServerDisconnect(object sender, EventArgs e)
        {
            if (_tryingToReconnect) return;
            Log("Disconnected from server.");
            _tryingToReconnect = true;
            for (int _retries = 0; _retries < RetryAmount; _retries++)
            {
                Log($"Reconnecting... (Attempt {_retries + 1} of {RetryAmount})");
                SetupClient();
                bool success = await TryConnect();
                if (success) return;
                Log("Trying again in 5 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            _tryingToReconnect = false;
            Log("Could not reconnect to server.");
        }

        private void ClientOnErrorReceived(object sender, ButtplugExceptionEventArgs e)
        {
            Log($"Received an error: {e.Exception.Message}");
        }

        private void ClientOnPingTimeout(object sender, EventArgs e)
        {
            Log("Server ping timed out.");
        }

        private async Task UpdatePowerLevels()
        {
            Log($"Updating power level to {_currentPower*100}%");
            if (Client == null)
            {
                Log($"Intiface Client is null - cannot update power. Try restarting the game.");
                if (_triedToInitialize) SetupClient();
                if (!await Initialize()) return;
            }
            if (!Client.Connected && Client.Devices.Length == 0 && false)
            {
                Log($"Intiface Client is disconnected - Is the server running?");
                if (!await TryConnect())
                {
                    Log($"Failed to connect. Resetting client to reinitialize");
                    SetupClient();
                    if (await Initialize()) Log("Reinitialized!");
                    else return;
                }
                else Log("Reconnected!");
            }
            else if (Client.Devices.Length == 0)
            {
                Log($"Intiface Client connected, but no devices are connected - Can you see the device under \"Devices\" on Intiface Central?");
            }
            foreach (var plug in Client?.Devices)
            {
                plug?.SendVibrateCmd(_currentPower);
            }
        }
        private async Task<bool> TryScanning()
        {
            Log("Starting to scan for devices.");
            try
            {
                await Client.StartScanningAsync();
                _triedToInitialize = false;
            }
            catch (ButtplugException ex)
            {
                Log($"Failed to start scanning for devices: {ex.InnerException?.Message}");
                return false;
            }
            return true;
        }
        private async Task<bool> TryConnect()
        {
            try
            {
                Log("Connecting to the server...");
                await Client.ConnectAsync(_connector);
                Log("Connected to server.");
                _tryingToReconnect = false;
                return await TryScanning();
            }
            catch (ButtplugConnectorException e)
            {
                Log($"Could not connect to the server: {e.InnerException?.Message}");
                return false;
            }
            catch (ButtplugHandshakeException e)
            {
                Log($"There was an error performing the handshake with the server: {e.InnerException?.Message}");
                return false;
            }
        }
        internal bool _triedToInitialize = false;
        public async Task<bool> Initialize() 
        {
            if (_triedToInitialize) return false;
            _triedToInitialize = true;
            _connector = new ButtplugWebsocketConnectorOptions(new Uri($"ws://localhost:{Port}/buttplug"));
            SetupClient();

            var success = await TryConnect();
            if (!success)
            {
                Log("Could not connect to the server.");
                return false;
            }

            return true;
        }
        public void SetPowerLevel(float level)
        {
            //if (level == _currentPower) return;
            _currentPower = Mathf.Clamp01(level);
            Task.Factory.StartNew(() => UpdatePowerLevels().FireAndForget(LogMessage));
        }
    }
}