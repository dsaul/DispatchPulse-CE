namespace API.Hubs
{
	public abstract class IdempotencyRequest : HubRequest
	{
		public string? IdempotencyToken { get; set; }
		public string? RoundTripRequestId { get; set; }
	}
}
