//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

using Thermo.Magellan.EntityDataFramework;
using Thermo.Metabolism.DataObjects.Constants;

namespace Duke.FergusonLab.Common.EntityItems
{
	/// <summary>
	/// Formulas predicted by SIRIUS.
	/// </summary>

	[EntityExport("126D2357-9D5D-4D5E-ABB1-BFD9AD639F7D",
		EntityName = "DFLSiriusFormulaItem",
		TableName = "DFLSiriusFormulaItems",
		DisplayName = "SIRIUS Formulas",
		Description = "Formulas predicted by SIRIUS",
		Visibility = GridVisibility.Visible,
		VisibilityStartingLayer = 2,
		VisiblePosition = 650)]

	[PredefinedEntityProperty(PredefinedEntityPropertyNames.Checkable)]
	[PredefinedEntityProperty(PredefinedEntityPropertyNames.Taggable)]

	public class DFLSiriusFormulaItem : DynamicEntity
	{
		/// <summary>
		/// Gets or sets the ID.
		/// </summary>		
		[EntityProperty(
			DisplayName = "ID",
			DataPurpose = CDEntityDataPurpose.ID)]
		[EntityId(1)]
		public int ID { get; set; }

		/// <summary>
		/// Gets or sets the elemental composition formula.
		/// </summary>		
		[EntityProperty(
			DisplayName = "Formula",
			Description = "Elemental composition formula",
			DataPurpose = CDEntityDataPurpose.ElementalCompositionFormula)]
		[GridDisplayOptions(
			VisiblePosition = 100,
			ColumnWidth = 170)]
		[SortComparerOptions(DefaultSortComparers.ElementalComposition)]
		public string ElementalCompositionFormula { get; set; }

