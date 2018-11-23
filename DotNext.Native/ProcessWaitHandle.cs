using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNext.Native {
	public class ProcessWaitHandle : WaitHandle {
		public ProcessWaitHandle(Process process) : this(process.Handle) {
		}

		public ProcessWaitHandle(IntPtr handle) {
			IntPtr hDuplicated;
			if (!NativeMethods.DuplicateHandle(NativeMethods.GetCurrentProcess(), handle, NativeMethods.GetCurrentProcess(), out hDuplicated, 0, false, DuplicateHandleOptions.SameAccess))
				throw new Win32Exception();

			SafeWaitHandle = new SafeWaitHandle(hDuplicated, true);
		}
	}
}
