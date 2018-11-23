using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;

namespace DotNext.Native {
	internal class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid {
		public SafeThreadHandle(int tid, ThreadAccessMask access, bool ownsHandle = true) : base(ownsHandle) {
			handle = NativeMethods.OpenThread(access, false, tid);
			if (handle == IntPtr.Zero)
				throw new Win32Exception();
		}

		protected override bool ReleaseHandle() {
			return NativeMethods.CloseHandle(handle);
		}
	}
}