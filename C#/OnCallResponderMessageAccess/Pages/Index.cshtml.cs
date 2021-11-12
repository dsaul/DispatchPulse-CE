using Databases.Records.Billing;
using Databases.Records.CRM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using OnCallResponderMessageAccess.Properties;

namespace OnCallResponderMessageAccess.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger) {
			_logger = logger;
		}

		[BindProperty(SupportsGet = true)]
		public Guid? BillingCompanyId { get; set; }
		[BindProperty(SupportsGet = true)]
		public Guid? VoicemailId { get; set; }
		[BindProperty(SupportsGet = true)]
		public string? AdditionalDataBase64 { get; set; }

		public NpgsqlConnection? BillingDB { get; set; } = null;
		public NpgsqlConnection? DPDB { get; set; } = null;
		public BillingCompanies? BillingCompany { get; set; } = null;
		public BillingSubscriptions? BillingSubscription { get; set; } = null;
		public List<Guid> DPPackageKeys { get; set; } = new List<Guid>();
		public string? DPDatabaseName { get; set; } = null;
		public Voicemails? Voicemail { get; set; } = null;
		public bool PostSuccess { get; set; } = false;
		public string? UserDescription { get; set; } = null;

		public IActionResult OnGet() {

			if (!SharedSetup())
				return Page();

			return Page();
		}

		public IActionResult OnPost(bool markAsHandled) {

			

			if (!SharedSetup())
				return Page();

			if (!markAsHandled)
				return Page();

			if (null == Voicemail)
				return Page();

			if (null == DPDB)
				return Page();

			Voicemail = Voicemail.MarkHandled(DPDB, string.IsNullOrWhiteSpace(UserDescription) ? "Unknown" : UserDescription, Resources.MarkedHandledNotificationEmailTemplate, BillingCompanyId, BillingCompany) ?? Voicemail;
			
			PostSuccess = true;


			return Page();
		}

		private bool SharedSetup() {
			if (null == BillingCompanyId)
				return false;

			if (null == VoicemailId)
				return false;

			// Parse additional data.
			if (string.IsNullOrWhiteSpace(AdditionalDataBase64))
				return false;
			byte[] additionalDataBytes = Convert.FromBase64String(AdditionalDataBase64);
			string? additionalDataStr = Encoding.UTF8.GetString(additionalDataBytes);
			if (string.IsNullOrWhiteSpace(additionalDataStr))
				return false;
			JObject? additionalData = JObject.Parse(additionalDataStr);
			if (null == additionalData)
				return false;
			UserDescription = additionalData.Value<string?>("description");
			if (string.IsNullOrWhiteSpace(UserDescription))
				return false;





			BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.KBillingDatabaseName));
			if (null == BillingDB)
				return false;
			BillingDB.Open();

			var resBC = BillingCompanies.ForIds(BillingDB, BillingCompanyId.Value);
			if (0 == resBC.Count)
				return false;

			BillingCompany = resBC.FirstOrDefault().Value;
			if (null == BillingCompany)
				return false;

			var resPackages = BillingPackages.ForProvisionOnCallAutoAttendants(BillingDB, true);
			if (0 == resPackages.Count)
				return false;

			DPPackageKeys.AddRange(resPackages.Keys);

			var resSubs = BillingSubscriptions.ForCompanyIdPackageIdsAndHasDatabase(BillingDB, BillingCompanyId.Value, DPPackageKeys);
			if (0 == resSubs.Count)
				return false;

			BillingSubscription = resSubs.FirstOrDefault().Value;
			if (null == BillingSubscription)
				return false;

			if (string.IsNullOrWhiteSpace(BillingSubscription.ProvisionedDatabaseName))
				return false;

			DPDatabaseName = BillingSubscription.ProvisionedDatabaseName;
			if (string.IsNullOrWhiteSpace(DPDatabaseName))
				return false;

			DPDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(DPDatabaseName));
			if (null == DPDB)
				return false;
			DPDB.Open();

			var resVM = Voicemails.ForId(DPDB, VoicemailId.Value);
			if (0 == resVM.Count)
				return false;

			Voicemail = resVM.FirstOrDefault().Value;
			if (null == Voicemail)
				return false;

			return true;
		}



	}
}
