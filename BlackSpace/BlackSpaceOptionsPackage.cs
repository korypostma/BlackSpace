//------------------------------------------------------------------------------
// <copyright file="BlackSpaceOptionsPackage.cs" company="Kory Postma">
//
//   Copyright 2016-2017 Kory Postma
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE; //DocumentEvents
using Task = System.Threading.Tasks.Task;

namespace BlackSpace
{
    public class BlackSpaceColorConverter : System.Drawing.ColorConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string sValue = ((string)value);
                return base.ConvertFrom(context, culture, sValue.TrimPrefix("1\"").TrimSuffix("\""));
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class OptionDialogPage : DialogPage
    {
        BlackSpaceOptionsPackage package = null;

        //Colors and thickness to use in creating the brushes and pens
        protected System.Drawing.Color spacesBackgroundColor = System.Drawing.Color.FromArgb(0xa0, 0x2b, 0x00, 0x95);
        protected System.Drawing.Color spacesBorderColor = System.Drawing.Color.FromArgb(0xff, 0x2b, 0x00, 0xb5);
        protected double spacesBorderThickness = 1.0;
        protected System.Drawing.Color tabsBackgroundColor = System.Drawing.Color.FromArgb(0xa0, 0x2b, 0x00, 0x65);
        protected System.Drawing.Color tabsBorderColor = System.Drawing.Color.FromArgb(0xff, 0x3b, 0x00, 0x85);
        protected double tabsBorderThickness = 1.0;

        [Category("General")]
        [DisplayName("Spaces: Background Color")]
        [Description("Color used for the background brush on end-of-line spaces.")]
        public System.Drawing.Color SpacesBackgroundColor
        {
            get { return spacesBackgroundColor; }
            set { spacesBackgroundColor = value; }
        }

        [Category("General")]
        [DisplayName("Spaces: Border Color")]
        [Description("Color used for the border/stroke pen on end-of-line spaces.")]
        public System.Drawing.Color SpacesBorderColor
        {
            get { return spacesBorderColor; }
            set { spacesBorderColor = value; }
        }

        [Category("General")]
        [DisplayName("Spaces: Border Thickness")]
        [Description("Thickness of the line/stroke used for the border/stroke pen on end-of-line spaces.")]
        public double SpacesBorderThickness
        {
            get { return spacesBorderThickness; }
            set { spacesBorderThickness = value; }
        }

        [Category("General")]
        [DisplayName("Tabs: Background Color")]
        [Description("Color used for the background brush on end-of-line tabs.")]
        public System.Drawing.Color TabsBackgroundColor
        {
            get { return tabsBackgroundColor; }
            set { tabsBackgroundColor = value; }
        }

        [Category("General")]
        [DisplayName("Tabs: Border Color")]
        [Description("Color used for the border/stroke pen on end-of-line tabs.")]
        public System.Drawing.Color TabsBorderColor
        {
            get { return tabsBorderColor; }
            set { tabsBorderColor = value; }
        }

        [Category("General")]
        [DisplayName("Tabs: Border Thickness")]
        [Description("Thickness of the line/stroke used for the border/stroke pen on end-of-line tabs.")]
        public double TabsBorderThickness
        {
            get { return tabsBorderThickness; }
            set { tabsBorderThickness = value; }
        }

        [Category("General")]
        [DisplayName("Delete EOL WhiteSpace when Saving")]
        [Description("Removes whitespace at the end-of-lines when saving files or solutions.")]
        public bool bDeleteWhiteSpaceWhenSaving
        {
            get; set;
        }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();

            //Only color settings have issues with auto-loading, so we need to manually load and convert these
            SpacesBackgroundColor = UpdateColorSetting(nameof(SpacesBackgroundColor), spacesBackgroundColor);
            SpacesBorderColor = UpdateColorSetting(nameof(SpacesBorderColor), spacesBorderColor);
            TabsBackgroundColor = UpdateColorSetting(nameof(TabsBackgroundColor), tabsBackgroundColor);
            TabsBorderColor = UpdateColorSetting(nameof(TabsBorderColor), tabsBorderColor);

            UpdateBrushesAndPens();
        }

        public void RegisterPackage(BlackSpaceOptionsPackage inPackage)
        {
            package = inPackage;
        }

        protected const string PROP_LOCATION = "ApplicationPrivateSettings\\BlackSpace\\OptionDialogPage";
        protected System.Drawing.Color UpdateColorSetting(string name, System.Drawing.Color defaultColor)
        {
            using (var regKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings))
            {
                if (regKey == null) { return defaultColor; }

                using (var colorKey = regKey.OpenSubKey(PROP_LOCATION))
                {
                    if (colorKey == null) { return defaultColor; }

                    var prop = colorKey.GetValue(name) as string;
                    if (prop == null) { return defaultColor; }

                    try
                    {
                        BlackSpaceColorConverter cc = new BlackSpaceColorConverter();
                        object Obj = cc.ConvertFromString(prop);
                        if (Obj != null && Obj is System.Drawing.Color)
                        {
                            System.Drawing.Color color = (System.Drawing.Color)Obj;
                            if (!color.IsEmpty)
                            {
                                return color;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.Message);
                        return defaultColor;
                    }
                    return defaultColor;
                }
            }
        }

