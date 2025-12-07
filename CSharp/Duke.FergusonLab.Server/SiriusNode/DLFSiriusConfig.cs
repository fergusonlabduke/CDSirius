//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

namespace Duke.FergusonLab.Server.SiriusNode
{
	/// <summary>
	/// Holds all parameters for the SIRIUS search.
	/// </summary>
	internal class DLFSiriusConfig
	{
		#region SIRIUS Settings

		/// <summary>
		/// Gets or sets the main program path.
		/// </summary>
		public string ProgramPath { get; set; }

		/// <summary>
		/// Gets or sets the service port.
		/// </summary>
		public int ServicePort { get; set; }

		/// <summary>
		/// Gets or sets the account username.
		/// </summary>
		public string AccountUsername { get; set; }

		/// <summary>
		/// Gets or sets the account password.
		/// </summary>
		public string AccountPassword { get; set; }

		/// <summary>
		/// Gets or sets the unique project name.
		/// </summary>
		public string ProjectName { get; set; }

		/// <summary>
		/// Gets or sets the project space path.
		/// </summary>
		public string ProjectPath { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether SIRIUS project space should be saved.
		/// </summary>
		public bool SaveProject { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether SIRIUS molecular fingerprints should be saved.
		/// </summary>
		public bool SaveFingerprints { get; set; }

		/// <summary>
		/// Gets or sets the SIRIUS molecular fingerprints path.
		/// </summary>
		public string FingerprintsPath { get; set; }

		#endregion

		#region Search Settings

		/// <summary>
		/// Gets or sets the MS1 mass search tolerance in PPM.
		/// </summary>
		public double MS1MassTolerance { get; set; }

		/// <summary>
		/// Gets or sets the MS2 mass search tolerance in PPM.
		/// </summary>
		public double MS2MassTolerance { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether compound classes should be predicted by ClassyFire (CANOPUS).
		/// </summary>
		public bool PredictCompoundClasses { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether compound classes should be predicted by CSI:FingerID.
		/// </summary>
		public bool PredictStructures { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether compound classes should be predicted by MSNovelist.
		/// </summary>
		public bool PredictDeNovoStructures { get; set; }

		#endregion

		#region Formula Settings

		/// <summary>
		/// Gets or sets the maximum number of formula candidates predicted for each compound.
		/// </summary>
		public int FormulaMaxCandidates { get; set; }

		/// <summary>
		/// Gets or sets the elemental constrains.
		/// </summary>
		public string ElementalConstraints { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether isotopic pattern similarity should be checked.
		/// </summary>
		public bool CheckIsotopePattern { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether lipid formulas will be enforced when given MS/MS spectrum is recognized as lipid.
		/// </summary>
		public bool EnforceRecognizedLipids { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether molecular formulas should be generated via bottom-up search.
		/// </summary>
		public bool BottomUpSearch { get; set; }

		/// <summary>
		/// Gets or sets the m/z, below which de novo molecular formula generation is enabled.
		/// </summary>
		public double DeNovoMassThreshold { get; set; }

		#endregion

		#region Structure Settings

		/// <summary>
		/// Gets or sets the maximum number of structures predicted for each formula.
		/// </summary>
		public int StructuresMaxCandidates { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of de-novo structures predicted for each formula.
		/// </summary>
		public int DeNovoStructuresMaxCandidates { get; set; }

		/// <summary>
		/// Gets or sets the list of databases to search structures in.
		/// </summary>
		public string[] StructuresDatabases { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether PubChem should be used as a fallback database if no matches found in primary databases.
		/// </summary>
		public bool PubChemAsFallback { get; set; }

		#endregion
	}
}
