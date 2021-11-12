using API.Hubs;
using API.Utility;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManuallyProcessPreAuthorizedPayments.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}


		
	}
}
