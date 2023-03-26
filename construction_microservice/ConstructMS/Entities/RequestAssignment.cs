namespace ConstructMS.Entities
{
	public class RequestAssignment : Entity
	{
		#region Properties
		public int RequestAssignmentId { get; set; }
		public int RequestId { get; set; }
		public int ContractorId { get; set; }
		public DateTime AssignmentDate { get; set; }
		public DateTime? CompletionDate { get; set; }

		#endregion
	}
}