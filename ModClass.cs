using ButtplugKnight;
using GoodVibes;
using MagicUI.Core;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace ButtplugMod
{
    public class ButtplugMod : Mod, IMenuMod, ITogglableMod
    {
        private LayoutRoot? layout;

        internal static ButtplugMod Instance;
        static PlayerData player => PlayerData.instance;

        const int port = 12345;
        const int retryAttempts = 10;

        private int   secondsPerHit = 5;
        private float baseVibeRate = 0.5f;

        private bool  doubleOnOverlap = true;
        private bool  scaleWithDamage = true;
        private bool  randomSurprises = true;
        private bool  buzzOnHeal = true;
        private bool  buzzOnDamage = true;
        private int buzzOnStrike = 0; //off, not when full, always
        private bool  punctuateHits = false;
        private bool  vulnerableWhileVibing = false;
        //UI options
        private bool  displayPercentage = false;
        private bool  displayTimeRemaining = false;

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
        float punctuateTimer;

        PlugManager plug;

        private void UpdateTextDisplay()
        {
            string text = "";
            if (displayPercentage) text += $"{plug._currentPower*100:f0}%\n";
            if (displayTimeRemaining && vibing) text += $"{timeToReset:f1}";
            try
            {
                VibeUI.textUI.Text = text;
            }
            catch (NullReferenceException e)
            {
                Log($"Text UI not properly initialised: {e.Message}");
            }
        }
        new public string GetName() => "Buttplug Knight";
        public override string GetVersion() => "1.2.6";
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
                buzzOnHeal = bool.Parse(settings[nameof(buzzOnHeal)]);
                buzzOnDamage = bool.Parse(settings[nameof(buzzOnDamage)]);
                buzzOnStrike = int.Parse(settings[nameof(buzzOnStrike)]);
                punctuateHits = bool.Parse(settings[nameof(punctuateHits)]);
                vulnerableWhileVibing = bool.Parse(settings[nameof(vulnerableWhileVibing)]);
                displayPercentage = bool.Parse(settings[nameof(displayPercentage)]);
                displayTimeRemaining = bool.Parse(settings[nameof(displayTimeRemaining)]);
            }
            catch (FileNotFoundException ex)
            {
                Log("No settings file found; using default settings.");
            }
            catch (Exception ex)
            {
                Log($"Failed to load settings. {ex.GetType()}; {ex.Message}");
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
                    $"{nameof(punctuateHits)}={punctuateHits}",
                    $"{nameof(vulnerableWhileVibing)}={vulnerableWhileVibing}",
                    $"{nameof(displayPercentage)}={displayPercentage}",
                    $"{nameof(displayTimeRemaining)}={displayTimeRemaining}"
                };
                File.WriteAllLines(settingsPath, settings.ToArray());
            }
            catch (Exception ex)
            {
                Log($"Failed to save settings. {ex.GetType()}; {ex.Message}");
            }
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            Instance = this;
            currentPower = 0;
            timeToReset = 0.09f;
            punctuateTimer = 0;

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.BeforeAddHealthHook += BeforeHealthAdd;
            ModHooks.AfterTakeDamageHook += OnHeroDamaged;
            On.HeroController.Awake += OnSaveOpened;
            ModHooks.SoulGainHook += OnSoulGain;

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

            Log("Initialized");
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

        private int OnSoulGain(int arg)
        {
            if (buzzOnStrike == 0) return arg;
            if (buzzOnStrike == 1 && player.GetInt(nameof(player.MPCharge)) == player.GetInt(nameof(player.maxMP)))
            {
                if (player.GetInt(nameof(player.MPReserve)) == player.GetInt(nameof(player.MPReserveMax))) return arg;
                if (BossSequenceController.BoundSoul) return arg;
            }
            DoGoodVibes(arg / 50f);
            return arg;
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
        static System.Random rng = new System.Random();
        private void OnHeroUpdate()
        {
            if (displayPercentage || displayTimeRemaining) UpdateTextDisplay();
            if (randomSurprises && rng.Next(1, 1_000_000) == 69)
            {
                timeToReset += 10;
                plug?.SetPowerLevel(1);
                LogVibe("Random surprise triggered! Enjoy 10 seconds of max power :)");
            }
            if (punctuateHits && punctuateTimer > 0)
            {
                punctuateTimer -= Time.deltaTime;
                if (punctuateTimer <= 0 && currentPower < 1)
                {
                    plug?.SetPowerLevel(currentPower);
                }
            }
            else if (vibing)
            {
                timeToReset -= Time.deltaTime;
                if (!vibing)
                {
                    plug?.SetPowerLevel(0);
                    currentPower = 0;
                    timeToReset = 0;
                }
            }
        }
        private int OnHeroDamaged(int hazardType, int damageAmount)
        {
            if (!buzzOnDamage) return damageAmount;
            if (hazardType == 0) return damageAmount;
            if (vulnerable) damageAmount *= 2;
            DoGoodVibes(damageAmount);
            if (punctuateHits)
            {
                if (currentPower < 1)
                {
                    plug?.SetPowerLevel(1);
                    LogVibe("Hit punctuated by half a second of max power.");
                }
                punctuateTimer = 0.5f;
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
            LogVibe($"{message}. Vibing at {100*newPower}% for {seconds} seconds. {timeToReset:f1}s left.");
            if (currentPower == newPower) return;
            currentPower = newPower;
            //if we're punctuating hits, we don't need to update the power here, as it'll be done after.
            //unless, of course, it's not caused by a hit (heal triggered) or it's already at 100%.
            if (!punctuateHits || currentPower == 1 || healTriggered) plug?.SetPowerLevel(currentPower);
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
                    Description = "The vibration level from 1 damage (50% recommended)",
                    Values = new string[] {
                        "5%",
                        "10%",
                        "20%",
                        "30%",
                        "40%",
                        "50%",
                        "75%",
                        "100%"
                    },
                    Saver = opt => {baseVibeRate = opt switch {
                        0 => 0.05f,
                        1 => 0.1f,
                        2 => 0.2f,
                        3 => 0.3f,
                        4 => 0.4f,
                        5 => 0.5f,
                        6 => 0.75f,
                        7 => 1f,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => baseVibeRate switch {
                        0.05f => 0,
                        0.1f => 1,
                        0.2f => 2,
                        0.3f => 3,
                        0.4f => 4,
                        0.5f => 5,
                        0.75f => 6,
                        1f => 7,
                        _ => 5
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
                    Saver = opt => {secondsPerHit = opt switch {
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
                    }; SaveSettings(); },
                    Loader = () => secondsPerHit switch {
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
                    Name = "Buzz when healing",
                    Description = "Healing will cause a small short vibration",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {buzzOnHeal = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => buzzOnHeal switch {
                        true => 0,
                        false => 1,
                    }
                }, //buzz when healing
                new IMenuMod.MenuEntry {
                    Name = "Buzz on soul gain",
                    Description = "Extracting white fluids from enemies will cause a small vibration",
                    Values = new string[] {
                        "Off",
                        "Not when full",
                        "Always" 
                    },
                    Saver = opt => {buzzOnStrike = opt; SaveSettings(); },
                    Loader = () => buzzOnStrike
                }, //buzz on soul gain
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
                    Description = "Hits cause half a second of max power vibe, followed by regular",
                    Values = new string[] {
                        "On",
                        "Off"
                    },
                    Saver = opt => {punctuateHits = opt switch {
                        0 => true,
                        1 => false,
                        // This should never be called
                        _ => throw new InvalidOperationException()
                    }; SaveSettings(); },
                    Loader = () => punctuateHits switch {
                        true => 0,
                        false => 1,
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
                } //Display Intensity
                //max line length |----------------------------------------------------------------|
            };
        }
        public void Unload()
        {
            plug?.SetPowerLevel(0);
            ModHooks.HeroUpdateHook -= OnHeroUpdate;
            ModHooks.BeforeAddHealthHook -= BeforeHealthAdd;
            ModHooks.AfterTakeDamageHook -= OnHeroDamaged;
            ModHooks.SoulGainHook -= OnSoulGain;
            On.HeroController.Awake -= OnSaveOpened;
            VibeUI.textUI.Text = string.Empty;
        }
    }
}