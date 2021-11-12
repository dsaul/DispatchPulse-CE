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
using Databases.Records.CRM;
using Databases.Records.Billing;
using Databases.Records;
using Amazon.Polly;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void WelcomeToCompanyAccess(AGIRequest request, AGIChannel channel, RequestData data) {

			

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
