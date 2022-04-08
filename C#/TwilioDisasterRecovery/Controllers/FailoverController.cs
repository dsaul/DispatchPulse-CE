using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Npgsql;
using System.Text.RegularExpressions;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TwilioDisasterRecovery.Controllers
{
	[Route("/Failover")]
	[ApiController]
	public class FailoverController : ControllerBase
	{
		// GET: api/<FailoverController>
		[HttpGet]
		public FileContentResult Get(
			string? CallSid, 
			string? AccountSid, 
			string? From, 
			string? To,
			string? CallStatus,
			string? ApiVersion,
			string? Direction,
			string? ForwardedFrom,
			string? CallerName,
			string? ParentCallSid,
			string? FromCity,
			string? FromState,
			string? FromZip,
			string? FromCountry,
			string? ToCity,
			string? ToState,
			string? ToZip,
			string? ToCountry
			) {

			VoiceResponse response = new VoiceResponse();

			do {
				if (string.IsNullOrWhiteSpace(To)) {
					response.Hangup();
					break;
				}

				string didStr = Regex.Replace(To, "[^0-9]", "");

				using NpgsqlConnection billingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
				billingDB.Open();

				var resRegPN = RegisteredPhoneNumbers.ForPhoneNumber(billingDB, didStr);
				if (!resRegPN.Any()) {
					response.Hangup();
					break;
				}

				RegisteredPhoneNumbers? registeredPhoneNumber = resRegPN.FirstOrDefault().Value;
				if (null == registeredPhoneNumber || null == registeredPhoneNumber.BillingCompanyId) {
					response.Hangup();
					break;
				}

				var resBC = BillingCompanies.ForIds(billingDB, registeredPhoneNumber.BillingCompanyId.Value);
				if (!resBC.Any()) {
					response.Hangup();
					break;
				}

				BillingCompanies? company = resBC.FirstOrDefault().Value;
				if (null == company) {
					response.Hangup();
					break;
				}

				var resPackages = BillingPackages.ForProvisionOnCallAutoAttendants(billingDB, true);
				if (!resPackages.Any()) {
					response.Hangup();
					break;
				}

				var resSubs = BillingSubscriptions.ForCompanyIdPackageIdsAndHasDatabase(billingDB, registeredPhoneNumber.BillingCompanyId.Value, resPackages.Keys);
				if (!resSubs.Any()) {
					response.Hangup();
					break;
				}

				BillingSubscriptions? subscription = resSubs.FirstOrDefault().Value;
				if (null == subscription || string.IsNullOrWhiteSpace(subscription.ProvisionedDatabaseName)) {
					response.Hangup();
					break;
				}

				using NpgsqlConnection dpDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(subscription.ProvisionedDatabaseName));
				dpDB.Open();

				var didsRes = DIDs.ForDIDNumber(dpDB, didStr);
				if (!didsRes.Any()) {
					response.Hangup();
					break;
				}

				DIDs? did = didsRes.FirstOrDefault().Value;
				if (null == did) {
					response.Hangup();
					break;
				}

				if (null == did.AssignToType) {
					response.Hangup();
					break;
				}

				bool fail = false;
				switch (did.AssignToType.Value) {
					default:
					case DIDs.AssignToTypeE.Hangup:
						fail = true;
						break;
					case DIDs.AssignToTypeE.OnCallAutoAttendant:

						Guid? aaId = did.AssignToID;
						if (null == aaId) {
							fail = true;
							break;
						}

						var resAA = OnCallAutoAttendants.ForId(dpDB, aaId.Value);
						if (!resAA.Any()) {
							fail = true;
							break;
						}

						OnCallAutoAttendants? aa = resAA.FirstOrDefault().Value;
						if (null == aa) {
							fail = true;
							break;
						}

						if (string.IsNullOrWhiteSpace(aa.FailoverNumber)) {
							fail = true;
							break;
						}
						
						response.Dial(aa.FailoverNumber);
						//response.Dial("204-890-0024");
						break;
				}
				if (fail == true) {
					response.Hangup();
					break;
				}









				

			} while (false);

			byte[] bytes = Encoding.UTF8.GetBytes(response.ToString());

			return File(bytes, "text/xml");
		}

		
	}
}
