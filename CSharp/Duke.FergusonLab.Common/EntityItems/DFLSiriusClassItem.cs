//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

using Thermo.Magellan.EntityDataFramework;
using Thermo.Metabolism.DataObjects.Constants;

namespace Duke.FergusonLab.Common.EntityItems
{
	/// <summary>
	/// Unique compound classes predicted by ClassyFire (CANOPUS).
	/// </summary>

	[EntityExport("8F3FB8FA-ADB8-4D58-BCE1-1128F2990038",
		EntityName = "DFLSiriusClassItem",
		TableName = "DFLSiriusClassItems",
		DisplayName = "SIRIUS Classes",
		Description = "Unique compound classes predicted by ClassyFire (CANOPUS)",
		Visibility = GridVisibility.Visible,
		VisibilityStartingLayer = 1,
		VisiblePosition = 651)]

	[PredefinedEntityProperty(PredefinedEntityPropertyNames.Checkable)]
	[PredefinedEntityProperty(PredefinedEntityPropertyNames.Taggable)]

	public class DFLSiriusClassItem : DynamicEntity
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
		/// Gets or sets the classification level index.
		/// </summary>
		[EntityProperty(
			DisplayName = "Level Index",
			Description = "Classification level index",
			DataPurpose = DFLDataPurpose.ClassLevelIndex)]
		[GridDisplayOptions(
			VisiblePosition = 100,
			TextHAlign = GridCellHAlign.Right)]
		public int LevelIndex { get; set; }

		/// <summary>
		/// Gets or sets the classification level.
		/// </summary>
		[EntityProperty(
			DisplayName = "Level",
			Description = "Classification level",
			DataPurpose = DFLDataPurpose.ClassLevel)]
		[GridDisplayOptions(
			VisiblePosition = 200,
			ColumnWidth = 150)]
		public string Level { get; set; }

		/// <summary>
		/// Gets or sets the classification name.
		/// </summary>
		[EntityProperty(
			DisplayName = "Name",
			Description = "Classification name",
			DataPurpose = DFLDataPurpose.ClassName)]
		[GridDisplayOptions(
			VisiblePosition = 300,
			ColumnWidth = 200)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		[EntityProperty(
			DisplayName = "Description",
			Description = "Class description",
			DataPurpose = DFLDataPurpose.ClassDescription)]
		[GridDisplayOptions(
			VisiblePosition = 400,
			ColumnWidth = 600)]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the SIRIUS class ID.
		/// </summary>
		[EntityProperty(
			DisplayName = "ClassyFire ID",
			Description = "ClassyFire class ID",
			DataPurpose = DFLDataPurpose.ClassyFireID)]
		[GridDisplayOptions(
			VisiblePosition = 500,
			ColumnWidth = 100,
			TextHAlign = GridCellHAlign.Center,
			GridCellControlGuid = "266DBB0A-AA6D-4613-BAAB-61B4FDB6F491")]
		public int ClassyFireID { get; set; }

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
			return $"ID:{ID} Level:{Level} Name:{Name}";
		}
	}
}
