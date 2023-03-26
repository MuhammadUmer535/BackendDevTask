namespace ConstructMS.Entities
{
    public abstract class Entity
    {
		#region Protected Properties

        public string TableName { get; set; }

        #endregion

        #region public constructor

        #endregion

        #region public Properties

        public object PrimaryKey { get; set; }
        
		public string PrimaryKeyName { get; set; }

		#endregion

        #region PropertyChange

        #endregion

		#region public Methods

		public virtual Dictionary<string, object> GetColumns()
        {
            return null;
        }

		#endregion
    }
}
