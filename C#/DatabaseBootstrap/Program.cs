using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseBootstrap
{
	public class Program
	{
		public static void Main(string[] args) {
			Console.WriteLine("Dispatch Pulse Database Bootstrap");















			// We don't want docker to keep relaunching this program,
			// so if it got to this point, sleep repeatedly forever.
			while (true) {
				Thread.Sleep(1000);
			}
		}
	}
}