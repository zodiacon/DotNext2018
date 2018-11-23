using DotNext.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpuLimit {
	static class Program {
		static void Main(string[] args) {
			if (args.Length < 2) {
				Console.WriteLine("Usage: cpulimit <pid> [<pid> ...] <percent>");
				return;
			}

			try {
				using (var job = Job.Create()) {
					for (int i = 0; i < args.Length - 1; i++) {
						int pid = int.Parse(args[i]);
						using (var process = Process.GetProcessById(pid)) {
							job.AddProcess(process);
						}
					}

					int percent = int.Parse(args.Last());

					job.SetCpuLimit(percent);

					Console.WriteLine("CPU limit set successfully.");
				}

			}
			catch (Exception ex) {
				Console.WriteLine($"Error: {ex.Message}");
			}
		}
	}
}
