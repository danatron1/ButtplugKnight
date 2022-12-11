using GoodVibes;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ButtplugMod
{
    public class ButtplugMod : Mod
    {
        internal static ButtplugMod Instance;

        string logPath = $"{Environment.CurrentDirectory}\\VibeLog.txt";
        int secondsPerHit = 5;
        bool hitRecently = false;
        int port = 12345;
        int retryAttempts = 10;

        bool vibing => timeToReset > 0;
        float currentPower = 0;
        float timeToReset = 0;

        PlugManager plug;

        new public string GetName() => "Buttplug Knight";
        public override string GetVersion() => "v1";

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;
            Log("Initialized");
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.BeforeAddHealthHook += BeforeHealthAdd;
            ModHooks.AfterTakeDamageHook += OnHeroDamaged;
            if (!File.Exists(logPath)) File.Create(logPath);
            else File.WriteAllLines(logPath, new string[0]);

            PlugSetup();
        }

        private int BeforeHealthAdd(int arg)
        {
            DoGoodVibes(arg * 0.2f);
            return arg;
        }

        void PlugSetup()
        {
            try
            {
                plug = new PlugManager()
                {
                    Port = port,
                    RetryAmount = retryAttempts
                };
                plug.LogMessage += LogVibe;
                plug.Initialize();
                Log($"Plug initialized, logging to {logPath}");
            }
            catch (Exception e)
            {
                Log($"Failed to initialize vibrator - {e.GetType()}: {e.Message}");
            }
        }
        private void OnHeroUpdate()
        {
            if (vibing)
            {
                timeToReset -= Time.deltaTime;
                if (!vibing)
                {
                    plug?.SetPowerLevel(0);
                    currentPower = 0;
                }
            }
        }
        private int OnHeroDamaged(int hazardType, int damageAmount)
        {
            if (hazardType == 0) DoGoodVibes(damageAmount * 0.4f);
            else DoGoodVibes(damageAmount);
            return damageAmount;
        }
        private void DoGoodVibes(float amount)
        {
            if (vibing) amount *= 2;
            currentPower = Mathf.Max(Mathf.Min(0.5f * amount, 1), currentPower);
            float seconds = secondsPerHit * amount;
            LogVibe($"Took {amount} damage, vibing at intensity {currentPower} for {seconds} seconds");
            timeToReset += seconds;
            plug?.SetPowerLevel(currentPower);
        }
        public void LogVibe(string s) 
        {
            File.AppendAllText(logPath, $"[{DateTime.Now}] {s}\n");
        }
    }
}