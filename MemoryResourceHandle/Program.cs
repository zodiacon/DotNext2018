using DotNext.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryResourceHandle {
	class Program {
		static void Main(string[] args) {
			using (var memoryRes = new MemoryResourceNotification(MemoryResourceNotificationType.Low)) {
				Console.WriteLine("Waiting for memory low notification...");
				memoryRes.WaitOne();
				Console.WriteLine("Memory available is Low");
			}
		}
	}
}
