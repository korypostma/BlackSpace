//------------------------------------------------------------------------------
// <copyright file="TextAdornment1TextViewCreationListener.cs" company="Kory Postma">
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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace BlackSpace
{
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class BlackSpaceAdornmentTextViewCreationListener : IWpfTextViewCreationListener
    {
        // Disable "Field is never assigned to..." and "Field is never used" compiler's warnings. Justification: the field is used by MEF.
#pragma warning disable 649, 169

        /// <summary>
        /// Defines the adornment layer for the adornment. This layer is ordered
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("BlackSpaceTextAdornment")]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
        private AdornmentLayerDefinition editorAdornmentLayer;
        private BlackSpaceAdornment Adornment;

#pragma warning restore 649, 169

        #region IWpfTextViewCreationListener

        /// <summary>
        /// Called when a text view having matching roles is created over a text data model having a matching content type.
        /// Instantiates a TextAdornment1 manager when the textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            // The adornment will listen to any event that changes the layout (text changes, scrolling, etc)
            Adornment = new BlackSpaceAdornment(textView);
        }

        #endregion
    }
}
