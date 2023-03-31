//------------------------------------------------------------------------------
// <copyright file="DeleteWhiteSpaceWhenSaving.cs" company="Kory Postma">
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
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace BlackSpace
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("SaveCommandHandler")]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class DeleteWhiteSpaceWhenSaving : IVsTextViewCreationListener
    {
        [Import]
        private IVsEditorAdaptersFactoryService AdapterService = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null) { return; }

            Func<DeleteWhiteSpaceWhenSavingCommandHandler> createCommandHandler = delegate () { return new DeleteWhiteSpaceWhenSavingCommandHandler(textViewAdapter, textView); };
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }
}
