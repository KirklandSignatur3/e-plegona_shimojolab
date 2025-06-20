
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Threading;

public class AlwaysTopWindow : MonoBehaviour 
{
    public bool isAlwaysOnTop = true;
    
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    const int GWL_HWNDPARENT = (-8);
    const Int32 GWL_ID = (-12);
    const Int32 GWL_STYLE = (-16);
    const Int32 GWL_EXSTYLE = (-20);

    // Window Styles 
    const Int32 WS_OVERLAPPED = 0;
    const Int64 WS_POPUP = 0x80000000;
    const Int32 WS_CHILD = 0x40000000;
    const Int32 WS_MINIMIZE = 0x20000000;
    const Int32 WS_VISIBLE = 0x10000000;
    const Int32 WS_DISABLED = 0x8000000;
    const Int32 WS_CLIPSIBLINGS = 0x4000000;
    const Int32 WS_CLIPCHILDREN = 0x2000000;
    const Int32 WS_MAXIMIZE = 0x1000000;
    const Int32 WS_CAPTION = 0xC00000;      // WS_BORDER or WS_DLGFRAME  
    const Int32 WS_BORDER = 0x800000;
    const Int32 WS_DLGFRAME = 0x400000;
    const Int32 WS_VSCROLL = 0x200000;
    const Int32 WS_HSCROLL = 0x100000;
    const Int32 WS_SYSMENU = 0x80000;
    const Int32 WS_THICKFRAME = 0x40000;
    const Int32 WS_GROUP = 0x20000;
    const Int32 WS_TABSTOP = 0x10000;
    const Int32 WS_MINIMIZEBOX = 0x20000;
    const Int32 WS_MAXIMIZEBOX = 0x10000;
    const Int32 WS_TILED = WS_OVERLAPPED;
    const Int32 WS_ICONIC = WS_MINIMIZE;
    const Int32 WS_SIZEBOX = WS_THICKFRAME;

    // Extended Window Styles 
    const Int32 WS_EX_DLGMODALFRAME = 0x0001;
    const Int32 WS_EX_NOPARENTNOTIFY = 0x0004;
    const Int32 WS_EX_TOPMOST = 0x0008;
    const Int32 WS_EX_ACCEPTFILES = 0x0010;
    const Int32 WS_EX_TRANSPARENT = 0x0020;
    const Int32 WS_EX_MDICHILD = 0x0040;
    const Int32 WS_EX_TOOLWINDOW = 0x0080;
    const Int32 WS_EX_WINDOWEDGE = 0x0100;
    const Int32 WS_EX_CLIENTEDGE = 0x0200;
    const Int32 WS_EX_CONTEXTHELP = 0x0400;
    const Int32 WS_EX_RIGHT = 0x1000;
    const Int32 WS_EX_LEFT = 0x0000;
    const Int32 WS_EX_RTLREADING = 0x2000;
    const Int32 WS_EX_LTRREADING = 0x0000;
    const Int32 WS_EX_LEFTSCROLLBAR = 0x4000;
    const Int32 WS_EX_RIGHTSCROLLBAR = 0x0000;
    const Int32 WS_EX_CONTROLPARENT = 0x10000;
    const Int32 WS_EX_STATICEDGE = 0x20000;
    const Int32 WS_EX_APPWINDOW = 0x40000;
    const Int32 WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
    const Int32 WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
    const Int32 WS_EX_LAYERED = 0x00080000;
    const Int32 WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
    const Int32 WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
    const Int32 WS_EX_COMPOSITED = 0x02000000;
    const Int32 WS_EX_NOACTIVATE = 0x08000000;
    const UInt32 SWP_NOSIZE = 0x0001;
    const UInt32 SWP_NOMOVE = 0x0002;
    const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(System.String className, System.String windowName);

    private const int SW_RESTORE = 9;

    static Thread thread;
    static IntPtr windowPtr;

    public static void SetAlwaysOnTop(bool alwaysOnTop)
    {
        string title = Application.productName;
        Debug.Log("Application ProductName " + title);
        windowPtr = FindWindow(null, title);
        if (windowPtr == IntPtr.Zero)
        {
            Debug.LogError("Failed Get Window Handle. " + title);
            return;
        }
        int oldExStyle = GetWindowLong(windowPtr, GWL_EXSTYLE);
        if (alwaysOnTop)
        {
            SetWindowLong(windowPtr, GWL_EXSTYLE, oldExStyle | WS_EX_TOPMOST);
            SetWindowPos(windowPtr, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
            thread = new Thread(MaxmizeWork);
            thread.Start();

        }
        else
        {
            SetWindowLong(windowPtr, GWL_EXSTYLE, oldExStyle &= (~WS_EX_TOPMOST));
            SetWindowPos(windowPtr, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
        }
    }

    static void MaxmizeWork()
    {
        while (true)
        {
            Thread.Sleep(5000);
            ShowWindowAsync(windowPtr, SW_RESTORE);
        }
    }

#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	public static void setAlwaysOnTop( bool alwaysOnTop ){
		//not implemented
	}
#endif

// Use this for initialization
#if !UNITY_EDITOR
    void Start () {
        SetAlwaysOnTop(isAlwaysOnTop);
    }
#endif

#if !UNITY_EDITOR
    void OnApplicationQuit()
    {
        SetAlwaysOnTop(false);

    }
#endif
}
