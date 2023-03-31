//------------------------------------------------------------------------------
// <copyright file="BlackSpaceSettings.cs" company="Kory Postma">
//
//   Copyright 2016-2023 Kory Postma
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.Drawing;

namespace BlackSpace
{
    #region Structs
    public struct BrushPenSettings
    {
        public System.Drawing.Color BackgroundColor { get; set; }
        public System.Drawing.Color BorderColor { get; set; }
        public double BorderThickness { get; set; }

        public BrushPenSettings(System.Drawing.Color backgroundColor, System.Drawing.Color borderColor, double borderThickness)
        {
            BackgroundColor = backgroundColor;
            BorderColor = borderColor;
            BorderThickness = borderThickness;
        }
    }

    public struct Settings
    {
        //Colors and thickness to use in creating the brushes and pens
        public BrushPenSettings Spaces;
        public BrushPenSettings Tabs;
        public bool DeleteWhiteSpaceWhenSaving { get; set; }

        public Settings(BrushPenSettings spaces, BrushPenSettings tabs, bool deleteWhiteSpaceWhenSaving = false)
        {
            Spaces = spaces;
            Tabs = tabs;
            DeleteWhiteSpaceWhenSaving = deleteWhiteSpaceWhenSaving;
        }

        public Settings(System.Drawing.Color spacesBackgroundColor, System.Drawing.Color spaceBorderColor, double spacesBorderThickness,
            System.Drawing.Color tabsBackgroundColor, System.Drawing.Color tabsBorderColor, double tabsBorderThickness,
            bool deleteWhiteSpaceWhenSaving)
        {
            Spaces = new BrushPenSettings(spacesBackgroundColor, spaceBorderColor, spacesBorderThickness);
            Tabs = new BrushPenSettings(tabsBackgroundColor, tabsBorderColor, tabsBorderThickness);
            DeleteWhiteSpaceWhenSaving = deleteWhiteSpaceWhenSaving;
        }
    }
    #endregion

    class BlackSpaceSettings
    {
        protected const string BS = "BlackSpace";
        protected readonly WritableSettingsStore userSettingsStore;
        protected readonly ColorConverter cc = new ColorConverter();

        #region Singleton
        protected static BlackSpaceSettings instance = null;
        public static BlackSpaceSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BlackSpaceSettings();
                }

