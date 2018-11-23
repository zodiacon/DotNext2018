using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotNext.Native {
	public enum JobAccessMask : uint {
		AllAccess = 0x1f001f,
		AssignProcess = 1,
		Query = 4,
		SetAttributes = 2,
		SetSecurityAttributes = 0x10,
		Terminate = 8
	}

	public sealed class Job : IDisposable {
		IntPtr _handle;

		public static Job Create(string name = null) {
			var handle = NativeMethods.CreateJobObject(IntPtr.Zero, name);
			if (handle == IntPtr.Zero)
				throw new Win32Exception();
			return new Job(handle);
		}

		public static Job Open(JobAccessMask accessMask, string name) {
			var handle = NativeMethods.OpenJobObject(accessMask, false, name);
			if (handle == IntPtr.Zero)
				throw new Win32Exception();
			return new Job(handle);
		}

		private Job(IntPtr handle) {
			_handle = handle;
		}

		public void Kill(int exitCode = 0) {
			if (!NativeMethods.TerminateJobObject(_handle, exitCode))
				throw new Win32Exception();
		}

		public void AddProcess(Process process) {
			if (!NativeMethods.AssignProcessToJobObject(_handle, process.Handle))
				throw new Win32Exception();
		}

		public void SetCpuLimit(int percent, bool hardCap = true) {
			JobCpuRateControlInformation info;
			info.CpuRate = percent * 100;
			info.Flags = CpuRateControlFlags.Enable | (hardCap ? CpuRateControlFlags.HardCap : CpuRateControlFlags.None);
			if (!NativeMethods.SetInformationJobObject(_handle, JobInformationClass.CpuRateControlInformation, info, Marshal.SizeOf<JobCpuRateControlInformation>()))
				throw new Win32Exception();
		}

		void Dispose(bool disposing) {
			if (_handle == IntPtr.Zero)
				return;

			NativeMethods.CloseHandle(_handle);
			_handle = IntPtr.Zero;
			if (disposing)
				GC.SuppressFinalize(this);
		}

		public void Dispose() {
			Dispose(true);
		}

		~Job() {
			Dispose(false);
		}
	}
}
