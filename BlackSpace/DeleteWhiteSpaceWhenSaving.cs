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
