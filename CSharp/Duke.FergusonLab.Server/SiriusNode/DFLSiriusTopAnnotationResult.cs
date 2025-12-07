//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

namespace Duke.FergusonLab.Server.SiriusNode
{
	/// <summary>
	/// Holds all information about SIRIUS top annotation result.
	/// </summary>
	internal class DFLSiriusTopAnnotationResult
	{
		/// <summary>
		/// Gets or sets the external item ID.
		/// </summary>
		public int ExternalID { get; set; }

		/// <summary>
		/// Gets or sets the elemental composition.
		/// </summary>
		public string Formula { get; set; }

		/// <summary>
		/// Gets or sets the formula score.
		/// </summary>
		public double? FormulaScore { get; set; }

		/// <summary>
		/// Gets or sets the CSIFingerID name.
		/// </summary>
		public string CSIFingerIDName { get; set; }

		/// <summary>
		/// Gets or sets the CSIFingerID InChIKey.
		/// </summary>
		public string CSIFingerIDInChIKey { get; set; }

		/// <summary>
		/// Gets or sets the CSIFingerID score.
		/// </summary>
		public double? CSIFingerIDScore { get; set; }

		/// <summary>
		/// Gets or sets the CSIFingerID Tanimoto similarity.
		/// </summary>
		public double? CSIFingerIDTanimotoSimilarity { get; set; }

		/// <summary>
		/// Gets or sets the CSIFingerID exact confidence.
		/// </summary>
		public double? CSIFingerIDConfidenceExact { get; set; }

		/// <summary>
		/// Gets or sets the CSIFingerID approximate confidence.
		/// </summary>
		public double? CSIFingerIDConfidenceApprox { get; set; }

		/// <summary>
		/// Gets or sets the ClassyFire level 1 name.
		/// </summary>
		public string ClassyFireLevel1 { get; set; }

		/// <summary>
		/// Gets or sets the ClassyFire level 2 name.
		/// </summary>
		public string ClassyFireLevel2 { get; set; }

		/// <summary>
		/// Gets or sets the ClassyFire level 3 name.
		/// </summary>
		public string ClassyFireLevel3 { get; set; }

		/// <summary>
		/// Gets or sets the ClassyFire level 4 name.
		/// </summary>
		public string ClassyFireLevel4 { get; set; }

		/// <summary>
		/// Gets or sets the ClassyFire level 5 name.
		/// </summary>
		public string ClassyFireLevel5 { get; set; }

		/// <summary>
		/// Gets or sets the ClassyFire level 6 name.
		/// </summary>
		public string ClassyFireLevel6 { get; set; }
	}
}
