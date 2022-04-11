using System;
using AsterNET.FastAGI;
using System.Linq;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Serilog;
using System.Threading.Tasks;

namespace ARI.IVR.OnCall
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected async Task ExistingPhoneNumber(AGIRequest request, AGIChannel channel, LeaveMessageRequestData requestData) {
			
			requestData.ConnectToBillingDB();

			if (null == requestData.BillingDB)
				await ThrowError(request, "3a9s", "null == data.BillingDB");
			if (string.IsNullOrWhiteSpace(requestData.CallerIdNonDigitsRemoved))
				await ThrowError(request, "266f", "string.IsNullOrWhiteSpace(data.CallerIdNonDigitsRemoved)");

			RegisteredPhoneNumbers? rpn = requestData.RegisteredPhoneNumber;
			if (null == rpn)
				await ThrowError(request, "3211", "null == rpn");
			if (null == rpn.PhoneNumber)
				await ThrowError(request, "3212", "null == rpn.PhoneNumber");

			Guid? billingCompanyId = rpn.BillingCompanyId;
			if (null == billingCompanyId)
				await ThrowError(request, "6221", "null == billingCompanyId");
			
			var resCompany = BillingCompanies.ForIds(requestData.BillingDB, billingCompanyId.Value);
			if (0 == resCompany.Count)
				await ThrowError(request, "2544", "0 == resCompany.Count");

			BillingCompanies company = resCompany.FirstOrDefault().Value;
			requestData.BillingCompany = company;
			Log.Information("[{AGIRequestUniqueId}] Call assigned to company {BillingCompanyFullName}.", request.UniqueId, requestData.BillingCompany.FullName);

			requestData.AddTimelineEntry(
				type: LeaveMessageRequestData.TimelineType.text,
				timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				description: $"Identified company \"{requestData.BillingCompany.FullName}\".",
				colour: "#ccc");

			var resPackages = BillingPackages.ForProvisionOnCallAutoAttendants(requestData.BillingDB, true);
			if (0 == resPackages.Count)
				await ThrowError(request, "23ds", "0 == resPackages.Count");

			var resSubs = BillingSubscriptions.ForCompanyIdPackageIdsAndHasDatabase(requestData.BillingDB, billingCompanyId.Value, resPackages.Keys);
			if (0 == resSubs.Count)
				await ThrowError(request, "21ds", "0 == resSubs.Count");

			requestData.Subscription = resSubs.FirstOrDefault().Value;

			if (null == requestData.Subscription)
				await ThrowError(request, "aa62", "0 == resSubs.Count");

			if (string.IsNullOrWhiteSpace(requestData.Subscription.ProvisionedDatabaseName))
				await ThrowError(request, "dlk3", "string.IsNullOrWhiteSpace(data.Subscription.ProvisionedDatabaseName)");

			requestData.DPDatabaseName = requestData.Subscription.ProvisionedDatabaseName;

			Log.Information("[{AGIRequestUniqueId}] Database {Database}.", request.UniqueId, requestData.DPDatabaseName);

			requestData.ConnectToDPDBName(requestData.DPDatabaseName);
			if (null == requestData.DPDB)
				await ThrowError(request, "25sp", "null == data.DPDB");

			var didsRes = DIDs.ForDIDNumber(requestData.DPDB, rpn.PhoneNumber);
			if (0 == didsRes.Count)
				await ThrowError(request, "3255", "0 == didsRes.Count");

			requestData.DPDID = didsRes.First().Value;

			Log.Information("[{AGIRequestUniqueId}] Found DP DID {DIDNumber}, AssignToType {AssignToType}.", request.UniqueId, requestData.DPDID.DIDNumber, requestData.DPDID.AssignToType);

			if (null == requestData.DPDID.AssignToType)
				await ThrowError(request, "3225", "null == did.AssignToType");

			switch (requestData.DPDID.AssignToType.Value) {
				default:
				case DIDs.AssignToTypeE.Hangup:
					throw new PerformHangupException();
				case DIDs.AssignToTypeE.OnCallAutoAttendant:
					requestData.Type = Voicemails.kJsonKeyTypeValueOnCall;
					await OnCallAutoAttendantStart(request, channel, requestData);
					throw new PerformHangupException();
			}

		}
	}
}
