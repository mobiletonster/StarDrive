﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarDrive;

public static class MessageService
{
    [DllImport("wtsapi32.dll", SetLastError = true)]
    static extern bool WTSSendMessage(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.I4)] int SessionId,
            String pTitle,
            [MarshalAs(UnmanagedType.U4)] int TitleLength,
            String pMessage,
            [MarshalAs(UnmanagedType.U4)] int MessageLength,
            [MarshalAs(UnmanagedType.U4)] int Style,
            [MarshalAs(UnmanagedType.U4)] int Timeout,
            [MarshalAs(UnmanagedType.U4)] out int pResponse,
            bool bWait);
    public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
    public static int WTS_CURRENT_SESSION = 1;
    public static void SendAlert(string title, string message)
    {
        for (int user_session = 10; user_session > 0; user_session--)
        {
                try
                {
                    bool result = false;
                    int tlen = title.Length;
                    int mlen = message.Length;
                    int resp = 7;
                    result = WTSSendMessage(WTS_CURRENT_SERVER_HANDLE, user_session, title, tlen, message, mlen, 4, 0, out resp, true);
                    int err = Marshal.GetLastWin32Error();
                    if (err == 0)
                    {
                        if (result) //user responded to box
                        {
                            if (resp == 7) //user clicked no
                            {

                            }
                            else if (resp == 6) //user clicked yes
                            {

                            }
                            Debug.WriteLine("user_session:" + user_session + " err:" + err + " resp:" + resp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("no such thread exists", ex);
                }
        }

    }
}
