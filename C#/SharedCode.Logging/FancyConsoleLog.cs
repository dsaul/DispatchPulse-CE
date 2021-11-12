﻿using System.IO;

namespace SharedCode.ARI
{
	public static class FancyConsoleLog
	{
		public static void Log(string message,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) {
			//System.Diagnostics.Trace.WriteLine("message: " + message);
			//System.Diagnostics.Trace.WriteLine("member name: " + memberName);
			//System.Diagnostics.Trace.WriteLine("source file path: " + sourceFilePath);
			//System.Diagnostics.Trace.WriteLine("source line number: " + sourceLineNumber);

			Serilog.Log.Debug($"[{Path.GetFileNameWithoutExtension(sourceFilePath)}::{sourceLineNumber}::{memberName}] {message}");
			
		}
	}
}
