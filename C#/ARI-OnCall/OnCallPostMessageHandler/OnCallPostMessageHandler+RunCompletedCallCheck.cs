using System;
using System.Collections.Generic;
using System.Linq;
using SharedCode.DatabaseSchemas;
using Npgsql;
using SharedCode.Asterisk;
using Serilog;
using System.Text.RegularExpressions;
using SharedCode;
using System.IO;

namespace ARI.IVR.OnCall
{
	public static partial class OnCallPostMessageHandler
	{
		public static void RunCompletedCallCheck(NpgsqlConnection billingDB) {



			/*
			Example failed call:
			Channel: Local/2048900024@outbound_oncall
			WaitTime: 30
			MaxRetries: 0
			Context: on_call_respondee_menu
			Extension: s
			Archive: yes
			Setvar: billingCompanyId=7482705b-8b06-4fb6-83c6-9feec50540ca
			Setvar: voicemailId=67d5ffb8-9874-4ad4-b016-45942a29cd13
			Setvar: databaseName=zclient_dp_dev_0
			Setvar: callWasTo=2048900024

			StartRetry: 12846 1 (1615039783)
			Status: Expired
			*/

			/*
			Example successful call:
			Channel: Local/2048900024@outbound_oncall
			WaitTime: 30
			MaxRetries: 0
			Context: on_call_respondee_menu
			Extension: s
			Archive: yes
			Setvar: billingCompanyId=7482705b-8b06-4fb6-83c6-9feec50540ca
			Setvar: voicemailId=8a93fe57-c10f-47e9-b3b7-b911ab76095c
			Setvar: databaseName=zclient_dp_dev_0
			Setvar: callWasTo=2048900024

			StartRetry: 12846 1 (1615043660)
			Status: Completed
			*/


			DirectoryInfo di = new DirectoryInfo(EnvAsterisk.ARI_OUTGOING_SPOOL_COMPLETED_DIRECTORY);

			FileInfo[] files = di.GetFiles();
			foreach (FileInfo fi in files) {

				if (fi.Name.IndexOf(kOnCallCallFilePrefix) != 0) {
					Log.Verbose("[{FileName}] Skipping file as it doesn't have the prefix {Prefix} ", fi.Name, kOnCallCallFilePrefix);
					continue;
				}

				Log.Information("[{FileName}] Found completed call file ", fi.Name);
				string callFilePath = fi.FullName;
				//Log.Debug("Path:{CallFilePath}", callFilePath);

				string? archivedCallFileContents;
				try {
					archivedCallFileContents = File.ReadAllText(fi.FullName);
				}
				catch (Exception ex) {
					Log.Error(ex, "[{FileName}] Unable to read call file. {CallFilePath}", fi.Name, callFilePath);
					continue;
				}

				// If the call file is empty, we can't do anything more, just delete it.
				if (string.IsNullOrWhiteSpace(archivedCallFileContents)) {
					Log.Warning("[{FileName}] Call file is empty? Deleting it. {CallFilePath}", fi.Name, callFilePath);

					try {
						fi.Delete();
					}
					catch (Exception ex) {
						Log.Error(ex, "[{FileName}] Unable to delete call file. {CallFilePath}", fi.Name, callFilePath);
					}
					continue;
				}

				// Extract the call information from the call file.
				Guid? billingCompanyId = null;
				Guid? voicemailId = null;
				string? databaseName = null;
				string? callWasTo = null;
				string? status = null;
				bool? isBackupTrunk = null;

				Match billingCompanyIdMatch = Regex.Match(archivedCallFileContents, @"(?<=Setvar: billingCompanyId=).*", RegexOptions.IgnoreCase);
				if (billingCompanyIdMatch.Success) {
					string capture = billingCompanyIdMatch.Value;

					if (Guid.TryParse(capture, out Guid tmp)) {
						billingCompanyId = tmp;
					}
				}

				Match voicemailIdMatch = Regex.Match(archivedCallFileContents, @"(?<=Setvar: voicemailId=).*", RegexOptions.IgnoreCase);
				if (voicemailIdMatch.Success) {
					string capture = voicemailIdMatch.Value;

					if (Guid.TryParse(capture, out Guid tmp)) {
						voicemailId = tmp;
					}
				}

				Match databaseNameMatch = Regex.Match(archivedCallFileContents, @"(?<=Setvar: databaseName=).*", RegexOptions.IgnoreCase);
				if (databaseNameMatch.Success) {
					databaseName = databaseNameMatch.Value;
				}

				Match callWasToMatch = Regex.Match(archivedCallFileContents, @"(?<=Setvar: callWasTo=).*", RegexOptions.IgnoreCase);
				if (callWasToMatch.Success) {
					callWasTo = callWasToMatch.Value;
				}

				Match isBackupTrunkMatch = Regex.Match(archivedCallFileContents, @"(?<=Setvar: isBackupTrunk=).*", RegexOptions.IgnoreCase);
				if (isBackupTrunkMatch.Success) {
					string capture = isBackupTrunkMatch.Value;

					if (bool.TryParse(capture, out bool tmp)) {
						isBackupTrunk = tmp;
					}
				}

				Match statusMatch = Regex.Match(archivedCallFileContents, @"(?<=Status: ).*", RegexOptions.IgnoreCase);
				if (statusMatch.Success) {
					status = statusMatch.Value;
				}

				Log.Debug("[{FileName}] billingCompanyId:{BillingCompanyId}", fi.Name, billingCompanyId);
				Log.Debug("[{FileName}] voicemailId:{VoicemailId}", fi.Name, voicemailId);
				Log.Debug("[{FileName}] databaseName:{DatabaseName}", fi.Name, databaseName, callWasTo);
				Log.Debug("[{FileName}] callWasTo:{CallWasTo}", fi.Name, callWasTo);
				Log.Debug("[{FileName}] status:{Status}", fi.Name, status);
				Log.Debug("[{FileName}] isBackupTrunk:{isBackupTrunk}", fi.Name, isBackupTrunk);

				if (null == billingCompanyId ||
					null == voicemailId ||
					string.IsNullOrWhiteSpace(databaseName) ||
					string.IsNullOrWhiteSpace(callWasTo) ||
					string.IsNullOrWhiteSpace(status)
					) {

					Log.Error("[{FileName}] Call File is missing information required to process, deleting it.");
					try {
						fi.Delete();
					}
					catch (Exception ex) {
						Log.Error(ex, "[{FileName}] Unable to delete call file. {CallFilePath}", fi.Name, callFilePath);
					}
					continue;
				}

				// Update the call attempt.
				using NpgsqlConnection dpDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(databaseName));
				dpDB.Open();

				var resVM = Voicemails.ForId(dpDB, voicemailId.Value);
				if (0 == resVM.Count) {
					Log.Error("[{FileName}] Could not find voicemail id in database, deleting call file.", fi.Name);
					try {
						fi.Delete();
					}
					catch (Exception ex) {
						Log.Error(ex, "[{FileName}] Unable to delete call file. {CallFilePath}", fi.Name, callFilePath);
					}
					continue;
				}

				Voicemails message = resVM.FirstOrDefault().Value;

				message = message.UpdateVoicemailWithCompletedCallInformation(dpDB, fi.Name, status, callWasTo, archivedCallFileContents) ?? message;

				// Nothing else we need to do for completed calls except delete the call file.
				if (status == "Completed") {
					Log.Information("[{FileName}] Call file is successful, deleting.", fi.Name);
					try {
						fi.Delete();
					}
					catch (Exception ex) {
						Log.Error(ex, "[{FileName}] Unable to delete call file. {CallFilePath}", fi.Name, callFilePath);
					}
					continue;
				}

				// If the status is something other then completed. We need to, try once more with the backup sip provider.

				if (status != "Completed" && null != isBackupTrunk && true == isBackupTrunk.Value) {
					Log.Error("[{FileName}] Call file failed, but we are already on the backup trunks so we'll stop here.", fi.Name);
					try {
						fi.Delete();
					}
					catch (Exception ex) {
						Log.Error(ex, "[{FileName}] Unable to delete call file. {CallFilePath}", fi.Name, callFilePath);
					}
					continue;
				}




				// We don't want to figure anything new out, just take the original call file and string replace the bad context with the new one.

				// Delete the original call file so this isn't handled multiple times.
				try {
					fi.Delete();
				}
				catch (Exception ex) {
					Log.Error(ex, "[{FileName}] Unable to delete call file. {CallFilePath}", fi.Name, callFilePath);
				}


				List<Voicemails.CallAttemptsProgressEntry> attempts = message.OnCallAttemptsProgress;

				for (int i = 0; i < attempts.Count; i++) {
					Voicemails.CallAttemptsProgressEntry entry = attempts[i];

					if (null == entry.CallFiles)
						continue;

					foreach (Voicemails.CallFile callFile in entry.CallFiles) {

						if (callFile.FileName != fi.Name) {
							continue;
						}

						string? originalCallFileContents = callFile.OriginalCallFile;
						if (string.IsNullOrWhiteSpace(originalCallFileContents))
							continue;

						string? channel = null;
						Match channelsMatch = Regex.Match(originalCallFileContents, @"(?<=Channel: ).*", RegexOptions.IgnoreCase);
						if (channelsMatch.Success) {
							channel = channelsMatch.Value;
						}
						if (null == channel)
							continue;
						if (null == message.Id)
							continue;

						channel = channel.Replace($"@{kAstOutboundCallContextPrimary}", $"@{kAstOutboundCallContextSecondary}");


						bool successfullySentToPBX = SpooledCall.Call(
							astChannel: channel,
							context: kAstOnCallRepContext,
							extension: kAstOnCallRepContextExtension,
							callCategory: message.Id.Value.ToString(),
							callFileName: out string? callFileName,
							callFileContents: out string? callFileContents,
							callFilePrefix: kOnCallCallFilePrefix,
							variables: new Dictionary<string, string> {
								{ "billingCompanyId", billingCompanyId.Value.ToString() },
								{ "voicemailId", message.Id.Value.ToString() },
								{ "databaseName", databaseName },
								{ "callWasTo", callWasTo },
								{ "isBackupTrunk", "true" }
							}
						);

						if (!successfullySentToPBX) {
							continue;
						}


						message = message.SaveNewBackupCallFileAtIndex(dpDB, i, $"Trying to call {callWasTo} on the backup phone lines.", callFile) ?? message;
						message = message.MarkNextAttemptTime(dpDB) ?? message;


					}
				}








			}

		}
	}
}
