using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EvernoteToNotionChrome.Utils
{
    public class Win32Drap
    {
        public static bool SendFileDrop(IntPtr hwnd,string path, int x, int y)
        {
            //var point = Window.BoundingRectangle.Center();
            //Mouse.Position = point;

            var dropfiles = new Win32Api.DROPFILES();
            var sizeOfDropfiles = Marshal.SizeOf<Win32Api.DROPFILES>();
            dropfiles.pFiles = sizeOfDropfiles;
            dropfiles.pt.x = x;//(int)point.X;
            dropfiles.pt.y = y;// (int)point.Y;
            dropfiles.fNC = true;
            dropfiles.fWide = false;

            var hmem = Marshal.AllocHGlobal(sizeOfDropfiles + path.Length + 2);
            var hmemPtr = Win32Api.GlobalLock(hmem);
            var ptr = hmemPtr;

            Marshal.StructureToPtr(dropfiles, ptr, false);
            ptr += sizeOfDropfiles;
            var pathBytes = path.ToUtf8Bytes();
            Marshal.Copy(pathBytes, 0, ptr, pathBytes.Length);
            ptr += path.Length;
            Marshal.WriteInt16(ptr, 0); // double 0 to terminate file list
            Win32Api.GlobalUnlock(hmemPtr);

            var success = Win32Api.PostMessage(hwnd, Win32Api.WM_DROPFILES, hmem, IntPtr.Zero);
            if (!success)
                Marshal.FreeHGlobal(hmem);
            return success;
        }
    }
}
