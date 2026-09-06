using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RainMeadow.UI.Components.Patched;
using UnityEngine;

namespace RainMeadow.UI.Components
{
    public class OnlineArenaBaseGameModeTab
        : RectangularMenuObject
    {
        public MenuTabWrapper tabWrapper;

        public GameSettings gameSettings;

        public MenuLabel arenaImportExportLabel;
        public MenuLabel arenaSettingsImportExportLabel;

        public OpSimpleButton arenaPlaylistImportButton;
        public OpSimpleButton arenaPlaylistExportButton;
        public OpSimpleButton arenaSettingsExportButton;
        public OpSimpleButton arenaSettingsImportButton;

        public EventfulScrollButton? prevButton,


            nextButton;
        public ArenaOnlineGameMode arena => OnlineManager.lobby.gameMode as ArenaOnlineGameMode;

        public bool AllSettingsDisabled =>
            arena.initiateLobbyCountdown && arena.arenaClientSettings.ready;
        public bool OwnerSettingsDisabled =>
            !(OnlineManager.lobby?.isOwner == true) || AllSettingsDisabled;


        public OnlineArenaBaseGameModeTab(
            Menu.Menu menu,
            MenuObject owner,
            Vector2 pos,
            Vector2 size
        )
            : base(menu, owner, pos, size)
        {
            tabWrapper = new(menu, this);

            gameSettings = new GameSettings(menu, this);

            InGameTranslator.LanguageID? lang = menu?.manager?.rainWorld?.inGameTranslator.currentLanguage;
            float leftMargin = 10f;
            float labelWidth = 140f;
            float topOffset = size.y - 60f;
            float rowHeight = 40f;
            float boxMargin = leftMargin + labelWidth
                + (lang == InGameTranslator.LanguageID.French || lang == InGameTranslator.LanguageID.Spanish
                    ? 85f
                    : 50f);
            float btnWidth = 90f;
            float btnGap = 6f;

            arenaImportExportLabel = new(menu, this, menu.Translate("Playlist:"),
                new(leftMargin, topOffset - rowHeight * 8), new(labelWidth, 20f), false);
            arenaImportExportLabel.label.alignment = FLabelAlignment.Left;

            arenaPlaylistExportButton = new(new Vector2(boxMargin, topOffset - (rowHeight * 8) - 2f), new Vector2(btnWidth, 30f), menu.Translate("Copy"));
            arenaPlaylistExportButton.OnClick += (_) =>
            {
                try
                {
                    var arenaMenu = menu as ArenaOnlineLobbyMenu;
                    string result = EncodePlaylist(arenaMenu?.arenaMainLobbyPage?.levelSelector?.SelectedPlayList);
                    if (string.IsNullOrEmpty(result))
                    {
                        arenaImportExportLabel.text = menu.Translate("Failed");
                        arenaImportExportLabel.label.color = Color.red;
                        return;
                    }
                    GUIUtility.systemCopyBuffer = result;
                    arenaImportExportLabel.text = menu.Translate("Copied");
                    arenaImportExportLabel.label.color = Color.green;
                }
                catch (Exception e)
                {
                    RainMeadow.Error(e);
                    arenaImportExportLabel.text = menu.Translate("Failed");
                    arenaImportExportLabel.label.color = Color.red;
                }
            };

            arenaPlaylistImportButton = new(new Vector2(boxMargin + btnWidth + btnGap, topOffset - (rowHeight * 8) - 2f), new Vector2(btnWidth, 30f), menu.Translate("Import"));
            arenaPlaylistImportButton.OnClick += (_) =>
            {
                try
                {
                    var arenaMenu = menu as ArenaOnlineLobbyMenu;
                    ArenaLevelSelector? levelSelector = arenaMenu?.arenaMainLobbyPage?.levelSelector;
                    string clipboardText = GUIUtility.systemCopyBuffer;

                    if (string.IsNullOrEmpty(clipboardText) || levelSelector == null)
                    {
                        arenaImportExportLabel.text = menu.Translate("Failed");
                        arenaImportExportLabel.label.color = Color.red;
                        return;
                    }
                    List<string> playlist = DecodePlaylist(clipboardText)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList();

                    if (playlist.Count == 0 || playlist.Any(name => name.Contains("=") || name.Contains("|")))
                    {
                        arenaImportExportLabel.text = menu.Translate("Failed");
                        arenaImportExportLabel.label.color = Color.red;
                        return;
                    }

                    List<string> knownLevels = playlist.Where(levelSelector.allLevels.Contains).ToList();
                    if (knownLevels.Count == 0)
                    {
                        arenaImportExportLabel.text = menu.Translate("Failed");
                        arenaImportExportLabel.label.color = Color.red;
                        return;
                    }

                    levelSelector.LoadNewPlaylist(knownLevels, true);
                    levelSelector.selectedLevelsPlaylist.ResolvePlaylistMismatch();
                    menu.PlaySound(SoundID.MENU_Add_Level);

                    bool importedAll = knownLevels.Count == playlist.Count;
                    arenaImportExportLabel.text = menu.Translate(importedAll ? "Imported" : "Missing levels");
                    arenaImportExportLabel.label.color = importedAll ? Color.green : Color.yellow;
                }
                catch (Exception e)
                {
                    RainMeadow.Error(e);
                    arenaImportExportLabel.text = menu.Translate("Failed import");
                    arenaImportExportLabel.label.color = Color.red;
                }
            };

            arenaSettingsImportExportLabel = new(menu, this, menu.Translate("Settings:"),
                new(leftMargin, topOffset - rowHeight * 9), new(labelWidth, 20f), false);
            arenaSettingsImportExportLabel.label.alignment = FLabelAlignment.Left;

            arenaSettingsExportButton = new(new Vector2(boxMargin, topOffset - (rowHeight * 9) - 2f), new Vector2(btnWidth, 30f), menu.Translate("Copy"));
            arenaSettingsExportButton.OnClick += (_) =>
            {
                try
                {
                    string result = arena.externalArenaGameMode.ExportLocalSettings(arena);
                    GUIUtility.systemCopyBuffer = result;
                    arenaSettingsImportExportLabel.text = menu.Translate("Copied");
                    arenaSettingsImportExportLabel.label.color = Color.green;
                }
                catch (Exception e)
                {
                    RainMeadow.Error(e);
                    arenaSettingsImportExportLabel.text = menu.Translate("Failed");
                    arenaSettingsImportExportLabel.label.color = Color.red;
                }
            };

            arenaSettingsImportButton = new(new Vector2(boxMargin + btnWidth + btnGap, topOffset - (rowHeight * 9) - 2f), new Vector2(btnWidth, 30f), menu.Translate("Import"));
            arenaSettingsImportButton.OnClick += (_) =>
            {
                try
                {
                    var arenaMenu = menu as ArenaOnlineLobbyMenu;
                    string clipboardText = GUIUtility.systemCopyBuffer;

                    if (!string.IsNullOrEmpty(clipboardText))
                    {
                        bool success = arena.externalArenaGameMode.ImportLocalSettings(arena, clipboardText);
                        if (!success)
                        {
                            arenaSettingsImportExportLabel.text = menu.Translate("Failed");
                            arenaSettingsImportExportLabel.label.color = Color.red;
                            return;
                        }

                        var settingsInterface = arenaMenu?.arenaMainLobbyPage?.arenaSettingsInterface;
                        if (settingsInterface != null)
                        {
                            settingsInterface.countdownTimerTextBox.valueInt = arena.setupTime;
                            settingsInterface.CallForSync();
                        }

                        arenaSettingsImportExportLabel.text = menu.Translate("Imported");
                        arenaSettingsImportExportLabel.label.color = Color.green;
                    }
                }
                catch (Exception e)
                {
                    RainMeadow.Error(e);
                    arenaSettingsImportExportLabel.text = menu.Translate("Failed");
                    arenaSettingsImportExportLabel.label.color = Color.red;
                }
            };

            this.SafeAddSubobjects(tabWrapper, gameSettings, arenaImportExportLabel, arenaSettingsImportExportLabel);

            new PatchedUIelementWrapper(tabWrapper, arenaPlaylistExportButton);
            new PatchedUIelementWrapper(tabWrapper, arenaPlaylistImportButton);
            new PatchedUIelementWrapper(tabWrapper, arenaSettingsImportButton);
            new PatchedUIelementWrapper(tabWrapper, arenaSettingsExportButton);

            gameSettings.SelectAndCreateBackButtons(null, false);
        }
        public void PopulatePage(int offset)
        {
            ClearInterface();

            float posXMultipler = size.x / 4;
            tabWrapper._tab.myContainer.MoveToFront();
        }

        public void ClearInterface() { }

        public void UnloadAnyConfig(params UIelement[]? elements)
        {
            if (elements == null)
                return;
            foreach (UIelement element in elements)
            {
                if (tabWrapper.wrappers.ContainsKey(element))
                {
                    tabWrapper.ClearMenuObject(tabWrapper.wrappers[element]);
                    tabWrapper.wrappers.Remove(element);
                }
                element.Unload();
            }
        }

        public void OnShutdown()
        {
            if (!(OnlineManager.lobby?.isOwner == true))
                return;
            gameSettings.SaveInterfaceOptions();
            RainMeadow.rainMeadowOptions.config.Save();
        }

        public void DeletePageButtons()
        {
            this.ClearMenuObject(ref prevButton);
            this.ClearMenuObject(ref nextButton);
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
        }

        public int timeToClearMessage = 120, timeToClearSettingsMessage = 120;
        public override void Update()
        {
            base.Update();

            if (arenaImportExportLabel.text != menu.Translate("Playlist:"))
            {
                timeToClearMessage--;
                if (timeToClearMessage <= 0)
                {
                    arenaImportExportLabel.text = menu.Translate("Playlist:");
                    arenaImportExportLabel.label.color = Color.white;
                    timeToClearMessage = 120;
                }
            }
            if (arenaPlaylistImportButton != null)
            {
                arenaPlaylistImportButton.greyedOut = OwnerSettingsDisabled;
            }

            if (arenaSettingsImportExportLabel.text != menu.Translate("Settings:"))
            {
                timeToClearSettingsMessage--;
                if (timeToClearSettingsMessage <= 0)
                {
                    arenaSettingsImportExportLabel.text = menu.Translate("Settings:");
                    arenaSettingsImportExportLabel.label.color = Color.white;
                    timeToClearSettingsMessage = 120;
                }
            }
            if (arenaSettingsImportButton != null)
            {
                arenaSettingsImportButton.greyedOut = OwnerSettingsDisabled;
            }
        }

        /// <summary>
        /// Encodes a List<string>  into a base64 encoding of Arena map names.
        /// </summary>
        public static string EncodePlaylist(List<string>? arenaMaps)
        {
            if (arenaMaps == null || arenaMaps.Count == 0)
            {
                return string.Empty;
            }

            // Join the list into a single string delimited by semicolons
            string joinedMaps = string.Join(";", arenaMaps);

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(joinedMaps);


            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes a Base64 string back into a List of Arena map names.
        /// </summary>
        public static List<string> DecodePlaylist(string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                return new List<string>();
            }

            try
            {
                byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                string decodedString = Encoding.UTF8.GetString(base64EncodedBytes);
                return decodedString.Split(';').ToList();
            }
            catch (FormatException)
            {
                Debug.LogError("Failed to load playlist: The provided string is not a valid Base64 format.");
                return new List<string>();
            }
        }

    }
}
