using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LechYTDLP.Util
{
    // Source - https://stackoverflow.com/a
    // Posted by Ray, modified by community. See post 'Timeline' for change history
    // Retrieved 2025-12-27, License - CC BY-SA 4.0

    public enum LechKnownFolder
    {
        Contacts,
        Downloads,
        Favorites,
        Links,
        SavedGames,
        SavedSearches
    }

    internal class LechKnownFolders
    {
        private static readonly Dictionary<LechKnownFolder, Guid> _guids = new()
        {
            [LechKnownFolder.Contacts] = new("56784854-C6CB-462B-8169-88E350ACB882"),
            [LechKnownFolder.Downloads] = new("374DE290-123F-4565-9164-39C4925E467B"),
            [LechKnownFolder.Favorites] = new("1777F761-68AD-4D8A-87BD-30B759FA33DD"),
            [LechKnownFolder.Links] = new("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968"),
            [LechKnownFolder.SavedGames] = new("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
            [LechKnownFolder.SavedSearches] = new("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA")
        };

        public static string GetPath(LechKnownFolder knownFolder)
        {
            return SHGetKnownFolderPath(_guids[knownFolder], 0);
        }

        [DllImport("shell32",
            CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern string SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
            nint hToken = 0);
    }

}
