using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNext.Native {
	public enum NativeProcessPriority {
		Normal = 0x20,
		AboveNormal = 0x8000,
		High = 0x80,
		Realtime = 0x100,
		BelowNormal = 0x4000,
		Idle = 0x40,
		BackgroundBegin = 0x100000,
		BackgroundEnd = 0x200000
	}

	public static class ProcessExtensions {
		static SYSTEM_CPU_SET_INFORMATION[] _systemCpuSets;

		unsafe static ProcessExtensions() {
			_systemCpuSets = new SYSTEM_CPU_SET_INFORMATION[Environment.ProcessorCount];
			int len = sizeof(SYSTEM_CPU_SET_INFORMATION) * Environment.ProcessorCount;
			fixed (SYSTEM_CPU_SET_INFORMATION* p = _systemCpuSets) {
				if (!NativeMethods.GetSystemCpuSetInformation(p, len, out len, IntPtr.Zero))
					throw new Win32Exception();
			}

		}

		public static void SetDefaultCpuSets(this Process process, ulong cpus) {
			var cpuSets = BuildCpuSets(cpus);
			if (!NativeMethods.SetProcessDefaultCpuSets(process.Handle, cpuSets.ToArray(), cpuSets.Length))
				throw new Win32Exception();
		}

		public static uint[] BuildCpuSets(ulong cpus) {
			var cpuSets = new List<uint>(Environment.ProcessorCount);
			int index = 0;
			while (cpus > 0) {
				if ((cpus & 1) == 1)
					cpuSets.Add(_systemCpuSets[index].Id);
				cpus >>= 1;
				index++;
			}
			return cpuSets.ToArray();
		}

		public static void SetBackgroundMode() {
			if (!NativeMethods.SetPriorityClass(NativeMethods.GetCurrentProcess(), NativeProcessPriority.BackgroundBegin))
				throw new Win32Exception();
		}

		public static void EndBackgroundMode() {
			if (!NativeMethods.SetPriorityClass(NativeMethods.GetCurrentProcess(), NativeProcessPriority.BackgroundEnd))
				throw new Win32Exception();
		}

		public static void SetMemoryPriority(this Process process, int priority) {
			int status = NativeMethods.NtSetInformationProcess(process.Handle, ProcessInformationClass.MemoryPriority, ref priority, sizeof(int));
			if(status < 0)
				throw new Win32Exception(status);
		}

	}
}
