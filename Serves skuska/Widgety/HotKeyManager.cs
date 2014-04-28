using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using System.Runtime.InteropServices;

public class InterceptKeys
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;

    public struct keyboardHookStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    Delegate down_hook;
    Delegate up_hook;

    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public InterceptKeys()
    {
        _hookID = SetHook(_proc);

    }

    ~InterceptKeys()
    {
        UnhookWindowsHookEx(_hookID);

    }

    public void set_down_hook(Delegate callback)
    {
        this.down_hook = callback;
    }

    public void set_up_hook(Delegate callback)
    {
        this.up_hook = callback;
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    public static event KeyEventHandler KeyDown;
    public static event KeyEventHandler KeyUp;


    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, keyboardHookStruct lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, keyboardHookStruct lParam)
    {
        Keys key = (Keys)lParam.vkCode;
        KeyEventArgs kea = new KeyEventArgs(key);

        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            KeyDown(null, kea);

            //int vkCode = Marshal.ReadInt32(lParam);
            //Console.WriteLine((Keys)vkCode);
        }
        else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
        {
            KeyUp(null, kea);


            //int vkCode = Marshal.ReadInt32(lParam);
            //Console.WriteLine((Keys)vkCode);
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    //public int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
    //{
    //    if (code >= 0)
    //    {
    //        Keys key = (Keys)lParam.vkCode;
    //        if (HookedKeys.Contains(key))
    //        {
    //            KeyEventArgs kea = new KeyEventArgs(key);
    //            if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
    //            {
    //                KeyDown(this, kea);
    //            }
    //            else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
    //            {
    //                KeyUp(this, kea);
    //            }
    //            //if (kea.Handled)
    //            //  return 1;
    //        }
    //    }
    //    return CallNextHookEx(hhook, code, wParam, ref lParam);
    //}


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, keyboardHookStruct lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

}
