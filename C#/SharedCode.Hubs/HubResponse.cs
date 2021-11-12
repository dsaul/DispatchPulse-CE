using Microsoft.AspNetCore.SignalR.Protocol;

namespace API.Hubs
{
	public abstract class HubResponse : HubMessage
	{
		public bool? IsError { get; set; }
		public string? ErrorMessage { get; set; }
		public bool? IsPermissionsError { get; set; }
		public bool? ForceLogout { get; set; }
	}
}
