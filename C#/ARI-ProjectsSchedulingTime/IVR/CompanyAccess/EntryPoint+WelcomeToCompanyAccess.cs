using System;
using System.Collections.Generic;
using System.Text;
using AsterNET.FastAGI;
using Npgsql;
using System.Linq;
using Newtonsoft.Json.Linq;
using SharedCode;
using System.IO;
using Afk.ZoneInfo;
using System.Text.RegularExpressions;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using System.Threading.Tasks;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected async Task WelcomeToCompanyAccess(AGIRequest request, AGIChannel channel, RequestData data) {

			

			//PlayPollyText("1234", escapeAllKeys);

			PlayTTS("Welcome to company access.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

			

			int attempt = 0;

			while (true) {
				if (attempt > 3)
					throw new PerformHangupException();

				EnterCompanyId(request, channel, data);

				attempt ++;
			}

			
		}
	}
}
