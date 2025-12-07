//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

namespace Duke.FergusonLab.Server.SiriusNode
{
	/// <summary>
	/// Holds information about SIRIUS structure result.
	/// </summary>
	internal class DFLSiriusStructureResult
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
		/// Gets or sets the name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the elemental composition.
		/// </summary>
		public string Formula { get; set; }

		/// <summary>
		/// Gets or sets the octanol-water partition coefficient.
		/// </summary>
		public double? LogKow { get; set; }

		/// <summary>
		/// Gets or sets the PubChem ID.
		/// </summary>
		public string PUBCHEM { get; set; }

		/// <summary>
		/// Gets or sets the DSSTox ID.
		/// </summary>
		public string DSSTOX { get; set; }

		/// <summary>
		/// Gets or sets the KEGG ID.
		/// </summary>
		public string KEGG { get; set; }

		/// <summary>
		/// Gets or sets the HMDB ID.
		/// </summary>
		public string HMDB { get; set; }

		/// <summary>
		/// Gets or sets the InChIKey.
		/// </summary>
		public string InChIKey { get; set; }

		/// <summary>
		/// Gets or sets the SMILES.
		/// </summary>
		public string SMILES { get; set; }

		/// <summary>
		/// Gets or sets the ion description.
		/// </summary>
		public string Adduct { get; set; }

		/// <summary>
		/// Gets or sets the rank.
		/// </summary>
		public int Rank { get; set; }

		/// <summary>
		/// Gets or sets the CSI score.
		/// </summary>
		public double? CSIScore { get; set; }

		/// <summary>
		/// Gets or sets the Tanimoto similarity.
		/// </summary>
		public double? TanimotoSimilarity { get; set; }
	}
}
