//using Buttplug;
using ButtplugManaged;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GoodVibes
{

    public class PlugManager
    {
        private ButtplugWebsocketConnectorOptions _connector;
        private int _retries;

        private float _currentPower = 0;

        public ButtplugClient Client { get; private set; }

        public int Port { get; init; }
        public int RetryAmount { get; init; }

        public event Action<string> LogMessage;

        private void Log(string s) => LogMessage?.Invoke(s);

        private void SetupClient()
        {
            Client = new ButtplugClient("Plug Control");

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

        private async void ClientOnServerDisconnect(object sender, EventArgs e)
        {
            Log("Disconnected from server.");
            for (_retries = 0; _retries < RetryAmount; _retries++)
            {
                Log($"Reconnecting... (Attempt {_retries + 1} of {RetryAmount})");
                SetupClient();
                var success = await TryConnect();
                if (success) return;
                Log("Trying again in 5 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
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

        private void UpdatePowerLevels()
        {
            if (Client == null)
            {
                Log($"Tried to update power level, but Client was null");
                return;
            }
            if (!Client.Connected)
            {
                Log($"Tried to update power level, but Client was disconnected");
                return;
            }
            foreach (var plug in Client?.Devices)
            {
                plug?.SendVibrateCmd(_currentPower);
            }
        }

        private async Task<bool> TryConnect()
        {
            try
            {
                Log("Connecting to the server...");
                await Client.ConnectAsync(_connector);
                Log("Connected to server.");

                Log("Starting to scan for devices.");
                try
                {
                    await Client.StartScanningAsync();
                }
                catch (ButtplugException ex)
                {
                    Log($"Failed to start scanning for devices: {ex.InnerException?.Message}");
                    return false;
                }

                return true;
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

        public async Task<bool> Initialize() 
        {
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
            _currentPower = Mathf.Clamp(level, 0, 1);
            UpdatePowerLevels();
        }

        public async Task SetPowerLevelDuration(float level, TimeSpan span)
        {
            var old = _currentPower;
            SetPowerLevel(level);
            await Task.Delay(span);
            SetPowerLevel(old);
        }
    }
}