//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Duke.FergusonLab.Server.SiriusNode
{
	/// <summary>
	/// Holds all information about individual feature imports.
	/// </summary>
	internal class DFLSiriusFeatureImport
	{
		/// <summary>
		/// Gets or sets the external item ID.
		/// </summary>
		public int ExternalID { get; set; }

		/// <summary>
		/// Gets or sets the neutral molecular weight.
		/// </summary>
		public double MW { get; set; }

		/// <summary>
		/// Gets or sets the ion m/z.
		/// </summary>
		public double Mass { get; set; }

		/// <summary>
		/// Gets or sets the ion charge.
		/// </summary>
		public int Charge { get; set; }

		/// <summary>
		/// Gets or sets the adduct description.
		/// </summary>
		public string Adduct { get; set; }

		/// <summary>
		/// Gets or sets the apex retention time.
		/// </summary>
		public double ApexRT { get; set; }

		/// <summary>
		/// Gets or sets the left retention time.
		/// </summary>
		public double LeftRT { get; set; }

		/// <summary>
		/// Gets or sets the right retention time.
		/// </summary>
		public double RightRT { get; set; }

		/// <summary>
		/// Gets or sets the MS1 mass spectrum.
		/// </summary>
		public DFLSiriusSpectrumImport MS1Spectrum { get; set; }

		/// <summary>
		/// Gets or sets the MS2 mass spectra.
		/// </summary>
		public List<DFLSiriusSpectrumImport> MS2Spectra { get; set; }
	}
}
