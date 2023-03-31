//------------------------------------------------------------------------------
// <copyright file="BlackSpaceAdornment.cs" company="Kory Postma">
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
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;

namespace BlackSpace
{
    /// <summary>
    /// TextAdornment1 places boxes around all end-of-line whitespace in the editor window
    /// </summary>
    internal sealed class BlackSpaceAdornment
    {
        #region Static Functions
        public static Color ToMediaColor(System.Drawing.Color inColor)
        {
            return Color.FromArgb(inColor.A, inColor.R, inColor.G, inColor.B);
        }
        #endregion

        #region Member Variables
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView view;

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService
        {
            get;
            private set;
        }

        /// <summary>
        /// Adornment brushs.
        /// </summary>
        private Brush spacesBrush;
        private Pen spacesPen;
        private Brush tabsBrush;
        private Pen tabsPen;

        public System.Windows.Media.Brush SpacesBrush
        {
            get
            {
                if (spacesBrush == null)
                {
                    spacesBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xa0, 0x2b, 0x00, 0x95));
                    spacesBrush.Freeze();
                }

                return spacesBrush;
            }
        }

        public System.Windows.Media.Pen SpacesPen
        {
            get
            {
                if (spacesPen == null)
                {
                    var spacesPenBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x2b, 0x00, 0xb5));
                    spacesPenBrush.Freeze();
                    spacesPen = new System.Windows.Media.Pen(spacesPenBrush, 1.0);
                    spacesPen.Freeze();
                }

                return spacesPen;
            }
        }

        public System.Windows.Media.Brush TabsBrush
        {
            get
            {
                if (tabsBrush == null)
                {
                    tabsBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xa0, 0x2b, 0x00, 0x65));
                    tabsBrush.Freeze();
                }

                return tabsBrush;
            }
        }

        public System.Windows.Media.Pen TabsPen
        {
            get
            {
                if (tabsPen == null)
                {
                    var tabsPenBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x3b, 0x00, 0x85));
                    tabsPenBrush.Freeze();
                    tabsPen = new System.Windows.Media.Pen(tabsPenBrush, 1.0);
                    tabsPen.Freeze();
                }

                return tabsPen;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackSpaceAdornment"/> class.
        /// </summary>
        /// <param name="view">Text view to create the adornment for</param>
        public BlackSpaceAdornment(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            layer = view.GetAdornmentLayer("BlackSpaceTextAdornment");

            this.view = view;
            this.view.LayoutChanged += OnLayoutChanged;

            //Register this adornment, will cause it to load user settings and LoadSettings will update brushes
            BlackSpaceSettings.Instance.RegisterAdornment(this);
        }

        /// <summary>
        /// Handles whenever the text displayed in the view changes by adding the adornment to any reformatted lines
        /// </summary>
        /// <remarks><para>This event is raised whenever the rendered text displayed in the <see cref="ITextView"/> changes.</para>
        /// <para>It is raised whenever the view does a layout (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification changes).</para>
        /// <para>It is also raised whenever the view scrolls horizontally or when its size changes.</para>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                CreateVisuals(line);
            }
        }

        /// <summary>
        /// Adds a box to the end-of-line whitespace on the given line
        /// </summary>
        /// <param name="line">Line to add the adornments</param>
        private void CreateVisuals(ITextViewLine line)
        {
            IWpfTextViewLineCollection textViewLines = view.TextViewLines;

            //Ignore empty lines
            if (line.Length == 0) { return; }

            // Loop through each character from end to beginning, and place a box around spaces and tabs at the end of lines
            for (int charIndex = line.End - 1; charIndex >= line.Start; --charIndex)
            //for (int charIndex = line.Start; charIndex < line.End; charIndex++)
            {
                bool bIsSpace = (view.TextSnapshot[charIndex] == ' ');
                bool bIsTab = (view.TextSnapshot[charIndex] == '\t');
                if (bIsSpace || bIsTab)
                {
                    SnapshotSpan span = new SnapshotSpan(view.TextSnapshot, Span.FromBounds(charIndex, charIndex + 1));
                    Geometry geometry = textViewLines.GetMarkerGeometry(span);
                    if (geometry != null)
                    {
                        var drawing = new GeometryDrawing(bIsSpace ? spacesBrush : tabsBrush, bIsSpace ? spacesPen : tabsPen, geometry);
                        drawing.Freeze();

                        var drawingImage = new DrawingImage(drawing);
                        drawingImage.Freeze();

                        var image = new Image
                        {
                            Source = drawingImage,
                        };

                        // Align the image with the top of the bounds of the text geometry
                        Canvas.SetLeft(image, geometry.Bounds.Left);
                        Canvas.SetTop(image, geometry.Bounds.Top);

                        layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
                    }
                }
                //Unable to find spaces or tabs at the end of the line, ignore the rest
                else
                {
                    return;
                }
            }
        }

        public void UpdateBrushesAndPens(Settings settings)
        {
            //Create the brushes and pens to color the end-of-line white-spaces
            var spacesBrush = new SolidColorBrush(ToMediaColor(settings.Spaces.BackgroundColor));
            spacesBrush.Freeze();
            var spacesPenBrush = new SolidColorBrush(ToMediaColor(settings.Spaces.BorderColor));
            spacesPenBrush.Freeze();
            var spacesPen = new Pen(spacesPenBrush, settings.Spaces.BorderThickness);
            spacesPen.Freeze();

            var tabsBrush = new SolidColorBrush(ToMediaColor(settings.Tabs.BackgroundColor));
            tabsBrush.Freeze();
            var tabsPenBrush = new SolidColorBrush(ToMediaColor(settings.Tabs.BorderColor));
            tabsPenBrush.Freeze();
            var tabsPen = new Pen(tabsPenBrush, settings.Tabs.BorderThickness);
            tabsPen.Freeze();

            //Generate the brushes and pens
            UpdateBrushesAndPens(spacesBrush, spacesPen, tabsBrush, tabsPen);
        }

        public void UpdateBrushesAndPens(Brush inSpacesBrush, Pen inSpacesPen, Brush inTabsBrush, Pen inTabsPen)
        {
            spacesBrush = inSpacesBrush;
            spacesPen = inSpacesPen;
            tabsBrush = inTabsBrush;
            tabsPen = inTabsPen;

            RedrawAllAdornments();
        }

        public void RedrawAllAdornments()
        {
            if (layer == null || view == null || view.TextViewLines == null) { return; }

            //Redraw all adornments
            layer.RemoveAllAdornments();
            foreach (ITextViewLine line in view.TextViewLines)
            {
                CreateVisuals(line);
            }
        }
    }
}
