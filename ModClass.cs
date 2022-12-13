using GoodVibes;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ButtplugMod
{
    public class ButtplugMod : Mod, IMenuMod, ITogglableMod
    {
        internal static ButtplugMod Instance;

        private int port = 12345;
        private int secondsPerHit = 5;
        private int retryAttempts = 10;
        private float baseVibeRate = 0.5f;

        private bool doubleOnOverlap = true;
        private bool buzzOnHeal = true;
        private bool scaleWithDamage = true;

        string logPath = $"{Environment.CurrentDirectory}\\VibeLog.txt";

        bool vibing => timeToReset > 0;

        float currentPower = 0;
        float timeToReset = 2;

        PlugManager plug;

        new public string GetName() => "Buttplug Knight";
        public override string GetVersion() => "v1.1";

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this; 
            Log("Initialized"); 
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.BeforeAddHealthHook += BeforeHealthAdd;
            ModHooks.AfterTakeDamageHook += OnHeroDamaged;

            if (!File.Exists(logPath)) File.Create(logPath);
            else if (plug == null) File.WriteAllLines(logPath, new string[0]);

            if (plug == null) PlugSetup();
        }

        private int BeforeHealthAdd(int arg)
        {
            if (buzzOnHeal) DoGoodVibes(arg * 0.2f);
            return arg;
        }

        async void PlugSetup()
        {
            try
            {
                plug = new PlugManager()
                {
                    Port = port,
                    RetryAmount = retryAttempts
                };
                plug.LogMessage += LogVibe;
                await plug.Initialize();
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
            if (!scaleWithDamage && amount > 1) amount = 1;
            currentPower = (doubleOnOverlap ? currentPower : 0) + baseVibeRate * amount;
            if (currentPower > 1) currentPower = 1;
            if (vibing && doubleOnOverlap) amount *= 2;
            currentPower = Mathf.Max(Mathf.Min(baseVibeRate * amount, 1), currentPower);
            if (amount > 10)
            {
                LogVibe($"Oof, radiant death? That'd normally be {secondsPerHit * amount} seconds. Lowering that a bit.");
                amount = 10;
            }
            float seconds = secondsPerHit * amount;
            if (amount is < 1 and > 0) LogVibe($"Healed. vibing at intensity {currentPower} for {seconds} seconds");
            else LogVibe($"Took {amount} damage, vibing at intensity {currentPower} for {seconds} seconds");
            timeToReset += seconds;
            plug?.SetPowerLevel(currentPower);
        }
        public void LogVibe(string s) 
        {
            File.AppendAllText(logPath, $"[{DateTime.Now}] {s}\n");
        }

        public bool ToggleButtonInsideMenu => false;
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>
            {
                new IMenuMod.MenuEntry {
                    Name = "Buzz when healing",
                    Description = "Healing will cause a small short vibration",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => buzzOnHeal = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => buzzOnHeal switch {
                        true => 0,
                        false => 1,
                    }
                }, //buzz when healing
                new IMenuMod.MenuEntry {
                    Name = "Scale with damage",
                    Description = "Heavier hits will cause longer vibrations",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => scaleWithDamage = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => scaleWithDamage switch {
                        true => 0,
                        false => 1,
                    }
                }, //scale with damage
                new IMenuMod.MenuEntry {
                    Name = "Double on overlap",
                    Description = "Vibration commands can stack",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => doubleOnOverlap = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => doubleOnOverlap switch {
                        true => 0,
                        false => 1,
                    } 
                }, //double on overlap
                new IMenuMod.MenuEntry {
                    Name = "Intensity",
                    Description = "The vibration level from 1 damage (50% recommended)",
                    Values = new string[] {
                        "10%",
                        "20%",
                        "30%",
                        "40%",
                        "50%",
                        "75%",
                        "100%"
                    },
                    Saver = opt => baseVibeRate = opt switch {
                        0 => 0.1f,
                        1 => 0.20f,
                        2 => 0.20f,
                        3 => 0.20f,
                        4 => 0.5f,
                        5 => 0.75f,
                        6 => 1f,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => baseVibeRate switch {
                        0.1f => 0,
                        0.2f => 1,
                        0.3f => 2,
                        0.4f => 3,
                        0.5f => 4,
                        0.75f => 5,
                        1f => 6,
                        _ => 4
                    } 
                }, //Intensity
                new IMenuMod.MenuEntry {
                    Name = "Seconds per hit",
                    Description = "The vibration duration from 1 damage",
                    Values = new string[] {
                        "1",
                        "2",
                        "3",
                        "4",
                        "5",
                        "10",
                        "20",
                        "69"
                    },
                    Saver = opt => baseVibeRate = opt switch {
                        0 => 1,
                        1 => 2,
                        2 => 3,
                        3 => 4,
                        4 => 5,
                        5 => 10,
                        6 => 20,
                        7 => 69,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => baseVibeRate switch {
                        1 => 0,
                        2 => 1,
                        3 => 2,
                        4 => 3,
                        5 => 4,
                        10 => 5,
                        20 => 6,
                        69 => 7,
                        _ => 4
                    }
                } //seconds per hit
            };
        }
        public void Unload()
        {
            ModHooks.HeroUpdateHook -= OnHeroUpdate;
            ModHooks.BeforeAddHealthHook -= BeforeHealthAdd;
            ModHooks.AfterTakeDamageHook -= OnHeroDamaged;
        }
    }
}