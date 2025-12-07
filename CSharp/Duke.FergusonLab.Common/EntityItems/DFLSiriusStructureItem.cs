//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

using Thermo.Magellan.EntityDataFramework;
using Thermo.Metabolism.DataObjects.Constants;

namespace Duke.FergusonLab.Common.EntityItems
{
	/// <summary>
	/// Unique molecular structures predicted by CSI:FingerID.
	/// </summary>
	[EntityExport("1B5897DB-5489-473F-9D2C-0ED3A5426D1C",
		EntityName = "DFLSiriusStructureItem",
		TableName = "DFLSiriusStructureItems",
		DisplayName = "SIRIUS Structures",
		Description = "Unique molecular structures predicted by CSI:FingerID",
		Visibility = GridVisibility.Visible,
		VisibilityStartingLayer = 1,
		VisiblePosition = 652)]

	[PredefinedEntityProperty(PredefinedEntityPropertyNames.Checkable)]
	[PredefinedEntityProperty(PredefinedEntityPropertyNames.Taggable)]

	public class DFLSiriusStructureItem : DFLSiriusStructureItemBase
	{ }

	/// <summary>
	/// Unique de-novo structures predicted by MSNovelist.
	/// </summary>
	[EntityExport("B55347A9-7347-4823-BC96-7AC46810322C",
		EntityName = "DFLSiriusDeNovoStructureItem",
		TableName = "DFLSiriusDeNovoStructureItems",
		DisplayName = "SIRIUS De-Novo Structures",
		Description = "Unique de-novo structures predicted by MSNovelist",
		Visibility = GridVisibility.Visible,
		VisibilityStartingLayer = 1,
		VisiblePosition = 653)]

	[PredefinedEntityProperty(PredefinedEntityPropertyNames.Checkable)]
	[PredefinedEntityProperty(PredefinedEntityPropertyNames.Taggable)]

	public class DFLSiriusDeNovoStructureItem : DFLSiriusStructureItemBase
	{ }

