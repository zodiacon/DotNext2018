using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNext.Native {
	enum JobInformationClass {
		CpuRateControlInformation = 15
	}

	[Flags]
	enum CpuRateControlFlags {
		None = 0,
		Enable = 1,
		HardCap = 4
	}

	[StructLayout(LayoutKind.Sequential)]
	struct JobCpuRateControlInformation {
		public CpuRateControlFlags Flags;
		public int CpuRate;
	}

	static class NativeMethods {
		const string KernelLib = "Kernel32";

		[DllImport(KernelLib)]
		public static extern bool CloseHandle(IntPtr handle);

		#region Jobs

		[DllImport(KernelLib, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr CreateJobObject(IntPtr sa, string name);

		[DllImport(KernelLib, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenJobObject(JobAccessMask accessMask, bool inheritHandle, string name);

		[DllImport(KernelLib, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool TerminateJobObject(IntPtr handle, int exitCode);

		[DllImport(KernelLib, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

		[DllImport(KernelLib, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetInformationJobObject(IntPtr hJob, JobInformationClass infoClass, in JobCpuRateControlInformation info, int size);

		#endregion

		#region CPU Sets

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool SetProcessDefaultCpuSets(IntPtr hProcess, in uint[] cpuSetIds, uint cpuSetIdCount);

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool SetThreadSelectedCpuSets(IntPtr hThread, in uint[] cpuSetIds, uint cpuSetIdCount);

		#endregion

	}
}
