using System;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		

		public override Task OnConnectedAsync()
		{
			//string name = Context.User.Identity.Name;

			//Groups.AddToGroupAsync(Context.ConnectionId, name);

			return base.OnConnectedAsync();
		}



	}
}
