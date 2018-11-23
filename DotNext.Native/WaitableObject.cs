using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNext.Native {
	public class WaitableObject : WaitHandle {
		public WaitableObject(IntPtr handle, bool ownsHandle = true) {
			SafeWaitHandle = new SafeWaitHandle(handle, ownsHandle);
		}
	}
}
