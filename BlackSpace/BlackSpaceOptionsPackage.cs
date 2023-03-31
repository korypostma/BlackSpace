//------------------------------------------------------------------------------
// <copyright file="BlackSpaceOptionsPackage.cs" company="Kory Postma">
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

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace BlackSpace
{
    public class OptionDialogPage : DialogPage
    {
        BlackSpaceOptionsPackage package = null;

        [Category("General")]
        [DisplayName("Spaces: Background Color")]
        [Description("Color used for the background brush on end-of-line spaces.")]
        public System.Drawing.Color SpacesBackgroundColor { get; set; }

        [Category("General")]
        [DisplayName("Spaces: Border Color")]
        [Description("Color used for the border/stroke pen on end-of-line spaces.")]
        public System.Drawing.Color SpacesBorderColor { get; set; }

        [Category("General")]
        [DisplayName("Spaces: Border Thickness")]
        [Description("Thickness of the line/stroke used for the border/stroke pen on end-of-line spaces.")]
        public double SpacesBorderThickness { get; set; }

        [Category("General")]
        [DisplayName("Tabs: Background Color")]
        [Description("Color used for the background brush on end-of-line tabs.")]
        public System.Drawing.Color TabsBackgroundColor { get; set; }

        [Category("General")]
        [DisplayName("Tabs: Border Color")]
        [Description("Color used for the border/stroke pen on end-of-line tabs.")]
        public System.Drawing.Color TabsBorderColor { get; set; }

        [Category("General")]
        [DisplayName("Tabs: Border Thickness")]
        [Description("Thickness of the line/stroke used for the border/stroke pen on end-of-line tabs.")]
        public double TabsBorderThickness { get; set; }

        [Category("General")]
        [DisplayName("Delete end-of-line whitespace when saving")]
        [Description("Removes whitespace at the end-of-lines when saving files or solutions.")]
        public bool DeleteWhiteSpaceWhenSaving { get; set; }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();

            BlackSpaceSettings.Instance.LoadSettings();
            SpacesBackgroundColor = BlackSpaceSettings.Instance.SpacesBackgroundColor;
            SpacesBorderColor = BlackSpaceSettings.Instance.SpacesBorderColor;
            SpacesBorderThickness = BlackSpaceSettings.Instance.SpacesBorderThickness;
            TabsBackgroundColor = BlackSpaceSettings.Instance.TabsBackgroundColor;
            TabsBorderColor = BlackSpaceSettings.Instance.TabsBorderColor;
            TabsBorderThickness = BlackSpaceSettings.Instance.TabsBorderThickness;
            DeleteWhiteSpaceWhenSaving = BlackSpaceSettings.Instance.DeleteWhiteSpaceWhenSaving;
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            BlackSpaceSettings.Instance.SaveSettings(SpacesBackgroundColor, SpacesBorderColor, SpacesBorderThickness,
                TabsBackgroundColor, TabsBorderColor, TabsBorderThickness, DeleteWhiteSpaceWhenSaving);
        }

        public void RegisterPackage(BlackSpaceOptionsPackage inPackage)
        {
            package = inPackage;
        }
    }

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(BlackSpaceOptionsPackage.PackageGuidString)]
    [ProvideOptionPage(typeof(OptionDialogPage), "Black Space", "General", 0, 0, true)]
    public sealed class BlackSpaceOptionsPackage : AsyncPackage
    {
        /// <summary>
        /// BlackSpaceOptionsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "3f7dbe25-8df3-454a-a340-31c8e0ba610e";

        public static OptionDialogPage OptionPage = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackSpaceOptionsPackage"/> class.
        /// </summary>
        public BlackSpaceOptionsPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            OptionDialogPage page = (OptionDialogPage)GetDialogPage(typeof(OptionDialogPage));
            if (page != null)
            {
                page.RegisterPackage(this);
                OptionPage = page;
            }
        }
        #endregion
    }
}
