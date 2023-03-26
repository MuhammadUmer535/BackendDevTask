namespace ConstructMS.Entities
{
	public class ConstructionRequest : Entity
	{
		#region Properties
		
		public int RequestId { get; set; }
		public string ClientName { get; set; }
		public string ClientEmail { get; set; }
		public string? ClientPhone { get; set; }
		public int ConstructionServiceId { get; set; }
		public DateTime RequestDate { get; set; }
		public string Status { get; set; }

		#endregion
	}
}