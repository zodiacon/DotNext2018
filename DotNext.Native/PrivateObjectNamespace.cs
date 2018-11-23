using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Security.Principal;

namespace DotNext.Native {
	public sealed class PrivateObjectNamespace : SafeHandleZeroOrMinusOneIsInvalid {
		public PrivateObjectNamespace(string boundary, string prefix) : base(true) {
			var bd = NativeMethods.CreateBoundaryDescriptor(boundary);
			if (bd == IntPtr.Zero)
				throw new Win32Exception();

			try {
				var userSid = WindowsIdentity.GetCurrent().User;
				var sidBytes = new byte[userSid.BinaryLength];
				userSid.GetBinaryForm(sidBytes, 0);
				if (!NativeMethods.AddSIDToBoundaryDescriptor(ref bd, sidBytes))
					throw new Win32Exception();

				handle = NativeMethods.CreatePrivateNamespace(IntPtr.Zero, bd, prefix);
				if (handle == IntPtr.Zero)
					handle = NativeMethods.OpenPrivateNamespace(bd, prefix);
				if (handle == IntPtr.Zero) {
					throw new Win32Exception();
				}
			}
			finally {
				NativeMethods.DeleteBoundaryDescriptor(bd);
			}
		}

		public void AddSid(SecurityIdentifier sid) {
		}

		protected override bool ReleaseHandle() {
			return NativeMethods.ClosePrivateNamespace(handle);
		}
	}
}
