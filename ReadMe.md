# Buttplug Knight

A Hollow Knight mod for when you want to feel a little less hollow.

NOTE: Requires Hollow Knight version 1.5.78 or earlier (for now)  

## Requirements

You will need:
* A vibrating toy. (tested with Lovense, should work with [many more](https://iostindex.com/?filter0ButtplugSupport=7))
* A copy of [Hollow Knight](https://store.steampowered.com/app/367520/Hollow_Knight/).
* A Hollow Knight mod manager, [Lumafly](https://github.com/TheMulhima/Lumafly/releases/tag/v3.3.0.0) or [Scarab](https://github.com/fifty-six/Scarab/releases). (free)
* [Intiface Central](https://intiface.com/central/) installed and enabled. (free)
* A smartphone with Intiface Central installed and [relaying](https://docs.intiface.com/docs/intiface-central/ui/app-modes-repeater-panel/) OR a bluetooth dongle for your PC. (Bluetooth 4.0+ [recommended](https://how.do.i.get.buttplug.in/hardware/bluetooth.html#can-i-use-a-bluetooth-5-dongle))

If you have a bluetooth dongle but are unsure if it's compatible, install Intiface Central and scan for devices. If it can find your vibrator, it should work.

## To install

SHORT VERSION: Install intiface central, hollow knight, and the mod manager. Install MagicUI through the mod manager. Enable intiface and search for devices. Drag the ButtplugKnight folder into your mods folder.

DETAILED VERSION:

Install Intiface Central using the above link. Turn on the server, go to devices, scan for devices, and see your device listed. The port should be set to the default `12345`, so it reads `Server Address: localhost:12345`. Note that your device MUST be visible in the devices list BEFORE launching the game. If it isn't, restart the game. (This port can be edited by digging around in a settings file if really needed)

* If it's not listed, check your bluetooth adapter/settings, and check your settings in intiface. 

* Alternatively; Install Intiface Central on a smartphone, and follow the [instructions here](https://docs.intiface.com/docs/intiface-central/ui/app-modes-repeater-panel/) to relay it to your PC.

Install Hollow Knight if you haven't already. Install lumafly by downloading the file and running it, and once it's open, install **Magic UI**. You'll know it's working if your game has the spaghetti icon on the main menu and the mod list in the top left.

* This mod uses MagicUI to display the UI as of v1.2.6 - if you cannot install it for whatever reason, use v1.2.5.

Drag the "ButtplugKnight" folder from this repo into your Mods folder. The path should look similar to this;

`\Program Files (x86)\Steam\SteamApps\common\Hollow Knight\hollow_knight_Data\Managed\Mods`

If done correctly, you should be able to go into settings > mods > check that it's enabled. If it's not there, make sure the mod folder is named ButtplugKnight (not ButtplugKnight(1) if you're redownloading - sorry, it's picky!)

If you have any issues installing, feel free to contact me for help. It should generate a `VibeLog.txt` log file in the `Managed\Mods\ButtplugKnight\` folder. The game also generates a `ModLog.txt` at `AppData\LocalLow\Team Cherry\Hollow Knight\`.

## Debugging

Some things have updated that may break this mod, mainly:

* Lovense Connect is no longer being supported. Blame Lovense. As an alternative, use repeater mode.
* Hollow Knight updated, which broke some mods. Until I get around to fixing it, downpatch to version 1.5.78.

If it's still not working, try the usual debugging steps:

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

## Settings explanation

There are various ways this mod can work, with options to buzz upon taking damage, healing, dealing damage, and more. Everything is fully customisable.
By default, it is set up to punish taking damage with distracting vibrations. This is the main purpose of the mod.

* Upon taking damage, it will buzz at 20% power for 5 seconds. Both of these settings are adjustable.
* Upon healing, it will give a smaller buzz. The amount/duration are relative to taking damage. By default, it will buzz equivalent to 1/4 of a hit.

Other modes include:  
* Buzz upon gaining soul; It will buzz for a duration equivalent to a fraction of a hit. This depends on soul gain, so varies slightly depending on charm loadout.
* Buzz upon death; If enabled, dying will immediately set the vibrator to 100% for a number of seconds. This will add time to the existing vibrator timer, which will likely be active. I give the option of 1 second for if you're only interested in the "set to 100%" feature.
* Buzz upon relic pickup; If enabled, vibrates when you pick up a relic. Intended for Archipelago (see below). 

Other settings:  
* Double on overlap; If one of the above triggers while it is still vibrating, the effect and duration will be combined, and added duration **doubled**. This means that if you take a regular hit and it activates for 5 seconds, then take another hit before that time's up, you will have an additional 10 seconds. The power stacks too, so if the first hit was 20%, the second will set it to 40%, then 60%, and so on. Default on, and recommended.
* Scale with damage; If a regular hit activates at 20% for 5 seconds, a double-damage hit will activate at 40% for 10 seconds. Default on, and recommended.
* Random surprises; Every frame has a minuscule chance of just turning the vibrator on maximum for 10 seconds. Leave this setting on. You will forget about it.
* Vulnerable vibing; If you take damage while your vibrator is active, the damage you take is doubled. Adds extra challenge! Default off.
* Punctuate hits; Not feeling the punch? Accentuates damage taken by setting the vibrator to 100% for a fraction of a second before continuing the regular vibes.
* Display intensity/timer; If you like the unknown or are feeling daring, you can turn these off and nobody will even know you have this mod installed... if you can keep a straight face. (I take no responsibility for any banned twitch accounts that come out of this)
* Oscillate; If the vibrator staying on is too boring for you, you have the option to activate the vibrator in a *wave pattern*. These oscillate between off and your current vibe level.
  * Sine wave - smoothly goes up and down every second.
  * Sine wave 2 - Like above, except the power level is the wave's average instead of peak. Much more aggressive!
  * Square wave - alternate off/on every second.
  * Square wave 2 - Always 100% or 0%, but power level instead determines how much of each second it stay on for.
  * Triangle wave - vibes decrease to zero before resetting every second.
  * Triangle wave 2 - vibes instead increase from zero before resetting every second.
* Rotation; If your toy can rotate, turn this on and it will activate rotations alongside vibrations (experimental - I lack the hardware to test this).

Settings save between sessions. I would recommend setting the default power to something lower, rather than 100%, as that leaves room for it to get more intense should you take multiple hits back to back. There's many customisation options in the mod, explore them for yourself!

## Archipelago

This mod also has pseudo [Archipelago](https://archipelago.gg/) support, in a "Buzz upon relic pickup" option. It has duration multipliers too, but they aren't recommended.  
* When enabled, it buzzes when picking up the relics you can sell to [Lemm](https://hollowknight.wiki/w/Relic_Seeker_Lemm#Goods), with greater vibrations for the more valuable ones.  
* The intent is for this to be played with [Hollow Knight's Archipelago mod](https://archipelago.gg/games/Hollow%20Knight/info/en). Archipelago is a multi-game randomizer, with relics functioning like "traps" other players can send to you, potentially giving you a vibe jumpscare.  
* The relics you can find throughout Hollow Knight are:  
  * 14 Wanderer's Journals: Worth 200 geo each. Vibes at 20% for 2 seconds.
  * 17 Hallownest Seals: Worth 450 geo each. Vibes at 45% for 4.5 seconds.
  * 8 King's Idols: Worth 800 geo each. Vibes at 80% for 8 seconds.
  * 4 Arcane Eggs: Worth 1200 geo each. Vibes at 100% for 12 seconds.
 
Whenever another player in your Archipelago room finishes their game, it will automatically release every item they missed. This will likely mean sending several uncollected relics to you at once. In extreme cases, this can potentially give you over a minute of 100% power. In other words, make sure everyone is thoroughly exploring their games before releasing, else you might release too!

What am I doing with my life.
