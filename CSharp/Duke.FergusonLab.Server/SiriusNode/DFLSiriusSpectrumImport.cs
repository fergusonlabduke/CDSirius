//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

namespace Duke.FergusonLab.Server.SiriusNode
{
	/// <summary>
	/// Holds all information about mass spectrum import.
	/// </summary>
	internal class DFLSiriusSpectrumImport
	{
		/// <summary>
		/// Gets or sets the spectrum name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the precursor neutral molecular weight.
		/// </summary>
		public double PrecursorMW { get; set; }

		/// <summary>
		/// Gets or sets the precursor m/z.
		/// </summary>
		public double PrecursorMass { get; set; }

		/// <summary>
		/// Gets or sets the precursor charge.
		/// </summary>
		public int PrecursorCharge { get; set; }

		/// <summary>
		/// Gets or sets the precursor adduct description.
		/// </summary>
		public string PrecursorAdduct { get; set; }

		/// <summary>
		/// Gets or sets the MS level.
		/// </summary>
		public int MSLevel { get; set; }

		/// <summary>
		/// Gets or sets the scan number.
		/// </summary>
		public int ScanNumber { get; set; }

		/// <summary>
		/// Gets or sets the collision energies.
		/// </summary>
		public double[] CollisionEnergies { get; set; }

		/// <summary>
		/// Gets or sets the centroids masses.
		/// </summary>
		public double[] Masses { get; set; }

		/// <summary>
		/// Gets or sets the centroids masses.
		/// </summary>
		public double[] Intensities { get; set; }
	}
}
