﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Win32 {

   static class FolderBrowserDialog {

      // Constants for sending and receiving messages in BrowseCallBackProc
      public const int WM_USER = 0x400;
      public const int BFFM_INITIALIZED = 1;
      public const int BFFM_SELCHANGED = 2;
      public const int BFFM_VALIDATEFAILEDA = 3;
      public const int BFFM_VALIDATEFAILEDW = 4;
      public const int BFFM_IUNKNOWN = 5; // provides IUnknown to client. lParam: IUnknown*
      public const int BFFM_SETSTATUSTEXTA = WM_USER + 100;
      public const int BFFM_ENABLEOK = WM_USER + 101;
      public const int BFFM_SETSELECTIONA = WM_USER + 102;
      public const int BFFM_SETSELECTIONW = WM_USER + 103;
      public const int BFFM_SETSTATUSTEXTW = WM_USER + 104;
      public const int BFFM_SETOKTEXT = WM_USER + 105; // Unicode only
      public const int BFFM_SETEXPANDED = WM_USER + 106; // Unicode only

      public const int MAX_PATH = 260;

      // Browsing for directory.
      private const uint BIF_RETURNONLYFSDIRS = 0x0001;  // For finding a folder to start document searching
      private const uint BIF_DONTGOBELOWDOMAIN = 0x0002;  // For starting the Find Computer
      private const uint BIF_STATUSTEXT = 0x0004;  // Top of the dialog has 2 lines of text for BROWSEINFO.lpszTitle and one line if
      // this flag is set.  Passing the message BFFM_SETSTATUSTEXTA to the hwnd can set the
      // rest of the text.  This is not used with BIF_USENEWUI and BROWSEINFO.lpszTitle gets
      // all three lines of text.
      private const uint BIF_RETURNFSANCESTORS = 0x0008;
      private const uint BIF_EDITBOX = 0x0010;   // Add an editbox to the dialog
      private const uint BIF_VALIDATE = 0x0020;   // insist on valid result (or CANCEL)

      private const uint BIF_NEWDIALOGSTYLE = 0x0040;   // Use the new dialog layout with the ability to resize
      // Caller needs to call OleInitialize() before using this API
      private const uint BIF_USENEWUI = 0x0040 + 0x0010; //(BIF_NEWDIALOGSTYLE | BIF_EDITBOX);

      private const uint BIF_BROWSEINCLUDEURLS = 0x0080;   // Allow URLs to be displayed or entered. (Requires BIF_USENEWUI)
      private const uint BIF_UAHINT = 0x0100;   // Add a UA hint to the dialog, in place of the edit box. May not be combined with BIF_EDITBOX
      private const uint BIF_NONEWFOLDERBUTTON = 0x0200;   // Do not add the "New Folder" button to the dialog.  Only applicable with BIF_NEWDIALOGSTYLE.
      private const uint BIF_NOTRANSLATETARGETS = 0x0400;  // don't traverse target as shortcut

      private const uint BIF_BROWSEFORCOMPUTER = 0x1000;  // Browsing for Computers.
      private const uint BIF_BROWSEFORPRINTER = 0x2000;// Browsing for Printers
      private const uint BIF_BROWSEINCLUDEFILES = 0x4000; // Browsing for Everything
      private const uint BIF_SHAREABLE = 0x8000;  // sharable resources displayed (remote shares, requires BIF_USENEWUI)

      [DllImport("shell32.dll")]
      static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

      // Note that the BROWSEINFO object's pszDisplayName only gives you the name of the folder.
      // To get the actual path, you need to parse the returned PIDL
      [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
      static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

      [DllImport("user32.dll", PreserveSig = true)]
      public static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, long wParam, IntPtr lParam);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

      private static string m_initialPath;

      public delegate int BrowseCallBackProc(IntPtr hwnd, int msg, IntPtr lp, IntPtr wp);
      [StructLayout(LayoutKind.Sequential)]
      struct BROWSEINFO {
         public IntPtr hwndOwner;
         public IntPtr pidlRoot;
         public string pszDisplayName;
         public string lpszTitle;
         public uint ulFlags;
         public BrowseCallBackProc lpfn;
         public IntPtr lParam;
         public int iImage;
      }

      private static int OnBrowseEvent(IntPtr hWnd, int msg, IntPtr lp, IntPtr lpData) {
         IntPtr pathPtr;

         switch (msg) {
            case BFFM_INITIALIZED: // Required to set initialPath
               // Use BFFM_SETSELECTIONW if passing a Unicode string, i.e. native CLR Strings.
               SendMessage(new HandleRef(null, hWnd), BFFM_SETSELECTIONW, 1, m_initialPath);
               break;

            case BFFM_SELCHANGED:
               pathPtr = Marshal.AllocHGlobal((int) (1024 * Marshal.SystemDefaultCharSize));
               if (SHGetPathFromIDList(lp, pathPtr)) {
                  SendMessage(new HandleRef(null, hWnd), BFFM_SETSTATUSTEXTW, 0, pathPtr);
               }
               Marshal.FreeHGlobal(pathPtr);
               break;
         }

         return 0;
      }

      public static string SelectFolder(string caption, string initialPath, IntPtr parentHandle) {
         m_initialPath = initialPath;
         string strDispName = new string('*', MAX_PATH);
         StringBuilder sb = new StringBuilder(2048);
         IntPtr bufferAddress = Marshal.AllocHGlobal(2048); ;
         IntPtr pidl = IntPtr.Zero;

         var bi = new BROWSEINFO {
            hwndOwner = parentHandle,
            pidlRoot = IntPtr.Zero,
            pszDisplayName = strDispName,
            lpszTitle = caption,
            ulFlags = BIF_NEWDIALOGSTYLE | BIF_SHAREABLE,
            lpfn = new BrowseCallBackProc(OnBrowseEvent),
            lParam = IntPtr.Zero,
            iImage = 0
         };

         try {
            pidl = SHBrowseForFolder(ref bi);
            if (!SHGetPathFromIDList(pidl, bufferAddress)) return null;
            sb.Append(Marshal.PtrToStringAuto(bufferAddress));
         }
         finally {
            Marshal.FreeCoTaskMem(pidl);  // Caller is responsible for freeing this memory.
            Marshal.FreeHGlobal(bufferAddress);
         }

         return sb.ToString();
      }
   }
}