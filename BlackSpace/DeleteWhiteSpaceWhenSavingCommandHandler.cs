using System;
using System.Linq;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio;

namespace BlackSpace
{
    internal class DeleteWhiteSpaceWhenSavingCommandHandler : IOleCommandTarget
    {
        private IWpfTextView View;
        private IOleCommandTarget NextCmdTarg;
        private static Guid CmdGroup = typeof(VSConstants.VSStd97CmdID).GUID;
        private static uint[] CmdID = new[] { (uint)VSConstants.VSStd97CmdID.SaveProjectItem, (uint)VSConstants.VSStd97CmdID.SaveSolution };

        public DeleteWhiteSpaceWhenSavingCommandHandler(IVsTextView textViewAdapter, IWpfTextView view)
        {
            textViewAdapter.AddCommandFilter(this, out NextCmdTarg);
            View = view;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == CmdGroup && CmdID.Contains(nCmdID))
            {
                if (View != null && View.TextBuffer != null && BlackSpaceOptionsPackage.OptionPage != null && BlackSpaceOptionsPackage.OptionPage.bDeleteWhiteSpaceWhenSaving)
                {
                    DeleteWhiteSpace(View.TextBuffer);
                }
            }
            return NextCmdTarg.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void DeleteWhiteSpace(ITextBuffer textBuffer)
        {
            ITextEdit EditBuffer = textBuffer.CreateEdit();
            foreach (ITextSnapshotLine Line in EditBuffer.Snapshot.Lines)
            {
                string sLine = Line.GetText();
                int i = Line.Length;
                //If this line is empty then move on
                if (i == 0) { continue; }
                //Start at the end of the line and find the starting index of the whitespace
                while (--i >= 0 && Char.IsWhiteSpace(sLine[i])) { };
                ++i;
                //If we found whitespace then remove it, this if check is unnecessary, but avoids us having to call Delete below unnecessarily
                if (i != Line.Length)
                {
                    EditBuffer.Delete(Line.Start.Position + i, Line.Length - i);
                }
            }
            EditBuffer.Apply();
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == CmdGroup)
            {
                for (uint i = 0; i < cCmds; ++i)
                {
                    if (CmdID.Contains(prgCmds[i].cmdID))
                    {
                        //One of the commands that we handle is contained here, so tell them we can handle it
                        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                        return VSConstants.S_OK;
                    }
                }
            }
            return NextCmdTarg.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