		/// <summary>
		/// Gets or sets the neutral molecular weight.
		/// </summary>		
		[EntityProperty(
			DisplayName = "Molecular Weight",
			Description = "Theoretical neutral mass in Da calculated from the formula",
			FormatString = "0.00000",
			DataPurpose = CDEntityDataPurpose.MolecularWeight)]
		[GridDisplayOptions(
			VisiblePosition = 200,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		public double MolecularWeight { get; set; }

		/// <summary>
		/// Gets or sets the delta mass in Da.
		/// </summary>
		[EntityProperty(
			DisplayName = "\x0394Mass [Da]",
			Description = "Difference between measured and theoretical mass in Da",
			FormatString = "0.00000",
			DataPurpose = CDEntityDataPurpose.DeltaMassInDa)]
		[GridDisplayOptions(
			VisiblePosition = 300,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		[SortComparerOptions(
			DefaultSortComparers.AbsoluteValuesFirst)]
		public double DeltaMassInDa { get; set; }

		/// <summary>
		/// Gets or sets the delta mass in ppm.
		/// </summary>
		[EntityProperty(
			DisplayName = "\x0394Mass [ppm]",
			Description = "Difference between measured and theoretical mass in ppm",
			FormatString = "0.00",
			DataPurpose = CDEntityDataPurpose.DeltaMassInPPM)]
		[GridDisplayOptions(
			VisiblePosition = 400,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		[SortComparerOptions(
			DefaultSortComparers.AbsoluteValuesFirst)]
		public double DeltaMassInPPM { get; set; }

		/// <summary>
		/// Gets or sets the rank.
		/// </summary>			
		[EntityProperty(
			DisplayName = "Rank",
			Description = "Candidate rank",
			DataPurpose = CDEntityDataPurpose.Rank)]
		[GridDisplayOptions(
			VisiblePosition = 500,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		public int Rank { get; set; }

		/// <summary>
		/// Gets or sets the SIRIUS score.
		/// </summary>		
		[EntityProperty(
			DisplayName = "SIRIUS Score",
			Description = "SIRIUS score",
			FormatString = "0.00",
			DataPurpose = DFLDataPurpose.SiriusScore,
			SortDirection = DataColumnSortDirection.Descending)]
		[GridDisplayOptions(
			VisiblePosition = 600,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		public double? SiriusScore { get; set; }

		/// <summary>
		/// Gets or sets the tree score.
		/// </summary>		
		[EntityProperty(
			DisplayName = "Tree Score",
			Description = "Tree score",
			FormatString = "0.00",
			DataPurpose = DFLDataPurpose.TreeScore,
			SortDirection = DataColumnSortDirection.Descending)]
		[GridDisplayOptions(
			VisiblePosition = 700,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		public double? TreeScore { get; set; }

		/// <summary>
		/// Gets or sets the isotope score.
		/// </summary>		
		[EntityProperty(
			DisplayName = "Isotope Score",
			Description = "Isotope score",
			FormatString = "0.00",
			DataPurpose = DFLDataPurpose.IsotopeScore,
			SortDirection = DataColumnSortDirection.Descending)]
		[GridDisplayOptions(
			VisiblePosition = 800,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		public double? IsotopeScore { get; set; }

		/// <summary>
		/// Gets or sets the SIRIUS score.
		/// </summary>		
		[EntityProperty(
			DisplayName = "MS2 Error [ppm]",
			Description = "MS2 median error in ppm",
			FormatString = "0.00",
			DataPurpose = DFLDataPurpose.MS2MedianErrorInPPM)]
		[GridDisplayOptions(
			VisiblePosition = 900,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		[SortComparerOptions(
			DefaultSortComparers.AbsoluteValuesFirst)]
		public double? MS2ErrorInPPM { get; set; }

		/// <summary>
		///	Gets or sets the number of explainable mass peaks in MS2 spectrum.
		/// </summary>				
		[EntityProperty(
			DisplayName = "# Explainable Peaks",
			Description = "Number of explainable mass peaks in MS2 spectrum",
			DataPurpose = DFLDataPurpose.ExplainablePeaksCount,
			SortDirection = DataColumnSortDirection.Descending)]
		[GridDisplayOptions(
			VisiblePosition = 1000,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.NumericAndOrdinal)]
		public int? ExplainablePeaksCount { get; set; }

		/// <summary>
		///	Gets or sets the number of explained mass peaks in MS2 spectrum.
		/// </summary>				
		[EntityProperty(
			DisplayName = "# Explained Peaks",
			Description = "Number of explained mass peaks in MS2 spectrum",
			DataPurpose = DFLDataPurpose.ExplainedPeaksCount,
			SortDirection = DataColumnSortDirection.Descending)]
		[GridDisplayOptions(
			VisiblePosition = 1100,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.NumericAndOrdinal)]
		public int? ExplainedPeaksCount { get; set; }

		/// <summary>
		///	Gets or sets the relative explained intensity in MS2 spectrum.
		/// </summary>				
		[EntityProperty(
			DisplayName = "# Explained Intensity",
			Description = "Relative explained intensity in MS2",
			FormatString = "0.00",
			DataPurpose = DFLDataPurpose.ExplainedIntensity,
			SortDirection = DataColumnSortDirection.Descending)]
		[GridDisplayOptions(
			VisiblePosition = 1200,
			TextHAlign = GridCellHAlign.Right)]
		public double? ExplainedIntensity { get; set; }

		/// <summary>
		/// Gets or sets the creation node number.
		/// </summary>		
		[EntityProperty(
			DisplayName = "Processing Node No",
			Description = "Processing node number as in workflow",
			DataPurpose = CDEntityDataPurpose.ProcessingNodeNumber)]
		[GridDisplayOptions(
			TextHAlign = GridCellHAlign.Center,
			DataVisibility = GridVisibility.Hidden)]
		[PlottingOptions(
			PlotType = PlotType.Venn)]
		public int IdentifyingNodeNumber { get; set; }

		/// <summary>
		/// Returns a string that represents this instance.
		/// </summary>
		public override string ToString()
		{
			return $"ID:{ID} {ElementalCompositionFormula}, MW:{MolecularWeight:F5}";
		}
	}
}