	/// <summary>
	/// Represents a base class defining shared properties of SIRIUS structure items.
	/// </summary>
	public abstract class DFLSiriusStructureItemBase : DynamicEntity
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
		/// Gets or sets the MOL structure string.
		/// </summary>
		[EntityProperty(
			DisplayName = "Structure",
			Description = "Molecular structure",
			DataPurpose = CDEntityDataPurpose.MolStructure,
			SortDirection = DataColumnSortDirection.Disabled)]
		[GridDisplayOptions(
			DataVisibility = GridVisibility.Visible,
			VisiblePosition = 100,
			ColumnWidth = 200,
			GridCellControlGuid = "6F0A4113-E338-454B-8E89-95C1BCE1380F")]
		[FilterOptions(Supported = PropertySupportType.No)]
		[ValueConverter(ValueConverterGuid = "BC434B48-7273-488B-8590-BB32AEB9712C")]
		public string MolStructure { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		[EntityProperty(
			DisplayName = "Name",
			Description = "Compound name",
			DataPurpose = CDEntityDataPurpose.CompoundName)]
		[GridDisplayOptions(
			VisiblePosition = 200,
			ColumnWidth = 300)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the elemental composition formula.
		/// </summary>		
		[EntityProperty(
			DisplayName = "Formula",
			Description = "Elemental composition formula",
			DataPurpose = CDEntityDataPurpose.ElementalCompositionFormula)]
		[GridDisplayOptions(
			VisiblePosition = 300,
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
			VisiblePosition = 400,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		public double MolecularWeight { get; set; }

		/// <summary>
		/// Gets or sets the octanol-water partition coefficient.
		/// </summary>		
		[EntityProperty(
			DisplayName = "Log Kow",
			Description = "Octanol-water partition coefficient",
			FormatString = "0.0",
			DataPurpose = DFLDataPurpose.LogKow)]
		[GridDisplayOptions(
			VisiblePosition = 500,
			TextHAlign = GridCellHAlign.Right)]
		[PlottingOptions(
			PlotType = PlotType.Numeric)]
		public double? LogKow { get; set; }

		/// <summary>
		/// Gets or sets the PubChem CID.
		/// </summary>
		[EntityProperty(
			DisplayName = "PubChem CID",
			Description = "PubChem CID",
			DataPurpose = CDEntityDataPurpose.PubChemId)]
		[GridDisplayOptions(
			VisiblePosition = 600,
			ColumnWidth = 100,
			TextHAlign = GridCellHAlign.Center,
			GridCellControlGuid = "C0699364-5E3A-41C9-9F27-690A41CC2385")]
		public string PubChemID { get; set; }

		/// <summary>
		/// Gets or sets the HMDB ID.
		/// </summary>
		[EntityProperty(
			DisplayName = "HMDB ID",
			Description = "HMDB ID",
			DataPurpose = CDEntityDataPurpose.HMDB)]
		[GridDisplayOptions(
			VisiblePosition = 610,
			ColumnWidth = 150,
			TextHAlign = GridCellHAlign.Center,
			GridCellControlGuid = "6C8927E9-54F0-4C6D-BA36-6B2DC172A961")]
		public string HmdbID { get; set; }

		/// <summary>
		/// Gets or sets the KEGG ID.
		/// </summary>
		[EntityProperty(
			DisplayName = "KEGG ID",
			Description = "KEGG ID",
			DataPurpose = CDEntityDataPurpose.KeggCompoundID)]
		[GridDisplayOptions(
			VisiblePosition = 620,
			ColumnWidth = 150,
			TextHAlign = GridCellHAlign.Center,
			GridCellControlGuid = "CA98CFAC-310A-4C09-8CC6-5D39AD44402A")]
		public string KeggID { get; set; }

		/// <summary>
		/// Gets or sets the DSSTox ID.
		/// </summary>
		[EntityProperty(
			DisplayName = "DSSTox ID",
			Description = "DSSTox ID",
			DataPurpose = DFLDataPurpose.DSSToxID)]
		[GridDisplayOptions(
			VisiblePosition = 630,
			ColumnWidth = 150,
			TextHAlign = GridCellHAlign.Center,
			GridCellControlGuid = "F37AF5A4-F3C5-4FCF-A00B-331CCEA7644B")]
		public string DSSToxID { get; set; }

		/// <summary>
		/// Gets or sets the InChI key.
		/// </summary>
		[EntityProperty(
			DisplayName = "InChIKey",
			Description = "Compacted version of InChI",
			DataPurpose = CDEntityDataPurpose.InChIKey)]
		[GridDisplayOptions(
			VisiblePosition = 700,
			ColumnWidth = 150,
			TextHAlign = GridCellHAlign.Center)]
		public string InChIKey { get; set; }

		/// <summary>
		/// Gets or sets the InChI string.
		/// </summary>
		[EntityProperty(
			DisplayName = "InChI",
			Description = "International Chemical Identifier",
			DataPurpose = CDEntityDataPurpose.InChI)]
		[GridDisplayOptions(
			VisiblePosition = 800,
			ColumnWidth = 250,
			TextHAlign = GridCellHAlign.Left)]
		public string InChI { get; set; }

		/// <summary>
		/// Gets or sets the SMILES string.
		/// </summary>
		[EntityProperty(
			DisplayName = "SMILES",
			Description = "SMILES string",
			DataPurpose = CDEntityDataPurpose.SMILES)]
		[GridDisplayOptions(
			VisiblePosition = 900,
			ColumnWidth = 250,
			TextHAlign = GridCellHAlign.Left)]
		public string SMILES { get; set; }

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
			return $"ID:{ID} {ElementalCompositionFormula}, MW:{MolecularWeight:F5}, Name:{Name}";
		}
	}
}