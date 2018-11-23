using DotNext.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessWait {
	class Program {
		static void Main(string[] args) {
			const int numProcesses = 5;

			var processes = new ProcessWaitHandle[numProcesses];

			for (int i = 0; i < numProcesses; i++) {
				var process = Process.Start(i % 2 == 0 ? "notepad" : "mspaint");
				Console.WriteLine($"Started {process.ProcessName} (PID={process.Id}...");
				processes[i] = new ProcessWaitHandle(process);
			}
			Console.WriteLine("Waiting for these processes to exit...");

			WaitHandle.WaitAll(processes);
			Console.WriteLine("All processes exited!");
		}
	}
}