        public static Color ToMediaColor(System.Drawing.Color inColor)
        {
            return Color.FromArgb(inColor.A, inColor.R, inColor.G, inColor.B);
        }

        protected virtual void UpdateBrushesAndPens()
        {
            //Create the brushes and pens to color the end-of-line white-spaces
            var spacesBrush = new SolidColorBrush(ToMediaColor(SpacesBackgroundColor));
            spacesBrush.Freeze();
            var spacesPenBrush = new SolidColorBrush(ToMediaColor(SpacesBorderColor));
            spacesPenBrush.Freeze();
            var spacesPen = new Pen(spacesPenBrush, SpacesBorderThickness);
            spacesPen.Freeze();

            var tabsBrush = new SolidColorBrush(ToMediaColor(TabsBackgroundColor));
            tabsBrush.Freeze();
            var tabsPenBrush = new SolidColorBrush(ToMediaColor(TabsBorderColor));
            tabsPenBrush.Freeze();
            var tabsPen = new Pen(tabsPenBrush, TabsBorderThickness);
            tabsPen.Freeze();

            //Generate the brushes and pens
            if (package != null)
            {
                package.UpdateBrushesAndPens(spacesBrush, spacesPen, tabsBrush, tabsPen);
            }
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
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(OptionDialogPage), "Black Space", "General", 0, 0, true)]
    public sealed class BlackSpaceOptionsPackage : AsyncPackage
    {
        /// <summary>
        /// BlackSpaceOptionsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "3f7dbe25-8df3-454a-a340-31c8e0ba610e";

        public static OptionDialogPage OptionPage = null;

        /// <summary>
        /// Visual Studio Events sub-objects.
        /// </summary>
        //private DTE dte;
        //private DocumentEvents VSDocumentEvents;
        //private BuildEvents VSBuildEvents;
        //private SolutionEvents VSSolutionEvents;

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

        public void UpdateBrushesAndPens(Brush inSpacesBrush, Pen inSpacesPen, Brush inTabsBrush, Pen inTabsPen)
        {
            spacesBrush = inSpacesBrush;
            spacesPen = inSpacesPen;
            tabsBrush = inTabsBrush;
            tabsPen = inTabsPen;

            if (adornment != null)
            {
                adornment.UpdateBrushesAndPens(spacesBrush, spacesPen, tabsBrush, tabsPen);
            }
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            //dte = (DTE)GetGlobalService(typeof(DTE));

            //VSDocumentEvents = dte.Events.DocumentEvents[null];
            //if (VSDocumentEvents != null)
            //{
            //    VSDocumentEvents.DocumentClosing += VSDocumentEvents_DocumentClosing;
            //    VSDocumentEvents.DocumentOpened += VSDocumentEvents_DocumentOpened;
            //    VSDocumentEvents.DocumentOpening += VSDocumentEvents_DocumentOpening;
            //    VSDocumentEvents.DocumentSaved += VSDocumentEvents_DocumentSaved;
            //}

            //VSBuildEvents = dte.Events.BuildEvents;
            //if (VSBuildEvents != null)
            //{
            //    VSBuildEvents.OnBuildBegin += VSBuildEvents_OnBuildBegin;
            //    VSBuildEvents.OnBuildDone += VSBuildEvents_OnBuildDone;
            //    VSBuildEvents.OnBuildProjConfigBegin += VSBuildEvents_OnBuildProjConfigBegin;
            //    VSBuildEvents.OnBuildProjConfigDone += VSBuildEvents_OnBuildProjConfigDone;
            //}

            //VSSolutionEvents = dte.Events.SolutionEvents;
            //if (VSSolutionEvents != null)
            //{
            //    VSSolutionEvents.AfterClosing += VSSolutionEvents_AfterClosing;
            //    VSSolutionEvents.BeforeClosing += VSSolutionEvents_BeforeClosing;
            //    VSSolutionEvents.Opened += VSSolutionEvents_Opened;
            //    VSSolutionEvents.ProjectAdded += VSSolutionEvents_ProjectAdded;
            //    VSSolutionEvents.ProjectRemoved += VSSolutionEvents_ProjectRemoved;
            //    VSSolutionEvents.ProjectRenamed += VSSolutionEvents_ProjectRenamed;
            //    VSSolutionEvents.QueryCloseSolution += VSSolutionEvents_QueryCloseSolution;
            //    VSSolutionEvents.Renamed += VSSolutionEvents_Renamed;
            //}

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            OptionDialogPage page = (OptionDialogPage)GetDialogPage(typeof(OptionDialogPage));
            if (page != null)
            {
                page.RegisterPackage(this);
                page.LoadSettingsFromStorage();
                OptionPage = page;
            }

            BlackSpaceAdornment.Initialize(this);
        }

        private void WriteToOutputWindow(Guid paneGuid, string message)
        {
            var pane = GetOutputPane(paneGuid, "CustomBuilder Output", true, true, message);
            pane.OutputString(message + "\n");
            pane.Activate();        // Activates the new pane to show the output we just add.
        }

        private IVsOutputWindowPane GetOutputPane(Guid paneGuid, string title, bool visible, bool clearWithSolution, string message)
        {
            IVsOutputWindow output = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
            IVsOutputWindowPane pane;
            output.CreatePane(ref paneGuid, title, Convert.ToInt32(visible), Convert.ToInt32(clearWithSolution));
            output.GetPane(ref paneGuid, out pane);
            return pane;
        }

        //private void VSSolutionEvents_Renamed(string OldName)
        //{

        //}

        //private void VSSolutionEvents_QueryCloseSolution(ref bool fCancel)
        //{

        //}

        //private void VSSolutionEvents_ProjectRenamed(Project Project, string OldName)
        //{

        //}

        //private void VSSolutionEvents_ProjectRemoved(Project Project)
        //{

        //}

        //private void VSSolutionEvents_ProjectAdded(Project Project)
        //{

        //}

        //private void VSSolutionEvents_Opened()
        //{
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Opened");
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "Opened");
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "Opened");
        //}

        //private void VSSolutionEvents_BeforeClosing()
        //{
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "BeforeClosing");
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "BeforeClosing");
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "BeforeClosing");
        //}

        //private void VSSolutionEvents_AfterClosing()
        //{
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "AfterClosing");
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "AfterClosing");
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "AfterClosing");
        //}

        //private void VSBuildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        //{

        //}

        //private void VSBuildEvents_OnBuildProjConfigBegin(string Project, string ProjectConfig, string Platform, string SolutionConfig)
        //{

        //}

        //private void VSBuildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        //{

        //}

        //private void VSBuildEvents_OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
        //{

        //}

        //private void VSDocumentEvents_DocumentOpening(string DocumentPath, bool ReadOnly)
        //{
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "DocumentOpening: " + DocumentPath);
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "DocumentOpening: " + DocumentPath);
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "DocumentOpening: " + DocumentPath);
        //}

        //private void VSDocumentEvents_DocumentOpened(Document Document)
        //{
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Opened: ");
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "Opened: ");
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "Opened: ");
        //    return;
        //    //if (Document == null)
        //    //{
        //    //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Opened: null");
        //    //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "Opened: null");
        //    //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "Opened: null");
        //    //    return;
        //    //}
        //    //WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Opened: " + Document.FullName);
        //    //WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "Opened: " + Document.FullName);
        //    //WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "Opened: " + Document.FullName);
        //}

        //private void VSDocumentEvents_DocumentClosing(Document Document)
        //{
        //    if (Document == null)
        //    {
        //        WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Closing: null");
        //        WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "Closing: null");
        //        WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "Closing: null");
        //        return;
        //    }
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Closing: " + Document.FullName);
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "Closing: " + Document.FullName);
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "Closing: " + Document.FullName);
        //}

        //private void VSDocumentEvents_DocumentSaved(Document Document)
        //{
        //    if (Document == null)
        //    {
        //        WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Saved: null");
        //        WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "Saved: null");
        //        WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "Saved: null");
        //        return;
        //    }
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Saved: " + Document.FullName);
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, "Saved: " + Document.FullName);
        //    WriteToOutputWindow(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "Saved: " + Document.FullName);
        //}

        BlackSpaceAdornment adornment = null;

        Brush spacesBrush = null;
        Pen spacesPen = null;
        Brush tabsBrush = null;
        Pen tabsPen = null;

        public Brush SpacesBrush
        {
            get
            {
                if (spacesBrush == null)
                {
                    spacesBrush = new SolidColorBrush(Color.FromArgb(0xa0, 0x2b, 0x00, 0x95));
                    spacesBrush.Freeze();
                }
                return spacesBrush;
            }
        }

        public Pen SpacesPen
        {
            get
            {
                if (spacesPen == null)
                {
                    var spacesPenBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x2b, 0x00, 0xb5));
                    spacesPenBrush.Freeze();
                    spacesPen = new Pen(spacesPenBrush, 1.0);
                    spacesPen.Freeze();
                }
                return spacesPen;
            }
        }

        public Brush TabsBrush
        {
            get
            {
                if (tabsBrush == null)
                {
                    tabsBrush = new SolidColorBrush(Color.FromArgb(0xa0, 0x2b, 0x00, 0x65));
                    tabsBrush.Freeze();
                }
                return tabsBrush;
            }
        }

        public Pen TabsPen
        {
            get
            {
                if (tabsPen == null)
                {
                    var tabsPenBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x3b, 0x00, 0x85));
                    tabsPenBrush.Freeze();
                    tabsPen = new Pen(tabsPenBrush, 1.0);
                    tabsPen.Freeze();
                }
                return tabsPen;
            }
        }

        internal void RegisterAdornment(BlackSpaceAdornment inAdornment)
        {
            adornment = inAdornment;
            adornment.UpdateBrushesAndPens(spacesBrush, spacesPen, tabsBrush, tabsPen);
        }
        #endregion
    }
}
