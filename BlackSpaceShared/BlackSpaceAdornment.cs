//------------------------------------------------------------------------------
// <copyright file="BlackSpaceAdornment.cs" company="Kory Postma">
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
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView view;

        /// <summary>
        /// Adornment brushs.
        /// </summary>
        private Brush spacesBrush;
        private Pen spacesPen;
        private Brush tabsBrush;
        private Pen tabsPen;

        public static BlackSpaceOptionsPackage Package
        {
            get;
            private set;
        }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="inPackage">Owner package, not null.</param>
        public static void Initialize(BlackSpaceOptionsPackage inPackage)
        {
            Package = inPackage;
        }

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

            this.layer = view.GetAdornmentLayer("BlackSpaceTextAdornment");

            this.view = view;
            this.view.LayoutChanged += this.OnLayoutChanged;

            if (Package != null)
            {
                //This will also update all of the brushes
                Package.RegisterAdornment(this);
            }
            else
            {
                // Create the pen and brush to color the boxes around the end-of-line whitespace
                spacesBrush = new SolidColorBrush(Color.FromArgb(0xa0, 0x2b, 0x00, 0x95));
                spacesBrush.Freeze();
                var spacesPenBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x2b, 0x00, 0xb5));
                spacesPenBrush.Freeze();
                spacesPen = new Pen(spacesPenBrush, 1.0);
                spacesPen.Freeze();

                tabsBrush = new SolidColorBrush(Color.FromArgb(0xa0, 0x2b, 0x00, 0x65));
                tabsBrush.Freeze();
                var tabsPenBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x3b, 0x00, 0x85));
                tabsPenBrush.Freeze();
                tabsPen = new Pen(tabsPenBrush, 1.0);
                tabsPen.Freeze();
            }
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
