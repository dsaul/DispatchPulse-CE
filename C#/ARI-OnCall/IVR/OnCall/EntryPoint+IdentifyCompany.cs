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
using SharedCode.Extensions;
using Serilog;

namespace ARI.IVR.OnCall
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void IdentifyCompany(AGIRequest request, AGIChannel channel, LeaveMessageRequestData requestData) {


			string? dedicatedDid = GetFullVariable("${DEDICATED_DID}");

			// Set caller id variables if avaliable.

			string? callerId = request.CallerId;
			if (!string.IsNullOrWhiteSpace(callerId) && callerId != "Anonymous") {
				requestData.CallerIdNonDigitsRemoved = Regex.Replace(callerId, "[^.0-9]", "");
				requestData.CallerIdNonDigitsRemovedWithSpaces = requestData.CallerIdNonDigitsRemoved.WithSpacesBetweenLetters();
				requestData.CallerIdNumber = requestData.CallerIdNonDigitsRemoved;
			}

			requestData.ConnectToBillingDB();
			if (null == requestData.BillingDB) {
				Log.Error("[{AGIRequestUniqueId}] null == data.BillingDB", request.UniqueId);
				PlayTTS("We're sorry, there was an error connecting to database. Please try again later.", string.Empty, Engine.Neural, VoiceId.Brian);
				throw new PerformHangupException();
			}

			if (!string.IsNullOrWhiteSpace(dedicatedDid)) {
				IdentifyCompanyDedicatedDID(request, channel, requestData, dedicatedDid);
			} else {
				IdentifyCompanyViaCallerID(request, channel, requestData);
			}

			throw new PerformHangupException();
		}

		protected void IdentifyCompanyDedicatedDID(AGIRequest request, AGIChannel channel, LeaveMessageRequestData requestData, string dedicatedDid) {


			if (null == requestData.BillingDB) {
				Log.Error("[{AGIRequestUniqueId}] null == data.BillingDB", request.UniqueId);
				throw new PerformHangupException();
			}


			var resRegPhoneNumbers = RegisteredPhoneNumbers.ForPhoneNumber(requestData.BillingDB, dedicatedDid);
			if (0 != resRegPhoneNumbers.Count) {
				requestData.RegisteredPhoneNumber = resRegPhoneNumbers.FirstOrDefault().Value;

				requestData.AddTimelineEntry(
					type: LeaveMessageRequestData.TimelineType.text,
					timestampISO8601: DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture),
					description: $"Identified registered phone number \"{dedicatedDid}\".",
					colour: "#ccc");

				ExistingPhoneNumber(request, channel, requestData);
			} else {

				Log.Warning("We're sorry, this number is not registered on this system. {DedicatedDid}", dedicatedDid);

				PlayTTS("We're sorry, this number is not registered on this system.", string.Empty, Engine.Neural, VoiceId.Brian);
				throw new PerformHangupException();
			}



		}


		protected void IdentifyCompanyViaCallerID(AGIRequest request, AGIChannel channel, LeaveMessageRequestData requestData) {
			if (string.IsNullOrWhiteSpace(request.CallerId) || request.CallerId == "Anonymous") {
				PlayTTS("Welcome to On Call Responder, by Dispatch Pulse. We did not get a caller id " +
					"with your call, if you are not able to send caller id, we can setup a dedicated " +
					"phone number for you. Please contact support to set this up.", string.Empty, Engine.Neural, VoiceId.Brian);
				throw new PerformHangupException();
			}

			if (null == requestData.BillingDB) {
				Log.Error("[{AGIRequestUniqueId}] null == data.BillingDB", request.UniqueId);
				throw new PerformHangupException();
			}

			if (null == requestData.BillingDB) {
				Log.Error("[{AGIRequestUniqueId}] null == data.BillingDB", request.UniqueId);
				throw new PerformHangupException();
			}

			if (null == requestData.CallerIdNonDigitsRemoved) {
				Log.Error("[{AGIRequestUniqueId}] null == requestData.CallerIdNonDigitsRemoved", request.UniqueId);
				throw new PerformHangupException();
			}


			var resRegPhoneNumbers = RegisteredPhoneNumbers.ForPhoneNumber(requestData.BillingDB, requestData.CallerIdNonDigitsRemoved);
			if (0 != resRegPhoneNumbers.Count) {
				requestData.RegisteredPhoneNumber = resRegPhoneNumbers.FirstOrDefault().Value;

				requestData.AddTimelineEntry(
					type: LeaveMessageRequestData.TimelineType.text,
					timestampISO8601: DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture),
					description: $"Identified registered phone number \"{requestData.CallerIdNonDigitsRemoved}\".",
					colour: "#ccc");

				ExistingPhoneNumber(request, channel, requestData);
			} else {

				Log.Warning("Can't find {CallerIdNonDigitsRemoved} beginning new number registration.", requestData.CallerIdNonDigitsRemoved);
				BeginNewNumberRegistration(request, channel, requestData);
			}
		}



	}
}
