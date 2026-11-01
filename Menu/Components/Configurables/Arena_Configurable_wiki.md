# Overview

The Slugcat Tab reunites a list of settings to balance and/or add certain slugcat abilities in arena mode. Meadow manages the Vanilla, MSC and Watcher settings depending on your mod list. In this page, we'll go on how they work and how you can add yours for your mod !

# Creating a SettingsPage

## With OnlineArenaSettingsBuilder

`OnlineArenaSettingsBuilder` is a subclass of `SettingsPage` which aim to facilitate the creation and management of setting pages, by taking the essential informations on the settings and building the menu from it.

The setup of those elements can be done in a static `Init()` method, or directly into the static constructor.

This is an example of the barebone of your class : 
```cs
public class MySuperCoolSlugcatSettings : OnlineArenaSettingsBuilder<MySuperCoolSlugcatSettings>
{
    // This is the name that'll appear in the slugcat tab
    public override string Name => "Cool Scug Setting"; 

    // This is the static constructor, this is where you can tell your menu what element it should build
    static MySuperCoolSlugcatSettings()
    {

    }

    // This is the actual constructor, we'll come back to this later
    public MySuperCoolSlugcatSettings(Menu.Menu menu, MenuObject owner) : base(menu, owner)
    {

    }
}
```
> [!CAUTION]
> Be sure to put the name of your class again in the `<TSelf>` of `OnlineArenaSettingsBuilder<TSelf>` !

### Data holder
The data where your settings will be held and synced is yours to make. A tutorial on how to make States is provided in the wiki.
However, the menu will need a function to get that object when trying to save the data (or it will not work !). To do so, you can call the static `OnlineSettingConfigurable.SettingsConfigData.AddNewGetAttributeOwnerFunction()`.
The function should take no argument, and should return `null` if your data holder is unavailable

```cs
OnlineSettingConfigurable.SettingsConfigData.AddNewGetAttributeOwnerFunction(
    () => MyCoolArenaMod.GetMyArenaDataHolder()
)
```

### Adding configurables
To add new configurables to your menu, you can use the function `AddSlugcatSettingsConfigurable`, which takes an `OnlineSettingConfigurable.SettingsConfigData` struct in.
This struct needs :
- A **name**, which will be the name of the configurable displayed in-game.
- (Optional) A **tab name** or a **`Slugcat.Name`**, to put this configurable in the right tab. If no tabs was provided before, it'll be created automatically.
- A **`Configurable`** from `Menu.Remix.UI`, which will be the object where the data will be saved when exiting the game.
- The **Type** where your data will be saved and synced mid-game.
- The **name of the field** where your data will be saved and synced mid-game.
- (Optional) The **description** of your setting that'd be displayed when the player hovers above it.
- (Optional) A bool to say if the setting is available to every client or not (client sided setting).
```cs
static MySuperCoolSlugcatSettings()
{
    AddSlugcatSettingsConfigurable(new(
        "Explode on touch",
        MyCoolArenaMod.MySlugcatName,
        MyCoolArenaMod.remixOptions.MeadowExplode,
        typeof(MyCoolArenaMod.ArenaDataHolder),
        nameof(MyCoolArenaMod.ArenaDataHolder.doesSlugExplode),
        "This slugcat will explode on hit or touch.")
    );
}
```

> [!TIP]
> The configurables will be ordered by tabs, then by order of addition.
> [!NOTE]
> If anything goes wrong in the creation of the configurable, it will be marked in logs to help you solve it.