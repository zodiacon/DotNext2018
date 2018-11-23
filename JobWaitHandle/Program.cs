using DotNext.Native;
using System;
using System.Diagnostics;

namespace JobWaitHandle {
	static class Program {
		static void Main(string[] args) {
			using (var job = Job2.Create()) {
				for (int i = 0; i < 5; i++) {
					var process = Process.Start(i % 2 == 0 ? "mspaint" : "notepad");
					Console.WriteLine($"Process started: {process.ProcessName} {process.Id}");
					job.AddProcess(process);
				}
				Console.WriteLine("Waiting for created processes to exit...");
				job.WaitOne();
				Console.WriteLine("All processes exited");

			}
		}
	}
}
