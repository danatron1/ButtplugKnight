using ButtplugKnight;
using GoodVibes;
using MagicUI.Core;
using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ButtplugMod
{
    public class ButtplugMod : Mod, IMenuMod, ITogglableMod
    {
        private LayoutRoot? layout;

        internal static ButtplugMod Instance;
        static PlayerData player => PlayerData.instance;

        private int port = 12345;
        private int retryAttempts = 10;
        private float updateFrequency = 0.125f;

        private int   secondsPerHit = 5;
        private float baseVibeRate = 0.2f;

        private int   buzzOnRelics = 0;
        private bool  doubleOnOverlap = true;
        private bool  scaleWithDamage = true;
        private bool  randomSurprises = true;
        private float buzzOnHeal = 0.25f;
        private bool  buzzOnDamage = true;
        private float buzzOnStrike = 0f;
        private int   buzzOnDeath = 0;
        private float punctuateHits = 0f;
        private bool  vulnerableWhileVibing = false;
        private bool  rotationEnabled = true;
        private int   waveType = 0;
        //UI options
        private bool  displayPercentage = true;
        private bool  displayTimeRemaining = true;

        /*adding a new setting? Make sure to;
         *  add it to the menu options (ensuring you change the loader and saver)
         *  add it to the save settings method
         *  add it to the load settings method
         */
        bool vulnerable => vulnerableWhileVibing && vibing;

        static string hkData = "hollow_knight_Data";
        string modPath => $@"{Environment.CurrentDirectory}\{hkData}\Managed\Mods\ButtplugKnight";
        string logPath => $@"{modPath}\VibeLog.txt";
        string settingsPath => $@"{modPath}\Settings.txt";

        bool vibing => timeToReset > 0;

        float currentPower;
        float timeToReset;
        float punctuateTimerRemaining;
        float lastUpdate;

        PlugManager plug;
        new public string GetName() => "Buttplug Knight";
        public override string GetVersion() => "1.4.4";
        void LoadSettings()
        {
            try
            {
                string[] settingsRaw = File.ReadAllLines(settingsPath);
                Dictionary<string, string> settings = new Dictionary<string, string>();
                foreach (string setting in settingsRaw)
                {
                    string[] parts = setting.Split('=');
                    settings.Add(parts[0], parts[1]);
                }
                secondsPerHit = int.Parse(settings[nameof(secondsPerHit)]);
                baseVibeRate = float.Parse(settings[nameof(baseVibeRate)]);
                doubleOnOverlap = bool.Parse(settings[nameof(doubleOnOverlap)]);
                scaleWithDamage = bool.Parse(settings[nameof(scaleWithDamage)]);
                randomSurprises = bool.Parse(settings[nameof(randomSurprises)]);
                buzzOnHeal = float.Parse(settings[nameof(buzzOnHeal)]);
                buzzOnDamage = bool.Parse(settings[nameof(buzzOnDamage)]);
                buzzOnStrike = float.Parse(settings[nameof(buzzOnStrike)]);
                buzzOnDeath = int.Parse(settings[nameof(buzzOnDeath)]);
                punctuateHits = float.Parse(settings[nameof(punctuateHits)]);
                vulnerableWhileVibing = bool.Parse(settings[nameof(vulnerableWhileVibing)]);
                displayPercentage = bool.Parse(settings[nameof(displayPercentage)]);
                displayTimeRemaining = bool.Parse(settings[nameof(displayTimeRemaining)]);
                rotationEnabled = bool.Parse(settings[nameof(rotationEnabled)]);
                waveType = int.Parse(settings[nameof(waveType)]);
                buzzOnRelics = int.Parse(settings[nameof(buzzOnRelics)]);

                //Hidden settings
                port = int.Parse(settings[nameof(port)]);
                retryAttempts = int.Parse(settings[nameof(retryAttempts)]);
                updateFrequency = float.Parse(settings[nameof(updateFrequency)]);

                plug?.SetRotationEnabled(rotationEnabled);
            }
            catch (FileNotFoundException ex)
            {
                LogVibe("No settings file found; using default settings.");
            }
            catch (Exception ex)
            {
                LogVibe($"Failed to load settings. {ex.GetType()}; {ex.Message}");
            }
        }
        void SaveSettings()
        {
            try
            {
                List<string> settings = new()
                {
                    $"{nameof(secondsPerHit)}={secondsPerHit}",
                    $"{nameof(baseVibeRate)}={baseVibeRate}",
                    $"{nameof(doubleOnOverlap)}={doubleOnOverlap}",
                    $"{nameof(scaleWithDamage)}={scaleWithDamage}",
                    $"{nameof(randomSurprises)}={randomSurprises}",
                    $"{nameof(buzzOnHeal)}={buzzOnHeal}",
                    $"{nameof(buzzOnDamage)}={buzzOnDamage}",
                    $"{nameof(buzzOnStrike)}={buzzOnStrike}",
                    $"{nameof(buzzOnDeath)}={buzzOnDeath}",
                    $"{nameof(punctuateHits)}={punctuateHits}",
                    $"{nameof(vulnerableWhileVibing)}={vulnerableWhileVibing}",
                    $"{nameof(displayPercentage)}={displayPercentage}",
                    $"{nameof(displayTimeRemaining)}={displayTimeRemaining}",
                    $"{nameof(rotationEnabled)}={rotationEnabled}",
                    $"{nameof(waveType)}={waveType}",
                    $"{nameof(buzzOnRelics)}={buzzOnRelics}",

                    $"{nameof(port)}={port}",
                    $"{nameof(retryAttempts)}={retryAttempts}",
                    $"{nameof(updateFrequency)}={updateFrequency}"
                };
                File.WriteAllLines(settingsPath, settings.ToArray());
                plug?.SetRotationEnabled(rotationEnabled);
            }
            catch (Exception ex)
            {
                LogVibe($"Failed to save settings. {ex.GetType()}; {ex.Message}");
            }
        }
        public void Unload()
        {
            TurnOffVibrator();
            ModHooks.HeroUpdateHook -= OnHeroUpdate;
            ModHooks.BeforeAddHealthHook -= BeforeHealthAdd;
            ModHooks.AfterTakeDamageHook -= OnHeroDamaged;
            On.HeroController.Awake -= OnSaveOpened;
            ModHooks.SoulGainHook -= OnSoulGain;
            ModHooks.AfterPlayerDeadHook -= OnDeath;
            ModHooks.SetPlayerIntHook -= OnCollectRelic;

            VibeUI.textUI.Text = string.Empty;
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            LogVibe("Initializing");

            Instance = this;
            currentPower = 0;
            timeToReset = 0.09f;
            punctuateTimerRemaining = 0;

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.BeforeAddHealthHook += BeforeHealthAdd;
            ModHooks.AfterTakeDamageHook += OnHeroDamaged;
            On.HeroController.Awake += OnSaveOpened;
            ModHooks.SoulGainHook += OnSoulGain;
            ModHooks.AfterPlayerDeadHook += OnDeath;
            ModHooks.SetPlayerIntHook += OnCollectRelic;

            Regex pattern = new(@"[hH]ollow[\s_][kK]night[\s_][dD]ata");
            foreach (string directory in Directory.EnumerateDirectories(Environment.CurrentDirectory))
            {
                Match match = pattern.Match(directory);
                if (match.Success)
                {
                    hkData = match.Value;
                    break;
                }
            }

            LoadSettings();

            if (!File.Exists(logPath)) File.Create(logPath).Close();
            else if (plug == null) File.WriteAllLines(logPath, new string[0]);
            if (plug == null) PlugSetup();

            LogVibe($"Initialized - {GetName()} {GetVersion()}");
        }

        private void OnSaveOpened(On.HeroController.orig_Awake orig, HeroController self)
        {
            if (layout == null)
            {
                layout = new(true, "Persistent layout");
                layout.RenderDebugLayoutBounds = false;

                // comment out examples as needed to see only the specific ones you want
                VibeUI.Setup(layout);
            }
            orig(self);
        }
        private void UpdateTextDisplay()
        {
            string text = "";
            if (displayPercentage)
            {
                if (plug._currentPower != currentPower) text += $"({plug._currentPower * 100:f0}%) ";
                text += $"{currentPower * 100:f0}%\n";
            }
            if (displayTimeRemaining && vibing)
            {
                if (timeToReset > 60) text += $"{(int)timeToReset / 60}:{timeToReset % 60:00.0}";
                else text += $"{timeToReset % 60:f1}";
            }
            try
            {
                VibeUI.textUI.Text = text;
            }
            catch (NullReferenceException e)
            {
                LogVibe($"Text UI not properly initialised: {e.Message}");
            }
        }
        private int OnCollectRelic(string name, int orig)
        {
            if (buzzOnRelics == 0) return orig;
            switch (name)
            {
                case "trinket1": //wanderer's journal - 200 geo
                    VibeTrinket(2); //20% for 2 seconds
                    break;
                case "trinket2": //hallownest seal - 450 geo
                    VibeTrinket(4.5f); //45% for 4.5 seconds
                    break;
                case "trinket3": //king's idol - 800 geo
                    VibeTrinket(8); //80% for 8 seconds
                    break;
                case "trinket4": //arcane egg - 1200 geo
                    VibeTrinket(12); //100% for 12 seconds
                    break;
            }
            return orig;

            void VibeTrinket(float seconds)
            {
                LogVibe($"Got {name}, vibing for {seconds} seconds");
                if (PlayerData.instance.GetInt(name) < orig)
                {
                    timeToReset += seconds * buzzOnRelics;
                    UpdateVibratorPower(Mathf.Min(1, currentPower + (seconds / 10)));
                    LogVibe($"Vibe activated for {seconds}");
                }
            }
        }
        private void OnDeath()
        {
            if (buzzOnDeath > 0)
            {
                timeToReset += buzzOnDeath;
                UpdateVibratorPower(1);
                LogVibe($"Died! Vibing for +{buzzOnDeath} seconds ({timeToReset})");
            }
        }
        /* DEPRECIATED (for now)
        
        private void OnStrike(Collider2D otherCollider, GameObject slash) 
        {
            if (buzzOnStrike != 0)
            {
                LogVibe($"Nail hit {slash.name} {slash.GetInstanceID()} on {otherCollider.name} {otherCollider.GetInstanceID()}");
                DoGoodVibes(buzzOnStrike);
            }
        }*/
        private int OnSoulGain(int arg)
        {
            if (buzzOnStrike == 0) return arg;
            DoGoodVibes(arg * buzzOnStrike);
            return arg;
        }
        private int BeforeHealthAdd(int arg)
        {
            if (buzzOnHeal != 0) DoGoodVibes(arg * buzzOnHeal);
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
                LogVibe($"Plug initialized, logging to {logPath}");
            }
            catch (Exception e)
            {
                LogVibe($"Failed to initialize vibrator - {e.GetType()}: {e.Message}");
            }
        }
        static System.Random rng = new System.Random();
        private void OnHeroUpdate()
        {
            if (displayPercentage || displayTimeRemaining) UpdateTextDisplay();
            if (randomSurprises && rng.Next(1, 1_000_000) == 69)
            {
                timeToReset += 10;
                UpdateVibratorPower(1);
                LogVibe("Random surprise triggered! Enjoy 10 seconds of max power :)");
            }
            if (punctuateHits > 0 && punctuateTimerRemaining > 0)
            {
                punctuateTimerRemaining -= Time.deltaTime;
                if (punctuateTimerRemaining <= 0 && currentPower < 1)
                {
                    UpdateVibratorPower();
                }
            }
            else if (vibing)
            {
                timeToReset -= Time.deltaTime;
                if (!vibing) TurnOffVibrator();
                else if (waveType != 0 && lastUpdate - timeToReset > updateFrequency)
                {
                    UpdateVibratorPower();
                }
            }
        }
        private void TurnOffVibrator()
        {
            plug?.SetPowerLevel(0);
            currentPower = 0;
            timeToReset = 0;
            lastUpdate = 0;
        }
        private void UpdateVibratorPower(float? level = null)
        {
            if (level is not null) currentPower = level.Value;
            plug?.SetPowerLevel(GetWaveMultiplier());
            lastUpdate = timeToReset;
        }
        float GetWaveMultiplier()
        {
            currentPower = Mathf.Clamp01(currentPower);
            float multiplier = waveType switch
            {
                //sine
                1 => (Mathf.Sin(timeToReset * Mathf.PI) + 2) / 3,
                //sine2
                2 => Mathf.Sin(timeToReset * (Mathf.PI / 2)) * currentPower + currentPower,
                //square
                3 => (timeToReset % 1f) >= 0.5f ? 0f : 1f,
                //square2
                4 => ((timeToReset % 1f) > currentPower ? 0f : 1f) / currentPower,
                //triangle
                5 => timeToReset % 1f,
                //reverse triangle
                6 => 1f - (timeToReset % 1f),
                //no wave (default)
                _ => 1f,
            };
            if (waveType == 2 || waveType == 4) return Mathf.Clamp01(multiplier); //these two don't need multiplying.
            return Mathf.Clamp01(currentPower * multiplier);
        }
        private int OnHeroDamaged(int hazardType, int damageAmount)
        {
            if (!buzzOnDamage) return damageAmount;
            if (hazardType == 0) return damageAmount;
            if (vulnerable) damageAmount *= 2;
            DoGoodVibes(damageAmount);
            if (punctuateHits > 0)
            {
                if (currentPower < 1)
                {
                    plug?.SetPowerLevel(1);
                    LogVibe($"Hit punctuated by {punctuateHits} seconds of max power.");
                }
                punctuateTimerRemaining = punctuateHits;
            }
            return damageAmount;
        }
        private void DoGoodVibes(float amount)
        {
            bool healTriggered = amount < 1;
            string message = "";
            //if took more than 1 damage
            if (amount > 1)
            {
                if (!scaleWithDamage) amount = 1; //scale it to 1 if we're not doing that
                else if (!vulnerable || amount > 2) message = "(heavy) "; //else mark it as a heavy hit
                                                                            //you always take 2x damage while vulnerable.
                                                                            //A heavy hit is defined by one that's normally 2+ damage.
            }
            //calculate new power level
            float newPower = baseVibeRate * amount;
            if (vulnerable && !healTriggered) newPower /= 2; //vulnerable is only supposed to effect the game, not the vibe
            if (!healTriggered && player.GetBool(nameof(player.overcharmed)))
            {
                newPower *= 2; //for some reason the game doesn't report double damage from overcharming?
                message = "(overcharmed) " + message;
            }
            if (doubleOnOverlap) newPower += currentPower;
            else if (newPower < currentPower) newPower = currentPower;
            newPower = Mathf.Clamp01(newPower);
            //logging type of damage
            if (healTriggered) message += $"Small vibe source ({amount})";
            else message += $"Took {amount} damage"; //just for log message
            if (vulnerable) message = "(vuln. 2x) " + message;
            if (vibing && doubleOnOverlap)
            {
                message = "(overlap) " + message;
                if (!vulnerable) amount *= 2; //apply doubling after so it only effects timer (since vibration is additive it's already accounted for)
            }
            //by default, dying in radiant mode triggers the game to deal 9999 damage to you
            //this equates to almost 14 hours of vibe at default settings. I think it's fair to lower it.
            if (amount > 10)
            {
                LogVibe($"Oof, radiant death? That'd normally be {secondsPerHit * amount} seconds. Lowering that a bit.");
                amount = 10;
            }
            float seconds = secondsPerHit * amount;
            timeToReset += seconds;
            lastUpdate += seconds;
            LogVibe($"{message}. Vibing at {100 * newPower}% for {seconds} seconds. {timeToReset:f1}s left.");
            currentPower = newPower;
            //if we're punctuating hits, we don't need to update the power here, as it'll be done after.
            //unless, of course, it's not caused by a hit (heal triggered) or it's already at 100%.
            if (punctuateHits == 0 || currentPower == 1 || healTriggered) UpdateVibratorPower();
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
                    Name = "Intensity",
                    Description = "The vibrator power increase for 1 damage (20% recommended)",
                    Values = new string[] {
                        "1%",
                        "5%",
                        "10%",
                        "15%",
                        "20%",
                        "25%",
                        "33%",
                        "50%",
                        "69%",
                        "100%"
                    },
                    Saver = opt => {baseVibeRate = opt switch {
                        0 => 0.01f,
                        1 => 0.05f,
                        2 => 0.1f,
                        3 => 0.15f,
                        4 => 0.2f,
                        5 => 0.25f,
                        6 => 0.33f,
                        7 => 0.5f,
                        8 => 0.69f,
                        9 => 1f,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => baseVibeRate switch {
                        0.01f => 0,
                        0.05f => 1,
                        0.1f => 2,
                        0.15f => 3,
                        0.2f => 4,
                        0.25f => 5,
                        0.33f => 6,
                        0.5f => 7,
                        0.69f => 8,
                        1f => 9,
                        _ => 4
                    }
                }, //Intensity
                new IMenuMod.MenuEntry {
                    Name = "Seconds per hit",
                    Description = "The vibration duration from 1 damage (5-10 recommended)",
                    Values = new string[] {
                        "1",
                        "2",
                        "3",
                        "4",
                        "5",
                        "6",
                        "7",
                        "8",
                        "9",
                        "10",
                        "15",
                        "20",
                        "69"
                    },
                    Saver = opt => {secondsPerHit = opt switch {
                        0 => 1,
                        1 => 2,
                        2 => 3,
                        3 => 4,
                        4 => 5,
                        5 => 6,
                        6 => 7,
                        7 => 8,
                        8 => 9,
                        9 => 10,
                        10 => 15,
                        11 => 20,
                        12 => 69,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => secondsPerHit switch {
                        1 => 0,
                        2 => 1,
                        3 => 2,
                        4 => 3,
                        5 => 4,
                        6 => 5,
                        7 => 6,
                        8 => 7,
                        9 => 8,
                        10 => 9,
                        15 => 10,
                        20 => 11,
                        69 => 12,
                        _ => 4
                    }
                }, //seconds per hit
                new IMenuMod.MenuEntry {
                    Name = "Buzz on damage",
                    Description = "Taking damage causes vibrations - the point of the mod.",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {buzzOnDamage = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => buzzOnDamage switch {
                        true => 0,
                        false => 1,
                    }
                }, //buzz on damage
                new IMenuMod.MenuEntry {
                    Name = "Buzz on heal",
                    Description = "Healing will vibe equivalent to a fraction of a hit",
                    Values = new string[] {
                        "Off",
                        "1/4",
                        "1/2",
                        "Full"
                    },
                    Saver = opt => {buzzOnHeal = opt switch {
                        0 => 0f,
                        1 => 0.25f,
                        2 => 0.5f,
                        3 => 1f,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => buzzOnHeal switch {
                        0f => 0,
                        0.25f => 1,
                        0.5f => 2,
                        1f => 3,
                        _ => 1
                    }
                }, //buzz when healing
                new IMenuMod.MenuEntry {
                    Name = "Buzz on soul gain",
                    Description = "Extracting white fluids from enemies will cause a vibration",
                    Values = new string[] {
                        "Off",
                        "1/4",
                        "1/2",
                        "Full"
                    },
                    Saver = opt => {buzzOnStrike = opt switch {
                        0 => 0f,
                        1 => 0.25f,
                        2 => 0.5f,
                        3 => 1f,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => buzzOnStrike switch {
                        0f => 0,
                        0.25f => 1,
                        0.5f => 2,
                        1f => 3,
                        _ => 1
                    }
                }, //buzz on soul gain
                new IMenuMod.MenuEntry {
                    Name = "Buzz on death",
                    Description = "On death, sets power to 100%, and adds a number of seconds",
                    Values = new string[] {
                        "Off",
                        "1",
                        "5",
                        "10",
                        "15",
                        "20",
                        "69",
                    },
                    Saver = opt => {buzzOnDeath = opt switch {
                        0 => 0,
                        1 => 1,
                        2 => 5,
                        3 => 10,
                        4 => 15,
                        5 => 20,
                        6 => 69,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => buzzOnDeath switch {
                        0 => 0,
                        1 => 1,
                        5 => 2,
                        10 => 3,
                        15 => 4,
                        20 => 5,
                        69 => 6,
                        _ => 0
                    }
                }, //buzz on death
                new IMenuMod.MenuEntry {
                    Name = "Buzz on relics",
                    Description = "Vibes upon picking up a relic. Meant for use with Archipelago",
                    Values = new string[] {
                        "Off",
                        "On",
                        "2x",
                        "4x",
                        "8x",
                    },
                    Saver = opt => {buzzOnRelics = opt switch {
                        0 => 0,
                        1 => 1,
                        2 => 2,
                        3 => 4,
                        4 => 8,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => buzzOnRelics switch {
                        0 => 0,
                        1 => 1,
                        2 => 2,
                        4 => 3,
                        8 => 4,
                        _ => 0
                    }
                }, //buzz on relic
                new IMenuMod.MenuEntry {
                    Name = "Scale with damage",
                    Description = "Heavier hits will cause longer vibrations",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {scaleWithDamage = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
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
                    Saver = opt => {doubleOnOverlap = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => doubleOnOverlap switch {
                        true => 0,
                        false => 1,
                    }
                }, //double on overlap
                new IMenuMod.MenuEntry {
                    Name = "Random surprises",
                    Description = "Every frame has a 1 in 1,000,000 chance of fun surprises",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {randomSurprises = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => randomSurprises switch {
                        true => 0,
                        false => 1,
                    }
                }, //random surprises
                new IMenuMod.MenuEntry {
                    Name = "Vulnerable vibing",
                    Description = "While vibing, the knight takes double damage.",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {vulnerableWhileVibing = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => vulnerableWhileVibing switch {
                        true => 0,
                        false => 1,
                    }
                }, //Vulnerable when vibing
                new IMenuMod.MenuEntry {
                    Name = "Punctuate hits",
                    Description = "Hits cause a short burst of max power vibe, followed by regular",
                    Values = new string[] {
                        "Off",
                        "0.2s",
                        "0.3s",
                        "0.4s",
                        "0.5s",
                        "0.6s",
                        "0.8s",
                        "1s",
                    },
                    Saver = opt => {punctuateHits = opt switch {
                        0 => 0f,
                        1 => 0.2f,
                        2 => 0.3f,
                        3 => 0.4f,
                        4 => 0.5f,
                        5 => 0.6f,
                        6 => 0.8f,
                        7 => 1f,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => punctuateHits switch {
                        0f => 0,
                        0.2f => 1,
                        0.3f => 2,
                        0.4f => 3,
                        0.5f => 4,
                        0.6f => 5,
                        0.8f => 6,
                        1f => 7,
                        _ => 3
                    }
                }, //punctuate hits
                new IMenuMod.MenuEntry {
                    Name = "Display Intensity",
                    Description = "Display the intensity percentage in the top right on the UI",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {displayPercentage = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => displayPercentage switch {
                        true => 0,
                        false => 1,
                    }
                }, //Display Intensity
                new IMenuMod.MenuEntry {
                    Name = "Display Timer",
                    Description = "Display the amount of time left before the vibrator turns off",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {displayTimeRemaining = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => displayTimeRemaining switch {
                        true => 0,
                        false => 1,
                    }
                }, //Display Timer
                new IMenuMod.MenuEntry {
                    Name = "Oscillate",
                    Description = "Vibes in a wave pattern instead of staying at max power",
                    Values = new string[] {
                        "Off",
                        "Sine",
                        "Sine2",
                        "Square",
                        "Square2",
                        "Triangle",
                        "Triangle2"
                    },
                    Saver = opt => { waveType = opt;
                        SaveSettings(); },
                    Loader = () => waveType
                }, //Enable Oscillation
                new IMenuMod.MenuEntry {
                    Name = "Enable rotation",
                    Description = "Toy also rotates on hit, for toys with rotation capabilities",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {rotationEnabled = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => rotationEnabled switch {
                        true => 0,
                        false => 1,
                    }
                } //Enable Rotation
                //max line length |----------------------------------------------------------------|
            };
        }
    }
}