//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

namespace Duke.FergusonLab.Common
{
	/// <summary>
	/// Provides constants for the custom data types used by nodes.
	/// </summary>
	public class DFLDataTypes
	{
		/// <summary>
		/// Data type specification: SIRIUS formulas.
		/// </summary>
		public const string SiriusFormulas = "http://ferguson.cee.duke.edu/owl/DataTypes/Duke/FergusonLab/SiriusFormulas";

		/// <summary>
		/// Data type specification: SIRIUS classes by ClassyFire (CANOPUS).
		/// </summary>
		public const string SiriusClasses = "http://ferguson.cee.duke.edu/owl/DataTypes/Duke/FergusonLab/SiriusClasses";

		/// <summary>
		/// Data type specification: SIRIUS structures by CSI:FingerID.
		/// </summary>
		public const string SiriusStructures = "http://ferguson.cee.duke.edu/owl/DataTypes/Duke/FergusonLab/SiriusStructures";

		/// <summary>
		/// Data type specification: SIRIUS structures by MSNovelist.
		/// </summary>
		public const string SiriusDeNovoStructures = "http://ferguson.cee.duke.edu/owl/DataTypes/Duke/FergusonLab/SiriusDeNovoStructures";
	}

	/// <summary>
	/// Defines data purposes used for entity items.
	/// </summary>
	public class DFLDataPurpose
	{
		/// <summary>
		/// Data purpose: Adduct description.
		/// </summary>
		public const string Adduct = "ResultItemDataPurpose/Duke/FergusonLab/Adduct";

		/// <summary>
		/// Data purpose: Structure Log Kow coefficient.
		/// </summary>
		public const string LogKow = "ResultItemDataPurpose/Duke/FergusonLab/LogKow";

		/// <summary>
		/// Data purpose: Structure DSSTox ID.
		/// </summary>
		public const string DSSToxID = "ResultItemDataPurpose/Duke/FergusonLab/DSSToxID";

		/// <summary>
		/// Data purpose: Compound class tree ID.
		/// </summary>
		public const string ClassTreeID = "ResultItemDataPurpose/Duke/FergusonLab/ClassTreeID";

		/// <summary>
		/// Data purpose: Compound class level.
		/// </summary>
		public const string ClassLevel = "ResultItemDataPurpose/Duke/FergusonLab/ClassLevel";

		/// <summary>
		/// Data purpose: Compound class level index.
		/// </summary>
		public const string ClassLevelIndex = "ResultItemDataPurpose/Duke/FergusonLab/ClassLevelIndex";

		/// <summary>
		/// Data purpose: Compound class name.
		/// </summary>
		public const string ClassName = "ResultItemDataPurpose/Duke/FergusonLab/ClassName";

		/// <summary>
		/// Data purpose: Compound class description.
		/// </summary>
		public const string ClassDescription = "ResultItemDataPurpose/Duke/FergusonLab/ClassDescription";

		/// <summary>
		/// Data purpose: Compound ClassyFireID ID.
		/// </summary>
		public const string ClassyFireID = "ResultItemDataPurpose/Duke/FergusonLab/ClassyFireID";

		/// <summary>
		/// Data purpose: Compound class probability.
		/// </summary>
		public const string ClassProbability = "ResultItemDataPurpose/Duke/FergusonLab/ClassProbability";

		/// <summary>
		/// Data purpose: Formula SIRIUS score.
		/// </summary>
		public const string SiriusScore = "ResultItemDataPurpose/Duke/FergusonLab/SiriusScore";

		/// <summary>
		/// Data purpose: Formula tree score.
		/// </summary>
		public const string TreeScore = "ResultItemDataPurpose/Duke/FergusonLab/TreeScore";

		/// <summary>
		/// Data purpose: Formula isotope score.
		/// </summary>
		public const string IsotopeScore = "ResultItemDataPurpose/Duke/FergusonLab/IsotopeScore";

		/// <summary>
		/// Data purpose:MS2 median error in ppm
		/// </summary>
		public const string MS2MedianErrorInPPM = "ResultItemDataPurpose/Duke/FergusonLab/MS2MedianErrorInPPM";

		/// <summary>
		/// Data purpose: Number of explainable mass peaks in MS2.
		/// </summary>
		public const string ExplainablePeaksCount = "ResultItemDataPurpose/Duke/FergusonLab/ExplainablePeaksCount";

		/// <summary>
		/// Data purpose: Number of explained mass peaks in MS2.
		/// </summary>
		public const string ExplainedPeaksCount = "ResultItemDataPurpose/Duke/FergusonLab/ExplainedPeaksCount";

		/// <summary>
		/// Data purpose: Relative explained intensity in MS2.
		/// </summary>
		public const string ExplainedIntensity = "ResultItemDataPurpose/Duke/FergusonLab/ExplainedIntensity";

		/// <summary>
		/// Data purpose: Tanimoto similarity.
		/// </summary>
		public const string TanimotoSimilarity = "ResultItemDataPurpose/Duke/FergusonLab/TanimotoSimilarity";

		/// <summary>
		/// Data purpose: CSI score.
		/// </summary>
		public const string CSIScore = "ResultItemDataPurpose/Duke/FergusonLab/CSIScore";

		/// <summary>
		/// Data purpose: CSI FingerID exact confidence.
		/// </summary>
		public const string CSIConfidenceExact = "ResultItemDataPurpose/Duke/FergusonLab/CSIConfidenceExact";

		/// <summary>
		/// Data purpose: CSI FingerID approximate confidence.
		/// </summary>
		public const string CSIConfidenceApprox = "ResultItemDataPurpose/Duke/FergusonLab/CSIConfidenceApprox";

		/// <summary>
		/// Data purpose: ClassyFire level 1 class.
		/// </summary>
		public const string ClassyFireLevel1 = "ResultItemDataPurpose/Duke/FergusonLab/ClassyFireLevel1";

		/// <summary>
		/// Data purpose: ClassyFire level 2 class.
		/// </summary>
		public const string ClassyFireLevel2 = "ResultItemDataPurpose/Duke/FergusonLab/ClassyFireLevel2";

		/// <summary>
		/// Data purpose: ClassyFire level 3 class.
		/// </summary>
		public const string ClassyFireLevel3 = "ResultItemDataPurpose/Duke/FergusonLab/ClassyFireLevel3";

		/// <summary>
		/// Data purpose: ClassyFire level 4 class.
		/// </summary>
		public const string ClassyFireLevel4 = "ResultItemDataPurpose/Duke/FergusonLab/ClassyFireLevel4";

		/// <summary>
		/// Data purpose: ClassyFire level 5 class.
		/// </summary>
		public const string ClassyFireLevel5 = "ResultItemDataPurpose/Duke/FergusonLab/ClassyFireLevel5";

		/// <summary>
		/// Data purpose: ClassyFire level 6 class.
		/// </summary>
		public const string ClassyFireLevel6 = "ResultItemDataPurpose/Duke/FergusonLab/ClassyFireLevel6";
	}
}