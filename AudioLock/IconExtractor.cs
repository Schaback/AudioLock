using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AudioUnfuck
{

    public class IconExtractor
    {
        [DllImport(
            "Shell32.dll",
            EntryPoint = "ExtractIconExW",
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall,
            SetLastError = true
        )]

        public static extern int ExtractIconEx(
            string lpszFile, // Name of the .exe or .dll that contains the icon
            int iconIndex, // zero based index of first icon to extract. If iconIndex == 0 and and phiconSmall == null and phiconSmall = null, the number of icons is returnd
            out IntPtr phiconLarge,
            out IntPtr phiconSmall,
            int nIcons
        );
        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        public static Icon ExtractIcon(String path, int index, bool large)
        {
            int readIconCount = 0;
            IntPtr hIconSmall = IntPtr.Zero;
            IntPtr hIconLarge = IntPtr.Zero;

            try
            {
                readIconCount = ExtractIconEx(path, index, out hIconLarge, out hIconSmall, 1);
                //Console.WriteLine(Marshal.GetLastWin32Error());
                if (readIconCount > 0)
                {
                    // GET FIRST EXTRACTED ICON
                    Icon smallIcon = (Icon)Icon.FromHandle(hIconSmall).Clone();
                    Icon largeIcon = (Icon)Icon.FromHandle(hIconLarge).Clone();

                    return large ? largeIcon : smallIcon;
                }
                else // NO ICONS READ
                    return null;
            }
            catch (Exception ex)
            {
                /* EXTRACT ICON ERROR */

                // BUBBLE UP
                throw new ApplicationException("Could not extract icon", ex);
            }
            finally
            {

                if (hIconSmall != IntPtr.Zero)
                    DestroyIcon(hIconSmall);


                if (hIconLarge != IntPtr.Zero)
                    DestroyIcon(hIconLarge);
            }
        }
    }
}