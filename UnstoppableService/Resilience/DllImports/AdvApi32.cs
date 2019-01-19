using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;

namespace svchost.Resilience.DllImports
{
    public static class AdvApi32
    {
        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/securitybaseapi/nf-securitybaseapi-adjusttokenprivileges
        /// </summary>
        public const uint SE_PRIVILEGE_DISABLED = 0x00000000;
        public const uint SE_PRIVILEGE_ENABLED = 0x00000002;

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/SecAuthZ/privilege-constants
        /// </summary>
        public const string SE_DEBUG_NAME = "SeDebugPrivilege";

        /// <summary>
        /// https://msdn.microsoft.com/en-us/windows/desktop/aa379180
        /// </summary>
        /// <param name="lpSystemName"></param>
        /// <param name="lpName"></param>
        /// <param name="lpLuid"></param>
        /// <returns></returns>
        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/securitybaseapi/nf-securitybaseapi-adjusttokenprivileges
        /// </summary>
        /// <param name="TokenHandle"></param>
        /// <param name="DisableAllPrivileges"></param>
        /// <param name="NewState"></param>
        /// <param name="BufferLength"></param>
        /// <param name="PreviousState"></param>
        /// <param name="ReturnLength"></param>
        /// <returns></returns>
        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGE NewState, uint BufferLength, ref TOKEN_PRIVILEGE PreviousState, ref uint ReturnLength);


        /// <summary>
        /// The OpenProcessToken function opens the access token associated with a process
        /// </summary>
        /// <param name="ProcessHandle"></param>
        /// <param name="DesiredAccess"></param>
        /// <param name="TokenHandle"></param>
        /// <returns></returns>
        [DllImport("advapi32", ExactSpelling = true, SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winsvc/nf-winsvc-openscmanagerw
        /// </summary>
        /// <param name="lpMachineName"></param>
        /// <param name="lpDatabaseName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <returns></returns>
        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, uint dwDesiredAccess);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winsvc/nf-winsvc-setserviceobjectsecurity
        /// </summary>
        /// <param name="hService"></param>
        /// <param name="dwSecurityInformation"></param>
        /// <param name="lpSecurityDescriptor"></param>
        /// <returns></returns>
        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetServiceObjectSecurity(IntPtr hService, SecurityInfos dwSecurityInformation, byte[] lpSecurityDescriptor);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winsvc/nf-winsvc-openservicew
        /// </summary>
        /// <param name="hSCManager"></param>
        /// <param name="lpServiceName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <returns></returns>
        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winsvc/nf-winsvc-queryserviceobjectsecurity
        /// </summary>
        /// <param name="hService"></param>
        /// <param name="dwSecurityInformation"></param>
        /// <param name="lpSecurityDescriptor"></param>
        /// <param name="cbBufSize"></param>
        /// <param name="pcbBytesNeeded"></param>
        /// <returns></returns>
        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool QueryServiceObjectSecurity(IntPtr hService, SecurityInfos dwSecurityInformation, byte[] lpSecDesrBuf, uint cbBufSize, out uint pcbBytesNeeded);


        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winsvc/nf-winsvc-closeservicehandle
        /// </summary>
        /// <param name="hSCObject"></param>
        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void CloseServiceHandle(IntPtr hSCObject);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/SecAuthZ/access-mask
        /// </summary>
        [Flags]
        public enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,
            STANDARD_RIGHTS_ALL = 0x001F0000,
            SPECIFIC_RIGHTS_ALL = 0x0000FFFF,
            ACCESS_SYSTEM_SECURITY = 0x01000000,
            MAXIMUM_ALLOWED = 0x02000000,
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,
            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,
            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037F
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/Services/service-security-and-access-rights
        /// </summary>
        [Flags]
        public enum SCM_ACCESS : uint
        {
            SC_MANAGER_CONNECT = 0x00001,
            SC_MANAGER_CREATE_SERVICE = 0x00002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,
            SC_MANAGER_LOCK = 0x00008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,
            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,
            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,
            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,
            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT | SC_MANAGER_LOCK,
            GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/Services/service-security-and-access-rights
        /// </summary>
        [Flags]
        public enum SERVICE_ACCESS : uint
        {
            SERVICE_ALL_ACCESS = (ACCESS_MASK.STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_INTERROGATE | SERVICE_USER_DEFINED_CONTROL), //Includes STANDARD_RIGHTS_REQUIRED in addition to all access rights in this table.
            SERVICE_CHANGE_CONFIG = 0x0002, //Required to call the ChangeServiceConfig or ChangeServiceConfig2 function to change the service configuration.Because this grants the caller the right to change the executable file that the system runs, it should be granted only to administrators.
            SERVICE_ENUMERATE_DEPENDENTS = 0x0008, //Required to call the EnumDependentServices function to enumerate all the services dependent on the service.
            SERVICE_INTERROGATE = 0x0080, //Required to call the ControlService function to ask the service to report its status immediately.
            SERVICE_PAUSE_CONTINUE = 0x0040, //Required to call the ControlService function to pause or continue the service.
            SERVICE_QUERY_CONFIG = 0x0001, //Required to call the QueryServiceConfig and QueryServiceConfig2 functions to query the service configuration.
            SERVICE_QUERY_STATUS = 0x0004, //Required to call the QueryServiceStatus or QueryServiceStatusEx function to ask the service control manager about the status of the service. Required to call the NotifyServiceStatusChange function to receive notification when a service changes status.
            SERVICE_START = 0x0010, //Required to call the StartService function to start the service.
            SERVICE_STOP = 0x0020, // Required to call the ControlService function to stop the service.
            SERVICE_USER_DEFINED_CONTROL = 0x0100, // Required to call the ControlService function to specify a user-defined control code.
        }

        /// https://docs.microsoft.com/en-us/dotnet/api/system.security.principal.tokenaccesslevels?view=netframework-4.7.2
        /// </summary>
        [Flags]
        public enum TokenAccessLevels : int
        {
            AssignPrimary = 1,
            Duplicate = 2,
            Impersonate = 4,
            Query = 8,
            QuerySource = 16,
            AdjustPrivileges = 32,
            AdjustGroups = 64,
            AdjustDefault = 128,
            AdjustSessionId = 256,
            Read = 131080,
            Write = 131296,
            AllAccess = 983551,
            MaximumAllow = 33554432
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winnt/ns-winnt-_luid
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LUID
        {
            public uint LowPart;
            public uint HighPart;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winnt/ns-winnt-token_privileges
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TOKEN_PRIVILEGE
        {
            public uint PrivilegeCount;
            public LUID Luid;
            public uint Attributes;
        }
    }
}
