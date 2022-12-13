# Buttplug Knight

For when you want to feel a little less hollow.

## Requirements

You will need:
* A vibrating toy (only tested with Lovense, should work with [many more](https://iostindex.com/?filter0ButtplugSupport=7))
* A bluetooth dongle for your PC (Bluetooth 4.0 [recommended](https://how.do.i.get.buttplug.in/hardware/bluetooth.html#can-i-use-a-bluetooth-5-dongle))
* A copy of [Hollow Knight](https://store.steampowered.com/app/367520/Hollow_Knight/)
* [Scarab](https://github.com/fifty-six/Scarab/releases), Hollow Knight's mod manager
* [Intiface Central](https://intiface.com/central/) installed and enabled

If you have a bluetooth dongle but are unsure if it's compatible, install Intiface Central and scan for devices. If it can find your vibrator, it should work.

## To install

SHORT VERSION: Install intiface central, hollow knight, and scarab. Drag the ButtplugKnight folder into your mods folder.

DETAILED VERSION:

Install Intiface Central using the above link. You'll know it's working if you can turn on the server, go to devices, scan for devices, and see your device listed. The port should be set to the default `12345`, so it reads `Server Address: localhost:12345`.

Install Hollow Knight if you haven't already. Install Scarab by downloading the file and running it - couldn't be easier to use. You may need to install a small mod through its UI just to get it to do its thing. I use Toggleable Bindings for this. You'll know it's working if your game has the spaghetti icon on the main menu and the mod list in the top left.

Drag the "ButtplugKnight" folder from this repo into your Mods folder. The path should look similar to this;

`\Program Files (x86)\Steam\SteamApps\common\Hollow Knight\hollow_knight_Data\Managed\Mods`

You need the 4 dll files; `Buttplug.dll`, `ButtplugKnight.dll`, `ButtplugManaged.dll`, and `websocket-sharp.dll` in order for this to work. If done correctly, you should be able to go into settings > mods > check that it's enabled. 

## Rules

When you take damage, it will buzz at 50% power for 5 seconds. (Double damage hits are 100% power for 10 seconds)

When you heal, it will buzz at 10% power for 1 second.

If one of the above triggers while it is still vibrating, the effect and duration will be **doubled**. This means that if you take a regular hit, then take another before the 5 seconds are up, you will have an additional 10 seconds at 100% power. 

This can be customised in the mod, but I do recommend starting with the default settings. It's better for the default to be 50% rather than 100%, as that leaves room for it to get more intense should you take a heavy hit or multiple hits back to back.

What am I doing with my life.
