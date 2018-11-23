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
	public enum MemoryResourceNotificationType {
		Low,
		High
	}

	public sealed class MemoryResourceNotification : WaitHandle {
		public MemoryResourceNotification(MemoryResourceNotificationType type) {
			var handle = NativeMethods.CreateMemoryResourceNotification(type);
			if (handle == IntPtr.Zero)
				throw new Win32Exception();

			SafeWaitHandle = new SafeWaitHandle(handle, true);
		}
	}
}
