using System.Linq;
using Menu;
using UnityEngine;
using static RainMeadow.UI.Components.OnlineSlugcatAbilitiesInterface;

namespace RainMeadow.UI.Components;

public class GameSettings : OnlineSlugcatSettings<GameSettings>
{
    public const string SCORING = "Scoring", DENS = "Dens";
    public override string Name => "Game Settings";

    private OnlineSettingIntValue? spearHitScoreSetting;

    static GameSettings()
    {
        AddSlugcatSettingsConfigurable(new(
            "Food Score",
            SCORING,
            RainMeadow.rainMeadowOptions.ArenaFoodScore,
            nameof(ArenaOnlineGameMode.foodScore),
            "Food points multiplier")
        );
        AddSlugcatSettingsConfigurable(new(
            "Spear Hit Score",
            SCORING,
            RainMeadow.rainMeadowOptions.ArenaSpearHitScore,
            nameof(ArenaOnlineGameMode.spearHitScore),
            "Points a spear is worth (non-lethal)")
        );
        AddSlugcatSettingsConfigurable(new(
            "Kill Score",
            SCORING,
            RainMeadow.rainMeadowOptions.ArenaKillScore,
            nameof(ArenaOnlineGameMode.killScore),
            "Points a kill is worth")
        );
        AddSlugcatSettingsConfigurable(new(
            "Survival Score",
            SCORING,
            RainMeadow.rainMeadowOptions.ArenaSurvivalScore,
            nameof(ArenaOnlineGameMode.survivalScore),
            "Points for surviving inside the shelter")
        );
        AddSlugcatSettingsConfigurable(new(
            "Empty Death Score",
            SCORING,
            RainMeadow.rainMeadowOptions.ArenaEmptyDeathScore,
            nameof(ArenaOnlineGameMode.emptyDeathScore),
            "Points lost from self-inflicted death")
        );
        AddSlugcatSettingsConfigurable(new(
            "Unlock Dens",
            DENS,
            RainMeadow.rainMeadowOptions.ArenaDenScore,
            nameof(ArenaOnlineGameMode.denScore),
            "Points required to unlock dens")
        );
        AddSlugcatSettingsConfigurable(new(
            "Den Entry",
            DENS,
            RainMeadow.rainMeadowOptions.ArenaDenType,
            nameof(ArenaOnlineGameMode.denEntryRule),
            "Den entry behavior")
        );
        AddSlugcatSettingsConfigurable(new(
            "Den Ejection",
            DENS,
            RainMeadow.rainMeadowOptions.ChallengeDenEjection,
            nameof(ArenaOnlineGameMode.challengeDenEjection),
            "Dens eject and block players after some time")
        );
    }

    public GameSettings(Menu.Menu menu, MenuObject owner) : base(menu, owner, 2f)
    {
        ConfigurableBase[] intConfigurables =
        [
            RainMeadow.rainMeadowOptions.ArenaFoodScore,
            RainMeadow.rainMeadowOptions.ArenaSpearHitScore,
            RainMeadow.rainMeadowOptions.ArenaKillScore,
            RainMeadow.rainMeadowOptions.ArenaSurvivalScore,
            RainMeadow.rainMeadowOptions.ArenaEmptyDeathScore,
            RainMeadow.rainMeadowOptions.ArenaDenScore,
        ];
        foreach (ConfigurableBase intConfigurable in intConfigurables)
        {
            if (GetSettingParameter(intConfigurable) is OnlineSettingIntValue intValue)
            {
                intValue.textBox.OnValueUpdate += (config, value, oldValue) =>
                {
                    if (intValue.textBox.valueInt < 0) intValue.textBox.valueInt = 0;
                };
            }
        }

        spearHitScoreSetting = GetSettingParameter(RainMeadow.rainMeadowOptions.ArenaSpearHitScore) as OnlineSettingIntValue;

        if (GetSettingParameter(RainMeadow.rainMeadowOptions.ChallengeDenEjection) is OnlineSettingCheckBox denEjection)
        {
            denEjection.altDescription = menu.Translate("Normal den behavior");
        }
    }

    public override void Update()
    {
        base.Update();

        if (!ModManager.MSC) spearHitScoreSetting?.grayedOut = true;
    }

    public override void SelectAndCreateBackButtons(SettingsPage? previousSettingPage, bool forceSelectedObject)
    {
        if (resetButton is null)
        {
            resetButton = new(menu, this, menu.Translate("RESET"), new(settingsBoxSize.x - 40, 20), new(80, 30));
            resetButton.OnClick += (b) => ResetSettings();
            AddObjects(resetButton);
        }

        BindSettingsButtons(IsActuallyHidden);
        if (forceSelectedObject) menu.selectedObject = elements.FirstOrDefault()?.selectable ?? resetButton;
    }
}
