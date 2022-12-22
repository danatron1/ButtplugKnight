# Buttplug Knight

For when you want to feel a little less hollow.

## Requirements

You will need:
* A vibrating toy. (only tested with Lovense, should work with [many more](https://iostindex.com/?filter0ButtplugSupport=7))
* (Optional with Lovense) A bluetooth dongle for your PC. (Bluetooth 4.0 [recommended](https://how.do.i.get.buttplug.in/hardware/bluetooth.html#can-i-use-a-bluetooth-5-dongle))
* A copy of [Hollow Knight](https://store.steampowered.com/app/367520/Hollow_Knight/).
* Hollow Knight's mod manager, [Scarab](https://github.com/fifty-six/Scarab/releases). (free)
* [Intiface Central](https://intiface.com/central/) installed and enabled. (free)

If you have a bluetooth dongle but are unsure if it's compatible, install Intiface Central and scan for devices. If it can find your vibrator, it should work.

## To install

SHORT VERSION: Install intiface central, hollow knight, and scarab. Enable intiface. Drag the ButtplugKnight folder into your mods folder.

DETAILED VERSION:

Install Intiface Central using the above link. Turn on the server, go to devices, scan for devices, and see your device listed. The port should be set to the default `12345`, so it reads `Server Address: localhost:12345`. Note that your device MUST be visible in the devices list BEFORE launching the game. If it isn't, restart the game.

* If it's not listed, check your bluetooth adapter/settings, and check your settings in intiface. 

* Alternatively; If you're using a Lovense, you don't need a bluetooth dongle; try using "Lovense Connect". Make sure Lovense Connect is enabled in the Intiface settings (it's off by default), and connect to your toy through that. This is recommended if you're moving around a lot, having trouble connecting, or don't own a Bluetooth Dongle.

Install Hollow Knight if you haven't already. Install Scarab by downloading the file and running it - couldn't be easier to use. You may need to install a small mod through its UI just to get it to do its thing. I use Toggleable Bindings for this. You'll know it's working if your game has the spaghetti icon on the main menu and the mod list in the top left.

Drag the "ButtplugKnight" folder from this repo into your Mods folder. The path should look similar to this;

`\Program Files (x86)\Steam\SteamApps\common\Hollow Knight\hollow_knight_Data\Managed\Mods`

If done correctly, you should be able to go into settings > mods > check that it's enabled. 

If you have any issues installing, feel free to contact me for help. It should generate a `VibeLog.txt` log file in the `Managed\Mods\ButtplugKnight\` folder. The game also generates a `ModLog.txt` at `AppData\LocalLow\Team Cherry\Hollow Knight\`.

## Rules

Options to cause a vibration whenever you take damage, heal, or gain soul;

* Upon taking damage (default on), it will buzz at 50% power for 5 seconds. Double damage hits are 100% power for 10 seconds.
* Upon healing (default on), it will buzz at 10% power for 1 second.
* Upon gaining soul (default off), it will buzz at ~10% power for ~1 second (varies slightly depending on fill level and charm loadout). Can be set to not go off when your soul gauge is full, or always.

If one of the above triggers while it is still vibrating, the effect and duration will be **doubled**. This means that if you take a regular hit, then take another before the 5 seconds are up, you will have an additional 10 seconds at 100% power. 

This can be customised in the mod, and settings save between sessions. It's better for the default to be 50% rather than 100%, as that leaves room for it to get more intense should you take a heavy hit or multiple hits back to back. There's many customisation options in the mod, explore them for yourself!

Settings for intensity and duration are for the 'upon taking damage' trigger - others are about 1/5th of that (e.g. if you want 2 seconds of buzz on healing, set timer to 10 seconds).

What am I doing with my life.
