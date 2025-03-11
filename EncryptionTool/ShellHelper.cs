#nullable enable

using System.Runtime.InteropServices;

namespace EncryptionTool
{
    // ShellHelper is a static class that provides functionality to refresh Windows Explorer,
    // ensuring that changes to files and folders are reflected immediately
    public static class ShellHelper
    {
        private const uint SHCNE_UPDATEDIR = 0x00001000; // Used to notify updates for a folder.
        private const uint SHCNE_UPDATEITEM = 0x00002000; // Used to notify updates for a specific file.
        private const uint SHCNF_PATHW = 0x0005;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        // Notifies Windows Explorer about changes in a specific file or folder, 
        // ensuring that modifications are displayed correctly.
        public static void RefreshExplorer(string? path)
        {
            if (path == null) return; // If path is null, do nothing

            if (File.Exists(path))
            {
                // Converts the file path to a memory pointer (IntPtr) using Marshal.StringToHGlobalUni(path),
                // so that it can be used with the Windows API.
                IntPtr pPathFile = Marshal.StringToHGlobalUni(path);
                try
                {
                    // Notifies Windows Explorer that the specific file has changed.
                    SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATHW, pPathFile, IntPtr.Zero);
                }
                finally
                {
                    // Releases the allocated memory.
                    Marshal.FreeHGlobal(pPathFile);
                }

                path = Path.GetDirectoryName(path);
            }

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                IntPtr pPathDir = Marshal.StringToHGlobalUni(path);
                try
                {
                    SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATHW, pPathDir, IntPtr.Zero);
                }
                finally
                {
                    Marshal.FreeHGlobal(pPathDir);
                }
            }
        }
    }
}
