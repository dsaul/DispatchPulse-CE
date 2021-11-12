using Amazon.Polly;
using Amazon.Polly.Model;
using Databases.Records.TTS;
using System;
using System.IO;
using TTS;

namespace AWSPollyTest
{
	class Program
	{
		
		
		static void Main()
		{
			Cache? entry = PollyText.EnsureDatabaseEntry("Welcome to earth!", Engine.Neural, VoiceId.Brian);

			if (null == entry)
				return;

			string? path = entry.S3LocalPCMPath("/srv/tts-cache/", '/', true);
			Console.WriteLine(path);

			//AmazonPollyClient pc = new AmazonPollyClient(Amazon.RegionEndpoint.GetBySystemName("us-east-1"));

			//SynthesizeSpeechRequest sreq = new SynthesizeSpeechRequest();
			//sreq.Text = "Your Sample Text Here";
			//sreq.OutputFormat = OutputFormat.Mp3;
			//sreq.VoiceId = VoiceId.Amy;
			//sreq.Engine = Engine.Neural;



			//SynthesizeSpeechResponse sres = pc.SynthesizeSpeechAsync(sreq).Result;

			//using (var fileStream = File.Create(@"Y:\generated-voice\yourfile.mp3")) {
			//	sres.AudioStream.CopyTo(fileStream);
			//	fileStream.Flush();
			//	fileStream.Close();
			//}

		}
	}
}
