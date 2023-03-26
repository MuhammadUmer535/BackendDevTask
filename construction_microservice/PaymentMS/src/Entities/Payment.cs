namespace PaymentMS.src.Entities
{
	public class Payment
	{
		#region Properties

		public int PaymentId { get; set; }
		public int RequestId { get; set; }
		public decimal PaymentAmount { get; set; }
		public DateTime PaymentDate { get; set; }

		#endregion
	}
}