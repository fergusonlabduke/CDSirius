//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

namespace Duke.FergusonLab.Server.SiriusNode
{
	/// <summary>
	/// Holds all information about SIRIUS formula result.
	/// </summary>
	internal class DFLSiriusFormulaResult
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
		/// Gets or sets the elemental composition.
		/// </summary>
		public string Formula { get; set; }

		/// <summary>
		/// Gets or sets the ion description.
		/// </summary>
		public string Adduct { get; set; }

		/// <summary>
		/// Gets or sets the rank.
		/// </summary>
		public int Rank { get; set; }

		/// <summary>
		/// Gets or sets the SIRIUS score.
		/// </summary>
		public double? SiriusScore { get; set; }

		/// <summary>
		/// Gets or sets the tree score.
		/// </summary>
		public double? TreeScore { get; set; }

		/// <summary>
		/// Gets or sets the isotope score.
		/// </summary>
		public double? IsotopeScore { get; set; }

		/// <summary>
		/// Gets or sets the MS2 median error in ppm.
		/// </summary>
		public double? MS2ErrorPpm { get; set; }

		/// <summary>
		/// Gets or sets the number of explainable peaks.
		/// </summary>
		public int? ExplainablePeaksCount { get; set; }

		/// <summary>
		/// Gets or sets the number of explained peaks.
		/// </summary>
		public int? ExplainedPeaksCount { get; set; }

		/// <summary>
		/// Gets or sets the explained intensity.
		/// </summary>
		public double? ExplainedIntensity { get; set; }
	}
}
