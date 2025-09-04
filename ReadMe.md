# Buttplug Knight

A Hollow Knight mod for when you want to feel a little less hollow.

## Requirements

You will need:
* A vibrating toy. (only tested with Lovense, should work with [many more](https://iostindex.com/?filter0ButtplugSupport=7))
* (Optional with Lovense) A bluetooth dongle for your PC. (Bluetooth 4.0 [recommended](https://how.do.i.get.buttplug.in/hardware/bluetooth.html#can-i-use-a-bluetooth-5-dongle))
* A copy of [Hollow Knight](https://store.steampowered.com/app/367520/Hollow_Knight/).
* A Hollow Knight mod manager, [Lumafly](https://github.com/TheMulhima/Lumafly/releases/tag/v3.3.0.0) or [Scarab](https://github.com/fifty-six/Scarab/releases). (free)
* [Intiface Central](https://intiface.com/central/) installed and enabled. (free)

If you have a bluetooth dongle but are unsure if it's compatible, install Intiface Central and scan for devices. If it can find your vibrator, it should work.

## To install

SHORT VERSION: Install intiface central, hollow knight, and the mod manager. Install MagicUI through the mod manager. Enable intiface and search for devices. Drag the ButtplugKnight folder into your mods folder.

DETAILED VERSION:

Install Intiface Central using the above link. Turn on the server, go to devices, scan for devices, and see your device listed. The port should be set to the default `12345`, so it reads `Server Address: localhost:12345`. Note that your device MUST be visible in the devices list BEFORE launching the game. If it isn't, restart the game. (This port can be edited by digging around in a settings file if really needed)

* If it's not listed, check your bluetooth adapter/settings, and check your settings in intiface. 

* Alternatively; If you're using a Lovense, you don't need a bluetooth dongle; try using "Lovense Connect". Make sure Lovense Connect is enabled in the Intiface settings (it's off by default), and connect to your toy through that. This is recommended if you're moving around a lot, having trouble connecting, or don't own a Bluetooth Dongle.

Install Hollow Knight if you haven't already. Install lumafly by downloading the file and running it, and once it's open, install **Magic UI**. You'll know it's working if your game has the spaghetti icon on the main menu and the mod list in the top left.

* This mod uses MagicUI to display the UI as of v1.2.6 - if you cannot install it for whatever reason, use v1.2.5.

Drag the "ButtplugKnight" folder from this repo into your Mods folder. The path should look similar to this;

`\Program Files (x86)\Steam\SteamApps\common\Hollow Knight\hollow_knight_Data\Managed\Mods`

If done correctly, you should be able to go into settings > mods > check that it's enabled. If it's not there, make sure the mod folder is named ButtplugKnight (not ButtplugKnight(1) if you're redownloading - sorry, it's picky!)

If you have any issues installing, feel free to contact me for help. It should generate a `VibeLog.txt` log file in the `Managed\Mods\ButtplugKnight\` folder. The game also generates a `ModLog.txt` at `AppData\LocalLow\Team Cherry\Hollow Knight\`.

## Debugging

* Ensure VPNs are turned off.
* Ensure the vibrator is turned on. (and seeking a connection if applicable)
* If using lovense connect, ensure it's connected in the app before even looking at your PC.
* Ensure the port number in Intiface Central is correct, it's set to Engine mode, and check other settings in the app (e.g. lovense connect)
* Ensure you've started the server and clicked Start Scanning.
* Ensure your vibrator appears in Intiface Central (and can be turned on via the slider) before opening Hollow Knight.
* Ensure the mod exists in the hollow knight mods folder. Ensure it exists as a folder (not zip), and is named exactly ButtplugKnight
* Ensure the ButtplugKnight folder contains ButtplugKnight.dll, ButtplugManaged.dll, and websocket-sharp.dll
* Ensure the mod appears on the mod list in the top right when you open hollow knight, and is enabled in the mod settings.
* If you're still having issues, please contact me - tell me which "ensure" you weren't able to do, and provide me with the VibeLog.txt file (in the mod folder) if possible.

## Rules

Options to cause a vibration whenever you take damage, heal, or gain soul;

* Upon taking damage (default on), it will buzz at 20% power for 5 seconds. Double damage hits are 40% power for 10 seconds. These are adjustable.
* Upon healing (default on), it will buzz equivalent to 1/5th of a hit.
* Upon gaining soul (default off), it will buzz equivalent to roughly 1/5th of a hit (varies slightly depending on fill level and charm loadout). Can be set to not go off when your soul gauge is full, or always.

If one of the above triggers while it is still vibrating, the effect and duration will be **doubled**. This means that if you take a regular hit and it activates for 5 seconds, then take another hit before that time's up, you will have an additional 10 seconds. The power stacks too, so if the first hit was 20%, the second will set it to 40%.

This can be customised in the mod, and settings save between sessions. It's better for the default to be lower, rather than 100%, as that leaves room for it to get more intense should you take multiple hits back to back. There's many customisation options in the mod, explore them for yourself!

What am I doing with my life.