                return instance;
            }
        }
        #endregion

        #region Default Settings
        protected static BrushPenSettings DefaultSpacesSettings = new BrushPenSettings(Color.FromArgb(0xa0, 0x2b, 0x00, 0x95), Color.FromArgb(0xff, 0x2b, 0x00, 0xb5), 1.0);
        protected static BrushPenSettings DefaultTabsSettings = new BrushPenSettings(Color.FromArgb(0xa0, 0x2b, 0x00, 0x65), Color.FromArgb(0xff, 0x3b, 0x00, 0x85), 1.0);
        protected static bool DefaultDeleteWhiteSpaceWhenSaving = false;
        protected static Settings DefaultSettings = new Settings(DefaultSpacesSettings, DefaultTabsSettings, DefaultDeleteWhiteSpaceWhenSaving);
        #endregion

        #region Member Variables
        public Settings Settings = DefaultSettings;
        #endregion

        #region Member Properties
        public Color SpacesBackgroundColor
        {
            get { return Settings.Spaces.BackgroundColor; }
            set { Settings.Spaces.BackgroundColor = value; }
        }

        public Color SpacesBorderColor
        {
            get { return Settings.Spaces.BorderColor; }
            set { Settings.Spaces.BorderColor = value; }
        }

        public double SpacesBorderThickness
        {
            get { return Settings.Spaces.BorderThickness; }
            set { Settings.Spaces.BorderThickness = value; }
        }

        public Color TabsBackgroundColor
        {
            get { return Settings.Tabs.BackgroundColor; }
            set { Settings.Tabs.BackgroundColor = value; }
        }

        public Color TabsBorderColor
        {
            get { return Settings.Tabs.BorderColor; }
            set { Settings.Tabs.BorderColor = value; }
        }

        public double TabsBorderThickness
        {
            get { return Settings.Tabs.BorderThickness; }
            set { Settings.Tabs.BorderThickness = value; }
        }

        public bool DeleteWhiteSpaceWhenSaving
        {
            get { return Settings.DeleteWhiteSpaceWhenSaving; }
            set { Settings.DeleteWhiteSpaceWhenSaving = value; }
        }
        #endregion

        #region Constructors
        public BlackSpaceSettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!userSettingsStore.CollectionExists(BS))
            {
                userSettingsStore.CreateCollection(BS);

                SaveSettings(DefaultSettings);
            }

            LoadSettings();
        }
        #endregion

        #region Getters
        internal bool GetBoolean(string name, bool defaultValue)
        {
            if (userSettingsStore.PropertyExists(BS, name))
            {
                try
                {
                    return userSettingsStore.GetBoolean(BS, name);
                }
                catch { }
            }

            return defaultValue;
        }

        internal Color GetColor(string name, Color defaultColor)
        {
            try
            {
                if (userSettingsStore.PropertyExists(BS, name))
                {
                    string colorString = userSettingsStore.GetString(BS, name);
                    return (Color)cc.ConvertFromString(colorString);
                }
            }
            catch
            {
                return defaultColor;
            }

            return defaultColor;
        }

        internal double GetDouble(string name, double defaultValue)
        {
            if (userSettingsStore.PropertyExists(BS, name))
            {
                try
                {
                    return double.Parse(userSettingsStore.GetString(BS, name));
                }
                catch { }
            }

            return defaultValue;
        }

        internal string GetString(string name, string defaultValue)
        {
            if (userSettingsStore.PropertyExists(BS, name))
            {
                try
                {
                    return userSettingsStore.GetString(BS, name);
                }
                catch { }
            }

            return defaultValue;
        }
        #endregion

        #region Setters
        internal void SetBoolean(string name, bool value)
        {
            userSettingsStore.SetBoolean(BS, name, value);
        }

        internal void SetColor(string name, Color value)
        {
            SetString(name, cc.ConvertToString(value));
        }

        internal void SetDouble(string name, double value)
        {
            SetString(name, value.ToString());
        }

        internal void SetString(string name, string value)
        {
            userSettingsStore.SetString(BS, name, value);
        }
        #endregion

        #region Load/Save Settings
        internal void LoadSettings()
        {
            SpacesBackgroundColor = GetColor(nameof(SpacesBackgroundColor), DefaultSettings.Spaces.BackgroundColor);
            SpacesBorderColor = GetColor(nameof(SpacesBorderColor), DefaultSettings.Spaces.BorderColor);
            SpacesBorderThickness = GetDouble(nameof(SpacesBorderThickness), DefaultSettings.Spaces.BorderThickness);
            BrushPenSettings spaces = new BrushPenSettings(SpacesBackgroundColor, SpacesBorderColor, SpacesBorderThickness);

            TabsBackgroundColor = GetColor(nameof(TabsBackgroundColor), DefaultSettings.Tabs.BackgroundColor);
            TabsBorderColor = GetColor(nameof(TabsBorderColor), DefaultSettings.Tabs.BorderColor);
            TabsBorderThickness = GetDouble(nameof(TabsBorderThickness), DefaultSettings.Tabs.BorderThickness);
            BrushPenSettings tabs = new BrushPenSettings(TabsBackgroundColor, TabsBorderColor, TabsBorderThickness);

            DeleteWhiteSpaceWhenSaving = GetBoolean(nameof(DeleteWhiteSpaceWhenSaving), DefaultDeleteWhiteSpaceWhenSaving);

            Settings = new Settings(spaces, tabs, DeleteWhiteSpaceWhenSaving);

            UpdateAdornment();
        }

        internal void SaveSettings(Settings settings)
        {
            SaveSettings(settings.Spaces, settings.Tabs, settings.DeleteWhiteSpaceWhenSaving);
        }

        internal void SaveSettings(BrushPenSettings spaces, BrushPenSettings tabs, bool deleteWhiteSpaceWhenSaving)
        {
            SaveSettings(spaces.BackgroundColor, spaces.BorderColor, spaces.BorderThickness, tabs.BackgroundColor, tabs.BorderColor, tabs.BorderThickness, deleteWhiteSpaceWhenSaving);
        }

        internal void SaveSettings(Color spacesBackgroundColor, Color spacesBorderColor, double spacesBorderThickness,
            Color tabsBackgroundColor, Color tabsBorderColor, double tabsBorderThickness, bool deleteWhiteSpaceWhenSaving)
        {
            SetColor(nameof(SpacesBackgroundColor), spacesBackgroundColor);
            SetColor(nameof(SpacesBorderColor), spacesBorderColor);
            SetColor(nameof(TabsBackgroundColor), tabsBackgroundColor);
            SetColor(nameof(TabsBorderColor), tabsBorderColor);
            SetBoolean(nameof(DeleteWhiteSpaceWhenSaving), deleteWhiteSpaceWhenSaving);
            SetDouble(nameof(SpacesBorderThickness), spacesBorderThickness);
            SetDouble(nameof(TabsBorderThickness), tabsBorderThickness);

            UpdateAdornment();
        }
        #endregion

        BlackSpaceAdornment Adornment { get; set; } = null;

        internal void RegisterAdornment(BlackSpaceAdornment adornment)
        {
            if (adornment != Adornment)
            {
                Adornment = adornment;
                Adornment.UpdateBrushesAndPens(Settings);
            }
        }

        internal void UpdateAdornment()
        {
            Adornment?.UpdateBrushesAndPens(Settings);
        }
    }
}
