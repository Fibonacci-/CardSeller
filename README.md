# CardSeller

This is a mod for TCG Card Shop Simulator by OPNeon Games

https://store.steampowered.com/app/3070070/TCG_Card_Shop_Simulator/

This mod will take cards out of your card album and place them on all available card table slots in order of highest market price. It ignores "personal" display-only card slots.

***HEADS UP:*** Mods can break your game. Back up your save file before installing this mod.


## Installing

Install by downloading CardSeller.dll and placing it in `\<gamedir>\BepInEx\plugins\`.

## Recommended Mods

ConfigurationManager to change the mod's config in-game without hand-editing .cfg files. Usually bundled with BepInEx.

[AutoSetPrices](https://www.nexusmods.com/tcgcardshopsimulator/mods/9) to make sure the cards this mod sets out have prices on the shelf. If AutoSetPrices is installed, this mod will try to trigger a price update for the cards it places on shelves. NOTE: for this to work, Auto Set Price's option `NewDayCardAutoPrice` need to be enabled in mod settings.

## Configuration

If you have ConfigurationManager installed, start the game, then press F1 to bring up the mod settings menu. Otherwise, start the game once, then access the configuration file at `\<gamedir>\BepInEx\config\io.helwig.tcgcss.CardSeller.cfg`.

### Configuration options available:
Upper and lower bounds on the market price of the cards this mod will select for placing on card tables for sale

Select only duplicate cards (ignore cards in the album with a quantity of 1)

Toggle for AutoSetPrice mod integration

Hotkey binding

Toggle for triggering this mod whenever a customer picks up a card

Toggle for triggering this mod when a new day starts


## Planned features

Options to exclude by border type/foil/etc