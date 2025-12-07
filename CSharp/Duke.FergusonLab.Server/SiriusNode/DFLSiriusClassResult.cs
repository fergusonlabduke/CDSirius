//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

namespace Duke.FergusonLab.Server.SiriusNode
{
	/// <summary>
	/// Holds all information about SIRIUS class result.
	/// </summary>
	internal class DFLSiriusClassResult
	{
		/// <summary>
		/// Gets or sets the external item ID.
		/// </summary>
		public int ExternalID { get; set; }

		/// <summary>
		/// Gets or sets the formula result ID.
		/// </summary>
		public string SiriusFormulaID { get; set; }

		/// <summary>
		/// Gets or sets the SIRIUS class ID.
		/// </summary>
		public int ClassID { get; set; }

		/// <summary>
		/// Gets or sets the class tree ID.
		/// </summary>
		public int TreeID { get; set; }

		/// <summary>
		/// Gets or sets the classification level index.
		/// </summary>
		public int LevelIndex { get; set; }

		/// <summary>
		/// Gets or sets the classification level.
		/// </summary>
		public string Level { get; set; }

		/// <summary>
		/// Gets or sets the classification name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the probability.
		/// </summary>
		public double Probability { get; set; }
	}
}
