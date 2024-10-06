# CardSeller

This is a mod for TCG Card Shop Simulator by OPNeon Games

https://store.steampowered.com/app/3070070/TCG_Card_Shop_Simulator/

This mod will take cards out of your card album and place them on all available card table slots. It ignores "personal" display-only card slots.

***HEADS UP:*** Mods can break your game. Back up your save file before installing this mod.


## Installing

Install by downloading CardSeller.dll and placing it in `\<gamedir>\BepInEx\plugins\`.

## Configuration

If you have ConfigurationManager installed, start the game, then press F1 to bring up the mod settings menu. Otherwise, start the game once, then access the configuration file at `\<gamedir>\BepInEx\config\io.helwig.tcgcss.CardSeller.cfg`.

### Settings
`ShouldTriggerOnCardPickup`
- **Default value:** false
- **Description:** Do you want your cards to automatically be placed on all empty shelves whenever a customer picks up a card?
- **Change if:** You want to let the mod keep your shelves stocked all the time.

`SellOnlyGreaterThan`
- **Default value:** 0.50
- **Description:** Ignore cards in the album with a market value below this.
- **Change if:** You want the mod to set out cards less than $0.50. You can also change this to a higher value to prevent the mod from selecting low-value cards to sell.

`SellOnlyLessThan`
- **Default value:** 100.00
- **Description:** Ignore cards in the album with a market value above this.
- **Change if:** You want the mod to set out cards worth more than $100.

`SetOutCardsKey`
- **Default value:** F9
- **Description:** Keyboard shortcut to set out cards.
- **Change if:** You are already using F9 for something else.