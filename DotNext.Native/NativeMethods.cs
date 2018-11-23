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

	[StructLayout(LayoutKind.Sequential)]
	struct SYSTEM_CPU_SET_INFORMATION {
		public uint Size;
		uint Type;

		public uint Id;
		public ushort Group;
		public byte LogicalProcessorIndex;
		public byte CoreIndex;
		public byte LastLevelCacheIndex;
		public byte NumaNodeIndex;
		public byte EfficiencyClass;
		public byte Flags;

		ulong AllocationTag;
	}

	enum ThreadAccessMask {
		SetInformation = 0x20,
		SetLimitedInformation = 0x400,
	}

	enum PrivateNamespaceFlags {
		None = 0,
		DestroyNamespace = 1
	}

	[Flags]
	enum DuplicateHandleOptions {
		CloseSource = 1,
		SameAccess = 2
	}

	enum ProcessInformationClass {
		MemoryPriority = 39
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

		#region Threads

		[DllImport(KernelLib)]
		public static extern IntPtr OpenThread(ThreadAccessMask accessMask, bool inheritHandle, int tid);

		[DllImport(KernelLib)]
		public static extern IntPtr GetCurrentThread();

		[DllImport(KernelLib)]
		public static extern IntPtr GetCurrentProcess();

		#endregion

		#region CPU Sets and Priorities

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool SetProcessDefaultCpuSets(IntPtr hProcess, uint[] cpuSetIds, int cpuSetIdCount);

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool SetThreadSelectedCpuSets(SafeThreadHandle hThread, uint[] cpuSetIds, int cpuSetIdCount);

		[DllImport(KernelLib, SetLastError = true)]
		public unsafe static extern bool GetSystemCpuSetInformation(SYSTEM_CPU_SET_INFORMATION* info, int length, out int returnedLength, IntPtr hProcess, uint flags = 0);

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool SetThreadPriority(SafeThreadHandle handle, NativeThreadPriority priority);

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool SetThreadPriority(IntPtr handle, NativeThreadPriority priority);

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool SetPriorityClass(IntPtr handle, NativeProcessPriority priority);

		#endregion

		#region Private Namespaces

		[DllImport(KernelLib, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr CreateBoundaryDescriptor(string name, uint flags = 0);

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool AddSIDToBoundaryDescriptor(ref IntPtr hBoundaryDescriptor, byte[] sid);

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool DeleteBoundaryDescriptor(IntPtr hBoundaryDescriptor);

		[DllImport(KernelLib, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr CreatePrivateNamespace(IntPtr securityAttributes, IntPtr hBoundaryDescriptor, string prefix);

		[DllImport(KernelLib, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr OpenPrivateNamespace(IntPtr hBoundaryDescriptor, string prefix);

		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool ClosePrivateNamespace(IntPtr hNamespace, PrivateNamespaceFlags flags = PrivateNamespaceFlags.None);

		#endregion

		#region Misc

		[DllImport(KernelLib, SetLastError = true)]
		public static extern IntPtr CreateMemoryResourceNotification(MemoryResourceNotificationType type);


		[DllImport(KernelLib, SetLastError = true)]
		public static extern bool DuplicateHandle(IntPtr hSourceProcess, IntPtr hSourceHandle, IntPtr hTargetProcess, out IntPtr hTargetHandle, uint access, bool inheritHandle, DuplicateHandleOptions options);

		[DllImport("ntdll")]
		public static extern int NtSetInformationProcess(IntPtr hProcss, ProcessInformationClass info, ref int memoryPriority, int size);

		#endregion

	}
}
