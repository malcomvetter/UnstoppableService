using svchost.Resilience.DllImports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace svchost.Resilience
{
    public class RestrictControl
    {
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        public static SecurityIdentifier GetCurrentAccountSid()
        {
            return WindowsIdentity.GetCurrent().User;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/Services/modifying-the-dacl-for-a-service
        /// </summary>
        /// <param name="service"></param>
        /// <param name="sid"></param>
        /// <param name="accessControl"></param>
        /// <param name="access"></param>
        public static void ServiceControlPermissions(string service, SecurityIdentifier sid, AccessControlType accessControl, AdvApi32.SERVICE_ACCESS access)
        {
            IntPtr scHandle = IntPtr.Zero;
            IntPtr serviceHandle = IntPtr.Zero;
            try
            {
                scHandle = AdvApi32.OpenSCManager(null, null, (uint)AdvApi32.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);

                if (scHandle == IntPtr.Zero)
                    throw new Exception("Unable to open service control");

                serviceHandle = AdvApi32.OpenService(scHandle, service, (uint)((uint)AdvApi32.ACCESS_MASK.STANDARD_RIGHTS_REQUIRED | (uint)AdvApi32.SERVICE_ACCESS.SERVICE_QUERY_CONFIG | (uint)AdvApi32.SERVICE_ACCESS.SERVICE_QUERY_STATUS));

                if (serviceHandle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error()); // throw new Exception("Unable to open service");

                ServiceControlPermissions(serviceHandle, sid, accessControl, access);
            }
            finally
            {
                if (serviceHandle != IntPtr.Zero)
                    AdvApi32.CloseServiceHandle(serviceHandle);

                if (scHandle != IntPtr.Zero)
                    AdvApi32.CloseServiceHandle(scHandle);
            }
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/Services/modifying-the-dacl-for-a-service
        /// </summary>
        /// <param name="serviceHandle"></param>
        /// <param name="sid"></param>
        public static void ServiceControlPermissions(IntPtr serviceHandle, SecurityIdentifier sid, AccessControlType accessControl, AdvApi32.SERVICE_ACCESS access)
        {
            byte[] buffer = new byte[0];
            uint size = 0;

            //what size buffer do we need?
            bool success = AdvApi32.QueryServiceObjectSecurity(serviceHandle, SecurityInfos.DiscretionaryAcl, buffer, 0, out size);
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();

                if (size > 0)
                {
                    if (error == ERROR_INSUFFICIENT_BUFFER) //need more buffer!
                    {
                        buffer = new byte[size];

                        if (!AdvApi32.QueryServiceObjectSecurity(serviceHandle, SecurityInfos.DiscretionaryAcl, buffer, size, out size))
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    else
                        throw new Win32Exception(error);
                }
                else
                    throw new Win32Exception(error);
            }

            RawSecurityDescriptor descriptor = new RawSecurityDescriptor(buffer, 0);
            RawAcl acl = descriptor.DiscretionaryAcl;
            DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, acl);
            dacl.AddAccess(accessControl, sid, (int)access, InheritanceFlags.None, PropagationFlags.None);

            byte[] rawdacl = new byte[dacl.BinaryLength];
            dacl.GetBinaryForm(rawdacl, 0);
            descriptor.DiscretionaryAcl = new RawAcl(rawdacl, 0);

            byte[] rawsd = new byte[descriptor.BinaryLength];
            descriptor.GetBinaryForm(rawsd, 0);

            if (!AdvApi32.SetServiceObjectSecurity(serviceHandle, SecurityInfos.DiscretionaryAcl, rawsd))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
