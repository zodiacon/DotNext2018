using Microsoft.Diagnostics.Runtime;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace DotNext.Native {
	public enum NativeThreadPriority {
		Idle = -15,
		Lowest = -2,
		BelowNormal = -1,
		Normal = 0,
		AbiveNormal = 1,
		Highest = 2,
		TimeCritical = 15,
		BackgroundBegin = 0x10000,
		BackgroundEnd = 0x20000
	}


	public static class ThreadExtensions {
		sealed class BackgroundEnd : IDisposable {
			public void Dispose() {
				NativeMethods.SetThreadPriority(NativeMethods.GetCurrentThread(), NativeThreadPriority.BackgroundEnd);
			}
		}

		static DataTarget _target;
		static ClrRuntime _runtime;

		static ConcurrentDictionary<int, int> _threadIds = new ConcurrentDictionary<int, int>(2, 16);

		static ThreadExtensions() {
			_target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 0, AttachFlag.Passive);
			_runtime = _target.ClrVersions[0].CreateRuntime();
		}

		public static int GetOSThreadId(this Thread thread) {
			if (_threadIds.TryGetValue(thread.ManagedThreadId, out var id))
				return id;
			id = (int)_runtime.Threads.First(t => t.ManagedThreadId == thread.ManagedThreadId).OSThreadId;
			_threadIds.AddOrUpdate(thread.ManagedThreadId, id, (_, __) => id);
			return id;
		}

		public static void SetPriority(this Thread thread, NativeThreadPriority priority) {
			using (var hThread = new SafeThreadHandle(thread.GetOSThreadId(), ThreadAccessMask.SetLimitedInformation)) {
				if (!NativeMethods.SetThreadPriority(hThread, priority))
					throw new Win32Exception();
			}
		}

		public static IDisposable BeginBackgroundMode() {
			if (!NativeMethods.SetThreadPriority(NativeMethods.GetCurrentThread(), NativeThreadPriority.BackgroundBegin))
				throw new Win32Exception();
			return new BackgroundEnd();
		}

		public static void SetSelectedCpuSets(this Thread thread, ulong cpus) {
			var cpuSets = ProcessExtensions.BuildCpuSets(cpus);
			using (var hThread = new SafeThreadHandle(thread.GetOSThreadId(), ThreadAccessMask.SetLimitedInformation)) {
				if (!NativeMethods.SetThreadSelectedCpuSets(hThread, cpuSets, cpuSets.Length))
					throw new Win32Exception();
			}
		}
	}
}
