//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Thermo.Magellan.BL.Data;
using Thermo.Magellan.BL.Processing;
using Thermo.Magellan.BL.Processing.Interfaces;
using Thermo.Magellan.Core.Exceptions;
using Thermo.Magellan.EntityDataFramework;
using Thermo.Magellan.MassSpec;
using Thermo.Magellan.Processing.Parameters;
using Thermo.Magellan.Processing.Parameters.Attributes;
using Thermo.Magellan.Processing.Workflows;
using Thermo.Magellan.Processing.Workflows.Enums;
using Thermo.Magellan.Processing.Workflows.Legal;
using Thermo.Magellan.Utilities;
using Thermo.Metabolism.DataObjects;
using Thermo.Metabolism.DataObjects.Constants;
using Thermo.Metabolism.DataObjects.EntityDataObjects;
using Thermo.Metabolism.DomainObjects;
using Thermo.Metabolism.Services.MassFrontier;
using Duke.FergusonLab.Common;
using Duke.FergusonLab.Common.EntityItems;

namespace Duke.FergusonLab.Server.SiriusNode
{
	/// <summary>
	/// Runs SIRIUS tool to annotate compounds with formula, structure and compound class.
	/// </summary>

	#region Node Setup

	[PublisherInformation(Publisher = "ferguson.lee.duke.edu")]

	[ProcessingNode("A045B9B7-FF1D-422E-8742-8AB3F515FE3B",
		DisplayName = "Search by SIRIUS",
		Description = "Runs SIRIUS tool to annotate compounds with formula, structure and compound class.",
		Category = CDProcessingNodeCategories.CompoundIdentification,
		MainVersion = 1,
		MinorVersion = 1)]

	[ConnectionPoint(
		"IncomingCompounds",
		ConnectionDirection = ConnectionDirection.Incoming,
		ConnectionMultiplicity = ConnectionMultiplicity.Single,
		ConnectionMode = ConnectionMode.AutomaticToAllPossibleParents,
		ConnectionRequirement = ConnectionRequirement.RequiredAtDesignTime,
		ConnectionDataHandlingType = ConnectionDataHandlingType.FileBased,
		ConnectionDisplayName = "Incoming Compounds",
		ConnectedParentNodeConstraint = ConnectedParentConstraint.OnlyToGeneratorsOfRequestedData)]
	[ConnectionPointDataContract(
		"IncomingCompounds",
		MetabolismDataTypes.ConsolidatedUnknownCompounds)]

	[ConnectionPoint(
		"OutgoingResults",
		ConnectionDirection = ConnectionDirection.Outgoing,
		ConnectionMultiplicity = ConnectionMultiplicity.Multiple,
		ConnectionMode = ConnectionMode.Manual,
		ConnectionRequirement = ConnectionRequirement.Optional,
		ConnectionDataHandlingType = ConnectionDataHandlingType.FileBased)]
	[ConnectionPointDataContract(
		"OutgoingResults",
		DFLDataTypes.SiriusFormulas)]
	[ConnectionPointDataContract(
		"OutgoingResults",
		DFLDataTypes.SiriusClasses)]
	[ConnectionPointDataContract(
		"OutgoingResults",
		DFLDataTypes.SiriusStructures)]
	[ConnectionPointDataContract(
		"OutgoingResults",
		DFLDataTypes.SiriusDeNovoStructures)]

	[ConnectionPoint(
		"OutgoingAnnotations",
		ConnectionDirection = ConnectionDirection.Outgoing,
		ConnectionMultiplicity = ConnectionMultiplicity.Multiple,
		ConnectionMode = ConnectionMode.Manual,
		ConnectionRequirement = ConnectionRequirement.Optional,
		ConnectionDataHandlingType = ConnectionDataHandlingType.FileBased)]
	[ConnectionPointDataContract(
		"OutgoingAnnotations",
		MetabolismDataTypes.CompoundAnnotationProposals,
		DataTypeAttributes = new[] { MetabolismDataTypeAttributes.IdentificationProposal })]

	[ProcessingNodeAppearance(
		ImageSmallSource = "IMG_SIRIUS_16x16.png",
		ImageLargeSource = "IMG_SIRIUS_32x32.png")]

	[ProcessingNodeConstraints(UsageConstraint = UsageConstraint.OnlyOncePerWorkflow)]
	[LicenseFeature(Feature = "CompoundDiscoverer_Base", ShowIfNotAvailable = false)]

	#endregion

	public class DLFSiriusNode : ProcessingNode, IReprocessing
	{
		#region Private Members

		private DLFSiriusConfig m_siriusConfig;

		private Dictionary<int, object[]> m_compoundIDs;
		private Dictionary<int, double> m_compoundMWs;
		private Dictionary<string, object[]> m_formulaIDs;

		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopScoreAccessor;
		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopSimilarityAccessor;
		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopConfidenceExactAccessor;
		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopConfidenceApproxAccessor;

		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopClassLevel1Accessor;
		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopClassLevel2Accessor;
		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopClassLevel3Accessor;
		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopClassLevel4Accessor;
		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopClassLevel5Accessor;
		private PropertyAccessor<ConsolidatedUnknownCompoundItem> m_compoundTopClassLevel6Accessor;

		private string m_classesSortPropertyName;
		private string m_structuresSortPropertyName;
		private string m_deNovoStructuresSortPropertyName;

		#endregion

		#region Node Parameters

		private const string m_categoryGeneral = "1. General Settings";
		private const string m_categoryFormula = "2. Formula Prediction";
		private const string m_categoryClass = "3. Class Prediction";
		private const string m_categoryStructure = "4. Structure Prediction";
		private const string m_categorySirius = "5. SIRIUS Settings";
		private const string m_categoryReprocessing = "6. Reprocessing";

		#region General Settings

		/// <summary>
		/// This parameter specifies the mass accuracy tolerance for MS1 spectra.
		/// </summary>
		[MassToleranceParameter(
			Category = m_categoryGeneral,
			DisplayName = "MS1 Mass Tolerance",
			Description = "This parameter specifies the mass accuracy tolerance for MS1 spectra.",
			Subset = "ppm",
			DefaultValue = "2 ppm",
			MinimumValue = "0.1 ppm",
			MaximumValue = "20 ppm",
			ValueRequired = true,
			Position = 1)]
		public MassToleranceParameter MS1MassTolerance;

		/// <summary>
		/// This parameter specifies the mass accuracy tolerance for MS2 spectra.
		/// </summary>
		[MassToleranceParameter(
			Category = m_categoryGeneral,
			DisplayName = "MS2 Mass Tolerance",
			Description = "This parameter specifies the mass accuracy tolerance for MS2 spectra.",
			Subset = "ppm",
			DefaultValue = "5 ppm",
			MinimumValue = "0.1 ppm",
			MaximumValue = "20 ppm",
			ValueRequired = true,
			Position = 2)]
		public MassToleranceParameter MS2MassTolerance;

		/// <summary>
		/// This parameter specifies whether compound classes should be predicted by ClassyFire (CANOPUS).
		/// </summary>
		[BooleanParameter(
			Category = m_categoryGeneral,
			DisplayName = "Predict Compound Classes",
			Description = "This parameter specifies whether compound classes should be predicted by ClassyFire (CANOPUS).",
			DefaultValue = "true",
			Position = 3)]
		public BooleanParameter PredictCompoundClasses;

		/// <summary>
		/// This parameter specifies whether molecular structures should be predicted by CSI:FingerID.
		/// </summary>
		[BooleanParameter(
			Category = m_categoryGeneral,
			DisplayName = "Predict Structures",
			Description = "This parameter specifies whether molecular structures should be predicted by CSI:FingerID.",
			DefaultValue = "true",
			Position = 4)]
		public BooleanParameter PredictStructures;

		/// <summary>
		/// This parameter specifies whether de-novo structures should be predicted by MSNovelist.
		/// </summary>
		[BooleanParameter(
			Category = m_categoryGeneral,
			DisplayName = "Predict de-Novo Structures",
			Description = "This parameter specifies whether de-novo structures should be predicted by MSNovelist.",
			DefaultValue = "true",
			Position = 5)]
		public BooleanParameter PredictDeNovoStructures;

		/// <summary>
		/// This parameter specifies the maximum molecular weight of a compound to be considered.
		/// </summary>
		[DoubleParameter(
			Category = m_categoryGeneral,
			DisplayName = "Molecular Weight Threshold",
			Description = "This parameter specifies the maximum molecular weight of a compound to be considered",
			DefaultValue = "1500",
			MinimumValue = "0",
			MaximumValue = "2000",
			ValueRequired = true,
			Position = 6)]
		public DoubleParameter MolecularWeightThreshold;

		#endregion
		
		#region Formula Settings

		/// <summary>
		/// This parameter specifies the maximum number of formula candidates predicted for each compound.
		/// </summary>
		[IntegerParameter(
			Category = m_categoryFormula,
			DisplayName = "Max. Formula Candidates",
			Description = "This parameter specifies the maximum number of formula candidates predicted for each compound.",
			DefaultValue = "10",
			ValueRequired = true,
			Position = 1)]
		public IntegerParameter FormulaMaxCandidates;

		/// <summary>
		/// This parameter specifies the expected elements constraints.
		/// </summary>
		[ElementalCompositionParameter(
			Category = m_categoryFormula,
			DisplayName = "Elemental Constraints",
			Description = "This parameter specifies the expected elements constraints. " +
						  "Please note that B, Cl, Br, S, and Se are included automatically",
			DefaultValue = "C H N O P4 F40",
			Position = 2)]
		public ElementalCompositionParameter ElementalConstraints;

		/// <summary>
		/// This parameter specifies whether isotope pattern should be checked.
		/// </summary>
		[BooleanParameter(
			Category = m_categoryFormula,
			DisplayName = "Check Isotope Pattern",
			Description = "This parameter specifies whether molecular formulas should be excluded " +
			              "if their theoretical isotope pattern does not match the measured one, " +
						  "even if their MS/MS pattern has high score.",
			DefaultValue = "true",
			Position = 3)]
		public BooleanParameter CheckIsotopePattern;

		/// <summary>
		/// This parameter specifies whether recognized lipids should be enforced.
		/// </summary>
		[BooleanParameter(
			Category = m_categoryFormula,
			DisplayName = "Enforce Recognized Lipids",
			Description = "This parameter specifies whether lipid formulas will be enforced when given MS/MS spectrum is recognized as lipid.",
			DefaultValue = "true",
			Position = 4)]
		public BooleanParameter EnforceRecognizedLipids;

		/// <summary>
		/// This parameter specifies whether molecular formulas should be generated via bottom-up search.
		/// </summary>
		[BooleanParameter(
			Category = m_categoryFormula,
			DisplayName = "Bottom-Up Search",
			Description = "This parameter specifies whether molecular formulas should be generated via bottom-up search.",
			DefaultValue = "true",
			Position = 5)]
		public BooleanParameter BottomUpSearch;

		/// <summary>
		/// This parameter specifies the m/z, below which de novo molecular formula generation is enabled.
		/// </summary>
		[DoubleParameter(
			Category = m_categoryFormula,
			DisplayName = "De-Novo Mass Threshold",
			Description = "This parameter specifies the m/z, below which de novo molecular formula generation is enabled. If set to 0, de-novo molecular formula generation will be disabled.",
			DefaultValue = "400",
			MinimumValue = "0",
			MaximumValue = "1000",
			ValueRequired = true,
			Position = 6)]
		public DoubleParameter DeNovoMassThreshold;

		#endregion

		#region Class Settings

		/// <summary>
		/// This parameter specifies the class levels to be pushed to compounds table.
		/// </summary>
		[StringSelectionParameter(
			Category = m_categoryClass,
			DisplayName = "Classes to Push",
			Description = "This parameter specifies the class levels to be pushed to compounds table.",
			SelectionValues = new[] { "Level 1: Kingdom", "Level 2: Superclass", "Level 3: Class", "Level 4: Subclass", "Level 5", "Level 6" },
			DefaultValue = "Level 3: Class; Level 4: Subclass",
			ValueRequired = false,
			IsMultiSelect = true,
			Position = 1)]
		public SimpleSelectionParameter<string> ClassLevel;

		#endregion

		#region Structure Settings

		/// <summary>
		/// This parameter specifies the maximum number of structures to be predicted for each compound.
		/// </summary>
		[IntegerParameter(
			Category = m_categoryStructure,
			DisplayName = "Max. Structure Candidates",
			Description = "This parameter specifies the maximum number of structures to be predicted for each compound.",
			DefaultValue = "10",
			MinimumValue = "1",
			MaximumValue = "500",
			ValueRequired = true,
			Position = 1)]
		public IntegerParameter StructuresMaxCandidates;

		/// <summary>
		/// This parameter specifies the maximum number of de-novo structures to be predicted for each compound.
		/// </summary>
		[IntegerParameter(
			Category = m_categoryStructure,
			DisplayName = "Max. De-Novo Structure Candidates",
			Description = "This parameter specifies the maximum number of de-novo structures to be predicted for each compound.",
			DefaultValue = "10",
			MinimumValue = "1",
			MaximumValue = "500",
			ValueRequired = true,
			Position = 2)]
		public IntegerParameter DeNovoStructuresMaxCandidates;

		/// <summary>
		/// This parameter specifies the databases to be searched for structure candidates.
		/// </summary>
		[StringSelectionParameter(
			Category = m_categoryStructure,
			DisplayName = "Databases",
			Description = "This parameters specifies the databases to be searched for structure candidates.",
			SelectionValues = new[] { "PUBCHEM", "HMDB", "KEGG", "DSSTOX" },
			DefaultValue = "PUBCHEM; HMDB; KEGG; DSSTOX",
			ValueRequired = true,
			IsMultiSelect = true,
			Position = 3)]
		public SimpleSelectionParameter<string> StructuresDatabases;

		/// <summary>
		/// This parameter specifies whether PubChem should be used as a fallback database.
		/// </summary>
		[BooleanParameter(
			Category = m_categoryStructure,
			DisplayName = "PubChem as Fallback",
			Description = "This parameter specifies whether PubChem should be used as a fallback database if no matches found in primary databases. If False, then PubChem will be used as a primary database.",
			DefaultValue = "true",
			Position = 4)]
		public BooleanParameter PubChemAsFallback;

		#endregion

		#region SIRIUS Settings

		/// <summary>
		/// This parameter specifies whether to save the SIRIUS project space output.
		/// </summary>
		[BooleanParameter(
			Category = m_categorySirius,
			DisplayName = "Keep Project Space",
			Description = "This parameter specifies whether to save the SIRIUS project space output for use with Sirius GUI. " +
			              "If set to True, project space file will be saved in the same directory as the corresponding cdResult file.",
			DefaultValue = "false",
			Position = 1)]
		public BooleanParameter SiriusSaveProject;

		/// <summary>
		/// This parameter specifies whether to save the SIRIUS project space output.
		/// </summary>
		[BooleanParameter(
			Category = m_categorySirius,
			DisplayName = "Keep Fingerprints",
			Description = "This parameter specifies whether to save the SIRIUS molecular fingerprints predictions. " +
			              "If True, files containing (1) the molecular fingerprint definition key and (2) the table of predicted molecular fingerprints " +
			              "for all compounds processed by SIRIUS is saved in the same directory as the corresponding cdResult file",
			DefaultValue = "false",
			Position = 2)]
		public BooleanParameter SiriusSaveFingerprints;

		#endregion

		#region Reprocessing

		/// <summary>
		/// This parameter specifies whether checked compounds only are considered during reprocessing.
		/// </summary>
		[BooleanParameter(
			Category = m_categoryReprocessing,
			DisplayName = "Checked Only",
			Description = "This parameter specifies whether checked compounds only are considered as during reprocessing. " +
			              "This settings has no effect during normal processing and all compounds will be considered.",
			DefaultValue = "false",
			Position = 1)]
		public BooleanParameter CheckedCompoundsOnly;

		#endregion

		#endregion

		/// <summary>
		/// Gets or sets a value indicating whether this instance is re-processable.
		/// </summary>
		public bool IsReprocessing { get; set; }
		
		/// <summary>
		/// Notification that all processing of a parent node is finished and all results are stored or sent.
		/// </summary>
		public override void OnAllParentNodesFinished()
		{
			try
			{
				SendAndLogTemporaryMessage("⇒ {0} started...", DisplayName);
				var timer = Stopwatch.StartNew();

				// register entities as needed
				RegisterSiriusFormulas();
				RegisterSiriusClasses();
				RegisterSiriusStructures<DFLSiriusStructureItem>(PredictStructures.Value, ref m_structuresSortPropertyName);
				RegisterSiriusStructures<DFLSiriusDeNovoStructureItem>(PredictDeNovoStructures.Value, ref m_deNovoStructuresSortPropertyName);
				RegisterTopAnnotationsScores();
				RegisterTopAnnotationsClasses(ClassLevel.Values);

				// start main processing
				ProcessCompounds();

				// finalize results database
				SetInitialSorting();

				timer.Stop();
				SendAndLogMessage("✓ {0} finished after {1}.", DisplayName, StringHelper.GetDisplayString(timer.Elapsed));
			}
			catch (Exception ex)
			{
				SendAndLogErrorMessage("Error: " + ex.Message);
				throw;
			}

			// inform child nodes that processing has finished
			var dataTypeNames = new List<string> { DFLDataTypes.SiriusFormulas };
			if (PredictCompoundClasses.Value)
			{
				dataTypeNames.Add(DFLDataTypes.SiriusClasses);
			}
			if (PredictStructures.Value)
			{
				dataTypeNames.Add(DFLDataTypes.SiriusStructures);
			}
			if (PredictDeNovoStructures.Value)
			{
				dataTypeNames.Add(DFLDataTypes.SiriusDeNovoStructures);
			}
			FireProcessingFinishedEvent(new ResultsArguments(dataTypeNames.ToArray()));
		}

		/// <summary>
		/// Registers tables and properties for SIRIUS formulas.
		/// </summary>
		private void RegisterSiriusFormulas()
		{
			// register table
			EntityDataService.RegisterEntity<DFLSiriusFormulaItem>(ProcessingNodeNumber);
			EntityDataService.RegisterEntityConnection<ConsolidatedUnknownCompoundItem, DFLSiriusFormulaItem>(ProcessingNodeNumber);
		}

		/// <summary>
		/// Registers tables and properties for SIRIUS classes.
		/// </summary>
		private void RegisterSiriusClasses()
		{
			// check if needed
			if (PredictCompoundClasses.Value == false)
			{
				return;
			}

			// register table
			EntityDataService.RegisterEntity<DFLSiriusClassItem>(ProcessingNodeNumber);

			// init connection properties
			var position = 98;

			var probability = PropertyAccessorFactory.CreateConnectionPropertyAccessor<double>(
				new PropertyDescription
				{
					DisplayName = "Probability",
					Description = "Candidate probability",
					FormatString = "0.000",
					DataPurpose = DFLDataPurpose.ClassProbability
				},
				"Probability");
			probability.GridDisplayOptions.VisiblePosition = position++;
			probability.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			probability.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };

			var treeID = PropertyAccessorFactory.CreateConnectionPropertyAccessor<int>(
				new PropertyDescription
				{
					DisplayName = "Tree ID",
					Description = "Class tree ID",
					DataPurpose = DFLDataPurpose.ClassTreeID
				},
				"TreeID");
			treeID.GridDisplayOptions.VisiblePosition = position++;
			treeID.GridDisplayOptions.DataVisibility = GridVisibility.Hidden;
			treeID.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			treeID.EntityId = new EntityIdOptions(1);
			m_classesSortPropertyName = treeID.Name;

			// register connections
			EntityDataService.RegisterEntityConnection<ConsolidatedUnknownCompoundItem, DFLSiriusClassItem>(ProcessingNodeNumber, treeID);
			EntityDataService.RegisterEntityConnection<DFLSiriusFormulaItem, DFLSiriusClassItem>(ProcessingNodeNumber, treeID);

			// register connection properties
			EntityDataService.RegisterConnectionProperties<ConsolidatedUnknownCompoundItem, DFLSiriusClassItem>(
				ProcessingNodeNumber,
				probability
			);

			EntityDataService.RegisterConnectionProperties<DFLSiriusFormulaItem, DFLSiriusClassItem>(
				ProcessingNodeNumber,
				probability
			);
		}

		/// <summary>
		/// Registers tables and properties for SIRIUS structures.
		/// </summary>
		private void RegisterSiriusStructures<TItem>(bool enabled, ref string sortColumnName)
			where TItem : DynamicEntity
		{
			// check if needed
			if (enabled == false)
			{
				return;
			}

			// register table
			EntityDataService.RegisterEntity<TItem>(ProcessingNodeNumber);
			EntityDataService.RegisterEntityConnection<ConsolidatedUnknownCompoundItem, TItem>(ProcessingNodeNumber);
			EntityDataService.RegisterEntityConnection<DFLSiriusFormulaItem, TItem>(ProcessingNodeNumber);

			// init connection properties
			var position = 401;

			var deltaMassInDa = PropertyAccessorFactory.CreateConnectionPropertyAccessor<double>(
				new PropertyDescription
				{
					DisplayName = "\x0394Mass [Da]",
					Description = "Difference between measured and theoretical mass in Da",
					FormatString = "0.00000",
					DataPurpose = CDEntityDataPurpose.DeltaMassInDa
				},
				"DeltaMassInDa");
			deltaMassInDa.GridDisplayOptions.VisiblePosition = position++;
			deltaMassInDa.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			deltaMassInDa.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };
			deltaMassInDa.SetDefaultSortComparer(DefaultSortComparers.AbsoluteValuesFirst);

			var deltaMassInPpm = PropertyAccessorFactory.CreateConnectionPropertyAccessor<double>(
				new PropertyDescription
				{
					DisplayName = "\x0394Mass [ppm]",
					Description = "Difference between measured and theoretical mass in ppm",
					FormatString = "0.00",
					DataPurpose = CDEntityDataPurpose.DeltaMassInPPM
				},
				"DeltaMassInPpm");
			deltaMassInPpm.GridDisplayOptions.VisiblePosition = position++;
			deltaMassInPpm.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			deltaMassInPpm.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };
			deltaMassInPpm.SetDefaultSortComparer(DefaultSortComparers.AbsoluteValuesFirst);

			var rank = PropertyAccessorFactory.CreateConnectionPropertyAccessor<int>(
				new PropertyDescription
				{
					DisplayName = "CSI Rank",
					Description = "Candidate rank",
					DataPurpose = CDEntityDataPurpose.Rank
				},
				"CSIRank");
			rank.GridDisplayOptions.VisiblePosition = position++;
			rank.GridDisplayOptions.TextHAlign = GridCellHAlign.Center;
			rank.PlottingOptions = new PlottingOptions { PlotType = PlotType.NumericAndOrdinal };

			var sciScore = PropertyAccessorFactory.CreateConnectionPropertyAccessor<double?>(
				new PropertyDescription
				{
					DisplayName = "CSI Score",
					Description = "CSI score",
					FormatString = "0.00",
					DataPurpose = DFLDataPurpose.CSIScore,
					SortDirection = DataColumnSortDirection.Descending
				},
				"CSIScore");
			sciScore.GridDisplayOptions.VisiblePosition = position++;
			sciScore.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			sciScore.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };

			var tanimotoSim = PropertyAccessorFactory.CreateConnectionPropertyAccessor<double?>(
				new PropertyDescription
				{
					DisplayName = "Tanimoto Sim.",
					Description = "Tanimoto similarity",
					FormatString = "0.00",
					DataPurpose = DFLDataPurpose.TanimotoSimilarity,
					SortDirection = DataColumnSortDirection.Descending
				},
				"TanimotoSimilarity");
			tanimotoSim.GridDisplayOptions.VisiblePosition = position++;
			tanimotoSim.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			tanimotoSim.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };

			// register connection properties
			EntityDataService.RegisterConnectionProperties<ConsolidatedUnknownCompoundItem, TItem>(
				ProcessingNodeNumber,
				deltaMassInDa,
				deltaMassInPpm,
				rank,
				sciScore,
				tanimotoSim
			);

			// define databases
			var databases = new Dictionary<string, string>
			{
				{ "PUBCHEM", CDEntityDataPurpose.PubChemId },
				{ "HMDB", CDEntityDataPurpose.HMDB },
				{ "KEGG", CDEntityDataPurpose.KeggCompoundID },
				{ "DSSTOX", DFLDataPurpose.DSSToxID }
			};

			// hide database ID columns if not selected
			foreach (var (name, purpose) in databases)
			{
				if (StructuresDatabases.Values.Contains(name) == false)
				{
					var accessor = EntityDataService.GetProperties<TItem>(purpose).FirstOrDefault();
					if (accessor != null)
					{
						accessor.GridDisplayOptions.DataVisibility = GridVisibility.Hidden;
						EntityDataService.UpdateProperties(accessor);
					}
				}
			}

			// set rank as sort column
			sortColumnName = rank.Name;
		}

		/// <summary>
		/// Registers additional compound columns for top annotation scores.
		/// </summary>
		private void RegisterTopAnnotationsScores()
		{
			// check if needed
			if (PredictStructures.Value == false)
			{
				return;
			}

			// init position
			var maxAreaAccessor = EntityDataService.GetProperties<ConsolidatedUnknownCompoundItem>(CDEntityDataPurpose.AreaSumMax).FirstOrDefault();
			var position = maxAreaAccessor?.GridDisplayOptions.VisiblePosition ?? 400;

			// create CSI FingerID score
			m_compoundTopScoreAccessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, double?>(
				new PropertyDescription
				{
					DisplayName = "Top CSI Score",
					Description = "Top CSI FingerID Score",
					DataPurpose = DFLDataPurpose.CSIScore,
					FormatString = "0.00"
				}, "SiriusTopScore");

			m_compoundTopScoreAccessor.GridDisplayOptions.DataVisibility = GridVisibility.Visible;
			m_compoundTopScoreAccessor.GridDisplayOptions.VisiblePosition = ++position;
			m_compoundTopScoreAccessor.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			m_compoundTopScoreAccessor.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };

			// create CSI FingerID Tanimoto similarity
			m_compoundTopSimilarityAccessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, double?>(
				new PropertyDescription
				{
					DisplayName = "Top CSI Tanimoto Sim.",
					Description = "Top CSI FingerID Tanimoto similarity",
					DataPurpose = DFLDataPurpose.TanimotoSimilarity,
					FormatString = "0.00"
				}, "SiriusTopSimilarity");

			m_compoundTopSimilarityAccessor.GridDisplayOptions.DataVisibility = GridVisibility.Visible;
			m_compoundTopSimilarityAccessor.GridDisplayOptions.VisiblePosition = ++position;
			m_compoundTopSimilarityAccessor.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			m_compoundTopSimilarityAccessor.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };

			// create CSI FingerID exact confidence
			m_compoundTopConfidenceExactAccessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, double?>(
				new PropertyDescription
				{
					DisplayName = "Top CSI Exact Confidence",
					Description = "Top CSI FingerID exact confidence",
					DataPurpose = DFLDataPurpose.CSIConfidenceExact,
					FormatString = "0.00"
				}, "SiriusTopConfidenceExact");

			m_compoundTopConfidenceExactAccessor.GridDisplayOptions.DataVisibility = GridVisibility.Visible;
			m_compoundTopConfidenceExactAccessor.GridDisplayOptions.VisiblePosition = ++position;
			m_compoundTopConfidenceExactAccessor.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			m_compoundTopConfidenceExactAccessor.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };

			// create CSI FingerID approximate confidence
			m_compoundTopConfidenceApproxAccessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, double?>(
				new PropertyDescription
				{
					DisplayName = "Top CSI Approx. Confidence",
					Description = "Top CSI FingerID approximate confidence",
					DataPurpose = DFLDataPurpose.CSIConfidenceApprox,
					FormatString = "0.00"
				}, "SiriusTopConfidenceApprox");

			m_compoundTopConfidenceApproxAccessor.GridDisplayOptions.DataVisibility = GridVisibility.Visible;
			m_compoundTopConfidenceApproxAccessor.GridDisplayOptions.VisiblePosition = ++position;
			m_compoundTopConfidenceApproxAccessor.GridDisplayOptions.TextHAlign = GridCellHAlign.Right;
			m_compoundTopConfidenceApproxAccessor.PlottingOptions = new PlottingOptions { PlotType = PlotType.Numeric };

			// register properties
			EntityDataService.RegisterProperties(
				ProcessingNodeNumber,
				m_compoundTopScoreAccessor,
				m_compoundTopSimilarityAccessor,
				m_compoundTopConfidenceExactAccessor,
				m_compoundTopConfidenceApproxAccessor);
		}

		/// <summary>
		/// Registers additional compound columns for top annotation classes.
		/// </summary>
		private void RegisterTopAnnotationsClasses(string[] levels)
		{
			// check if needed
			if (PredictCompoundClasses.Value == false)
			{
				return;
			}
			
			// init position
			var position = m_compoundTopConfidenceApproxAccessor.GridDisplayOptions.VisiblePosition;

			// init accessors
			var accessors = new List<PropertyAccessor<ConsolidatedUnknownCompoundItem>>();
			
			if (levels.Contains("Level 1: Kingdom"))
			{
				m_compoundTopClassLevel1Accessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, string>(
					new PropertyDescription
					{
						DisplayName = "Top ClassyFire Kingdom",
						Description = "Top ClassyFire compound kingdom",
						DataPurpose = DFLDataPurpose.ClassyFireLevel1,
					}, "SiriusTopClassyFireLevel1");

				accessors.Add(m_compoundTopClassLevel1Accessor);
			}

			if (levels.Contains("Level 2: Superclass"))
			{
				m_compoundTopClassLevel2Accessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, string>(
					new PropertyDescription
					{
						DisplayName = "Top ClassyFire Superclass",
						Description = "Top ClassyFire compound superclass",
						DataPurpose = DFLDataPurpose.ClassyFireLevel2,
					}, "SiriusTopClassyFireLevel2");

				accessors.Add(m_compoundTopClassLevel2Accessor);
			}

			if (levels.Contains("Level 3: Class"))
			{
				m_compoundTopClassLevel3Accessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, string>(
					new PropertyDescription
					{
						DisplayName = "Top ClassyFire Class",
						Description = "Top ClassyFire compound class",
						DataPurpose = DFLDataPurpose.ClassyFireLevel3,
					}, "SiriusTopClassyFireLevel3");

				accessors.Add(m_compoundTopClassLevel3Accessor);
			}

			if (levels.Contains("Level 4: Subclass"))
			{
				m_compoundTopClassLevel4Accessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, string>(
					new PropertyDescription
					{
						DisplayName = "Top ClassyFire Subclass",
						Description = "Top ClassyFire compound subclass",
						DataPurpose = DFLDataPurpose.ClassyFireLevel4,
					}, "SiriusTopClassyFireLevel4");

				accessors.Add(m_compoundTopClassLevel4Accessor);
			}

			if (levels.Contains("Level 5"))
			{
				m_compoundTopClassLevel5Accessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, string>(
					new PropertyDescription
					{
						DisplayName = "Top ClassyFire Level 5",
						Description = "Top ClassyFire compound level 5",
						DataPurpose = DFLDataPurpose.ClassyFireLevel5,
					}, "SiriusTopClassyFireLevel5");

				accessors.Add(m_compoundTopClassLevel5Accessor);
			}

			if (levels.Contains("Level 6"))
			{
				m_compoundTopClassLevel6Accessor = PropertyAccessorFactory.CreateDynamicPropertyAccessor<ConsolidatedUnknownCompoundItem, string>(
					new PropertyDescription
					{
						DisplayName = "Top ClassyFire Level 6",
						Description = "Top ClassyFire compound level 6",
						DataPurpose = DFLDataPurpose.ClassyFireLevel6,
					}, "SiriusTopClassyFireLevel6");

				accessors.Add(m_compoundTopClassLevel6Accessor);
			}

			// finalize properties
			foreach (var accessor in accessors)
			{
				accessor.GridDisplayOptions.DataVisibility = GridVisibility.Visible;
				accessor.GridDisplayOptions.VisiblePosition = ++position;
				accessor.GridDisplayOptions.ColumnWidth = 200;
				accessor.GridDisplayOptions.TextHAlign = GridCellHAlign.Left;
				accessor.PlottingOptions = new PlottingOptions { PlotType = PlotType.Ordinal };
			}

			// register properties
			EntityDataService.RegisterProperties(ProcessingNodeNumber, accessors.ToArray());
		}

		/// <summary>
		/// Sets default sort conditions.
		/// </summary>
		private void SetInitialSorting()
		{
			// get service
			var settingsService = ProcessingServices.Get<ReportFileSettingsPersistenceService>(true);
			var compoundEntity = EntityDataService.GetEntity<ConsolidatedUnknownCompoundItem>();
			EntityType resultEntity;

			// sort formulas
			settingsService.AddSortCondition<DFLSiriusFormulaItem>(EntityDataService, i => i.Rank);

			// sort classes
			if (m_classesSortPropertyName.IsNullOrEmpty() == false)
			{
				resultEntity = EntityDataService.GetEntity<DFLSiriusClassItem>();
				settingsService.IntegrateNestedSortConditionSet(
					SortConditionHelper.CreateNestedSortConditionSet(
						new[] { compoundEntity.Name, resultEntity.Name },
						new SortCondition(
							ReportingHelper.CreateConnectionPropertyName(m_classesSortPropertyName, compoundEntity.Name),
							ListSortDirection.Ascending)));
			}

			// sort structures
			if (m_structuresSortPropertyName.IsNullOrEmpty() == false)
			{
				resultEntity = EntityDataService.GetEntity<DFLSiriusStructureItem>();
				settingsService.IntegrateNestedSortConditionSet(
					SortConditionHelper.CreateNestedSortConditionSet(
						new[] { compoundEntity.Name, resultEntity.Name },
						new SortCondition(
							ReportingHelper.CreateConnectionPropertyName(m_structuresSortPropertyName, compoundEntity.Name),
							ListSortDirection.Ascending)));
			}

			// sort de-novo structures
			if (m_deNovoStructuresSortPropertyName.IsNullOrEmpty() == false)
			{
				resultEntity = EntityDataService.GetEntity<DFLSiriusDeNovoStructureItem>();
				settingsService.IntegrateNestedSortConditionSet(
					SortConditionHelper.CreateNestedSortConditionSet(
						new[] { compoundEntity.Name, resultEntity.Name },
						new SortCondition(
							ReportingHelper.CreateConnectionPropertyName(m_deNovoStructuresSortPropertyName, compoundEntity.Name),
							ListSortDirection.Ascending)));
			}
		}

		/// <summary>
		/// Main processing part.
		/// </summary>
		private void ProcessCompounds()
		{
			// get scratch folder
			var scratchDir = NodeScratchDirectory;

			// export main params
			WriteSiriusConfig(scratchDir);

			// export compounds info and spectra
			WriteCompoundsData(scratchDir);

			// execute tool
			var exitCode = ExecuteScript(scratchDir);
			if (exitCode != 0)
			{
				SendAndLogErrorMessage($"Failed to execute SIRIUS script: exitCode={exitCode}");
				throw new MagellanProcessingException("SIRIUS error.");
			}

			// persist formulas
			LoadAndPersistFormulas(scratchDir);

			// persist classes
			LoadAndPersistClasses(scratchDir);

			// persist structures
			LoadAndPersistStructures<DFLSiriusStructureItem>(scratchDir, PredictStructures.Value, "sirius_structures", "structure");

			// persist de-novo structures
			LoadAndPersistStructures<DFLSiriusDeNovoStructureItem>(scratchDir, PredictDeNovoStructures.Value, "sirius_denovo_structures", "de-novo structure");

			// persist top annotations
			LoadAndPersistTopAnnotations(scratchDir);
		}

		/// <summary>
		/// Prepares and writes SIRIUS config to JSON file.
		/// </summary>
		private void WriteSiriusConfig(string outputDir)
		{
			SendAndLogTemporaryMessage("Writing SIRIUS config...");

			// init config
			m_siriusConfig = new DLFSiriusConfig
			{
				SaveProject = SiriusSaveProject.Value,
				SaveFingerprints = SiriusSaveFingerprints.Value,

				MS1MassTolerance = MS1MassTolerance.Value.Tolerance,
				MS2MassTolerance = MS2MassTolerance.Value.Tolerance,
				PredictCompoundClasses = PredictCompoundClasses.Value,
				PredictStructures = PredictStructures.Value,
				PredictDeNovoStructures = PredictDeNovoStructures.Value,

				FormulaMaxCandidates = FormulaMaxCandidates.Value,
				ElementalConstraints = ElementalConstraints.Value,
				CheckIsotopePattern = CheckIsotopePattern.Value,
				EnforceRecognizedLipids = EnforceRecognizedLipids.Value,
				BottomUpSearch = BottomUpSearch.Value,
				DeNovoMassThreshold = DeNovoMassThreshold.Value,

				StructuresMaxCandidates = StructuresMaxCandidates.Value,
				DeNovoStructuresMaxCandidates = DeNovoStructuresMaxCandidates.Value,
				StructuresDatabases = StructuresDatabases.Values,
				PubChemAsFallback = PubChemAsFallback.Value,
			};

			// get result name and location
			var resultPath = EntityDataService.ReportFile.FileName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			var resultDir = Path.GetDirectoryName(resultPath);

			// init project name
			var resultFileName = Path.GetFileNameWithoutExtension(resultPath);
			var projectName = Regex.Replace(resultFileName, @"[^a-zA-Z0-9_-]", "");
			m_siriusConfig.ProjectName = projectName;

			// init project path
			var projectDir = SiriusSaveProject.Value ? resultDir : outputDir;
			var projectPath = Path.Combine(projectDir, $"{projectName}.sirius");
			m_siriusConfig.ProjectPath = projectPath;

			// init fingerprint export path
			m_siriusConfig.FingerprintsPath = Path.Combine(resultDir, resultFileName);

			// write config to JSON
			var configPath = Path.Combine(outputDir, "sirius_config.json");
			using var writer = File.CreateText(configPath);
			var jsonSerializer = new JsonSerializer();
			jsonSerializer.Serialize(writer, m_siriusConfig);
		}

		/// <summary>
		/// Prepares and writes compounds data to search.
		/// </summary>
		private void WriteCompoundsData(string outputDir)
		{
			// init progress
			SendAndLogTemporaryMessage("Writing SIRIUS features import...");
			var timer = Stopwatch.StartNew();

			// init lookups
			m_compoundIDs = new Dictionary<int, object[]>();
			m_compoundMWs = new Dictionary<int, double>();
			var compoundTmpID = 1;

			// init adducts
			var mZero = AbstractIon.CreateByProton(0);
			var mhPlus = AbstractIon.CreateByProton(1);
			var mhMinus = AbstractIon.CreateByProton(-1);

			// get check-state accessor
			var checkedOnly = CheckedCompoundsOnly.Value && ProcessingServices.CurrentWorkflow.GetWorkflow().IsReprocessing;
			var checkedAccessor = checkedOnly
				? EntityDataService.GetProperties<ConsolidatedUnknownCompoundItem>(CDEntityDataPurpose.CheckState).FirstOrDefault()
				: null;

			// init compound reader
			var compoundEntityType = EntityDataService.GetEntity<ConsolidatedUnknownCompoundItem>();
			var compoundMWName = nameof(ConsolidatedUnknownCompoundItem.MolecularWeight);
			var compoundReaderSettings = checkedAccessor != null
				? new ReaderSettings { EntityName = compoundEntityType.Name, PropertyNames = new[] { compoundMWName, checkedAccessor.Name } }
				: new ReaderSettings { EntityName = compoundEntityType.Name, PropertyNames = new[] { compoundMWName } };

			// init readers
			var compoundReader = EntityDataService.CreateEntityItemReader();
			var spectraReader = EntityDataService.CreateEntityItemReader();

			// init JSON writer
			var exportPath = Path.Combine(outputDir, "sirius_features.json");
			using var fileStream = new FileStream(exportPath, FileMode.Create);
			using var streamWriter = new StreamWriter(fileStream);
			using var jsonWriter = new JsonTextWriter(streamWriter);
			var jsonSerializer = new JsonSerializer();

			// init JSON
			jsonWriter.WriteStartArrayAsync();

			// read compound annotations
			foreach (var compound in compoundReader.ReadAllHierarchical<ConsolidatedUnknownCompoundItem, BestHitIonInstanceItem, MassSpectrumInfoItem>(readerSettingsT1: compoundReaderSettings))
			{
				JobCancellationToken.ThrowIfCancellationRequested();

				// use checked only
				if (checkedOnly
					&& checkedAccessor != null
					&& (bool)checkedAccessor.GetValue(compound.Item1) == false)
				{
					continue;
				}

				// check molecular weight threshold
				if (compound.Item1.MolecularWeight > MolecularWeightThreshold.Value)
				{
					continue;
				}

				// get best hit
				var bestHitMS1 = compound.Item2.FirstOrDefault(f => f.Item1.BestHitType == BestHitType.BestMS1);
				var bestHitMS2 = compound.Item2.FirstOrDefault(f => f.Item1.BestHitType == BestHitType.BestMS2);

				// skip if no spectrum data
				if (bestHitMS1 == null || bestHitMS2 == null)
				{
					continue;
				}

				// update compound lookups
				m_compoundIDs.Add(compoundTmpID, compound.Item1.GetIDs());
				m_compoundMWs.Add(compoundTmpID, compound.Item1.MolecularWeight);

				// get MS1 and MS2 spectra
				var spectra = spectraReader
					.ReadMany<MassSpectrumItem>(
						compound.Item2
							.SelectMany(sm => sm.Item2)
							.Where(w => (int)w.MSOrder <= 2)
							.Select(s => s.GetIDs()))
					.DistinctBy(d => d.ID)
					.ToDictionary(k => k.ID, v => v);

				// init feature import
				var feature = new DFLSiriusFeatureImport()
				{
					ExternalID = compoundTmpID,
					MW = bestHitMS1.Item1.MolecularWeight,
					Mass = bestHitMS1.Item1.Mass,
					Charge = bestHitMS1.Item1.Charge,
					Adduct = bestHitMS1.Item1.IonDescription,
					ApexRT = bestHitMS1.Item1.RetentionTime,
					LeftRT = bestHitMS1.Item1.RetentionTime - 0.02,
					RightRT = bestHitMS1.Item1.RetentionTime + 0.02,
					MS1Spectrum = new DFLSiriusSpectrumImport(),
					MS2Spectra = new List<DFLSiriusSpectrumImport>()
				};

				// get MS1 centroids
				var singlyChargedMass = AbstractIon.ConvertMass(feature.MW, mZero, feature.Charge < 0 ? mhMinus : mhPlus);
				var minMass = Math.Min(feature.Mass, singlyChargedMass) - 1;
				var maxMass = Math.Max(feature.Mass, singlyChargedMass) + 8;
				var ms1Spectrum = spectra[bestHitMS1.Item2.First().ID].Spectrum;
				var ms1Centroids = ms1Spectrum.PeakCentroids.FindAllPeaksWithinMassRange(minMass, maxMass);

				// add MS1 spectrum
				feature.MS1Spectrum.Name = "MS1";
				feature.MS1Spectrum.MSLevel = 1;
				feature.MS1Spectrum.ScanNumber = ms1Spectrum.Header.ScanNumbers.First();
				feature.MS1Spectrum.PrecursorMW = bestHitMS1.Item1.MolecularWeight;
				feature.MS1Spectrum.PrecursorMass = bestHitMS1.Item1.Mass;
				feature.MS1Spectrum.PrecursorCharge = bestHitMS1.Item1.Charge;
				feature.MS1Spectrum.PrecursorAdduct = bestHitMS1.Item1.IonDescription;
				feature.MS1Spectrum.Masses = ms1Centroids.Select(s => s.Position).ToArray();
				feature.MS1Spectrum.Intensities = ms1Centroids.Select(s => s.Intensity).ToArray();

				// add MS2 spectra
				foreach (var spectrumInfo in bestHitMS2.Item2.Where(w => w.MSOrder == MSOrderType.MS2))
				{
					var ms2Spectrum = spectra[spectrumInfo.ID].Spectrum;
					feature.MS2Spectra.Add(new DFLSiriusSpectrumImport
					{
						Name = $"MS{(int)ms2Spectrum.ScanEvent.MSOrder}_{ms2Spectrum.Header.ScanNumbers.First()}",
						MSLevel = (int)ms2Spectrum.ScanEvent.MSOrder,
						ScanNumber = ms2Spectrum.Header.ScanNumbers.First(),
						PrecursorMW = bestHitMS2.Item1.MolecularWeight,
						PrecursorMass = bestHitMS2.Item1.Mass,
						PrecursorCharge = bestHitMS2.Item1.Charge,
						PrecursorAdduct = bestHitMS2.Item1.IonDescription,
						CollisionEnergies = ms2Spectrum.ScanEvent.ActivationEnergies.ToArray(),
						Masses = ms2Spectrum.PeakCentroids.Select(s => s.Position).ToArray(),
						Intensities = ms2Spectrum.PeakCentroids.Select(s => s.Intensity).ToArray(),
					});
				}

				// write to JSON
				jsonSerializer.Serialize(jsonWriter, feature);
				jsonWriter.Flush();

				// increase tmp ID
				++compoundTmpID;
			}

			// finalize JSON
			jsonWriter.WriteEndArray();

			SendAndLogVerboseMessage($"Writing {compoundTmpID-1} SIRIUS features import took {timer.ElapsedToDisplayString()}.");
		}

		/// <summary>
		/// Runs specified executable.
		/// </summary>
		private int ExecuteScript(string outputDir)
		{
			SendAndLogTemporaryMessage("Executing SIRIUS search tool...");
			var timer = Stopwatch.StartNew();

			// get script folder
			var toolDir = Path.Combine(ServerConfiguration.ToolsDirectory, "CDSirius");

			// load script settings
			var settings = LoadScriptSettings(toolDir);

			// check Python exists
			var pythonPath = settings["PythonPath"];
			if (File.Exists(pythonPath) == false)
			{
				SendAndLogErrorMessage($"Python executable not found: '{pythonPath}'");
				throw new MagellanProcessingException("Python executable not found.");
			}

			// check main script exists
			var pythonScript = Path.Combine(toolDir, "main.py");
			if (File.Exists(pythonScript) == false)
			{
				SendAndLogErrorMessage($"Node script not found: '{pythonScript}'");
				throw new MagellanProcessingException("Node script not found.");
			}

			// check SIRIUS exists
			var siriusPath = settings["SiriusPath"];
			if (File.Exists(siriusPath) == false)
			{
				SendAndLogErrorMessage($"SIRIUS executable not found: '{siriusPath}'");
				throw new MagellanProcessingException("SIRIUS executable not found.");
			}

			// init message parser
			var messageRegex = new Regex(@"^CDS ([A-Z]+): (.+)$");

			// get script params
			var execParam = $"\"{pythonScript}\" \"{outputDir}\"";

			// run executable
			var exitCode = ExternalProcessHelper.ExecuteAbortableProcess(

				// executable and its params
				pythonPath,
				FileHelper.GetShortFileName(Path.GetDirectoryName(pythonPath)),
				execParam,

				// output messages handler
				delegate (object sender, DataReceivedEventArgs e)
				{
					// check event data
					if (e == null || string.IsNullOrEmpty(e.Data))
					{
						return;
					}

					// parse message
					var match = messageRegex.Match(e.Data);

					// log messages not coming from the script
					if (match.Success == false)
					{
						NodeLogger.Debug(e.Data);
						return;
					}

					// get message
					var messageType = match.Groups[1].Value;
					var messageValue = match.Groups[2].Value;

					switch (messageType)
					{

						// handle info
						case "INFO":
							SendAndLogMessage(messageValue);
							break;

						// handle temporary
						case "TEMP":
							SendAndLogTemporaryMessage(messageValue);
							break;

						// handle verbose
						case "VERBOSE":
							SendAndLogVerboseMessage(messageValue);
							break;

						// handle warnings
						case "WARNING":
							SendAndLogWarningMessage(messageValue);
							break;

						// handle errors
						case "ERROR":
							SendAndLogErrorMessage(messageValue);
							break;

						// handle progress
						case "PROGRESS":
							if (double.TryParse(messageValue, out var percentage))
							{
								var progress = Math.Min(1, Math.Max(0, percentage / 100.0));
								ReportSingleStepProgress(progress, "SIRIUS progress");
							}
							break;

						// forward to console log
						default:
							NodeLogger.Debug(e.Data);
							break;
					}
				},

				// error handler
				delegate (object sender, DataReceivedEventArgs e)
				{
					// Ignore all error messages since there is too much text going from Sirius
					// and all this text is provided into standard error output.
					// In addition, it seems to be somehow async, which causes database locks very often.
				},

				// cancellation token
				JobCancellationToken);

			SendAndLogVerboseMessage($"SIRIUS search took {timer.ElapsedToDisplayString()}.");

			return exitCode;
		}

		/// <summary>
		/// Loads main settings from the node scripts folder.
		/// </summary>
		private Dictionary<string, string> LoadScriptSettings(string scriptDir)
		{
			// check settings exists
			var settingsPath = Path.Combine(scriptDir, "settings.json");
			if (File.Exists(settingsPath) == false)
			{
				SendAndLogErrorMessage($"Main script settings not found: '{settingsPath}'");
				throw new MagellanProcessingException("Main script settings not found.");
			}

			// load content
			var content = File.ReadAllText(settingsPath);
			var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
			
			return settings ?? new Dictionary<string, string>();
		}

		/// <summary>
		/// Loads and persists unique formulas.
		/// </summary>
		private void LoadAndPersistFormulas(string outputDir)
		{
			// init progress
			SendAndLogTemporaryMessage("Processing formula results...");
			var timer = Stopwatch.StartNew();

			// init containers
			m_formulaIDs = new Dictionary<string, object[]>();
			var entityItems = new List<DFLSiriusFormulaItem>();
			var compoundConnections = new Dictionary<int, EntityConnectionItemList<ConsolidatedUnknownCompoundItem, DFLSiriusFormulaItem>>();

			// init reader
			var reader = LoadResultJSON<DFLSiriusFormulaResult>(outputDir, "sirius_formulas");
			
			// load formulas
			foreach (var resultData in reader)
			{
				JobCancellationToken.ThrowIfCancellationRequested();

				// validate and format formula
				if (ChemicalCompositionCalculator.TryCalculateElementalCompositionFormula(resultData.Formula, string.Empty, null, out var parsedComposition) == false)
				{
					WriteLogMessage(MessageLevel.Warn, $"Result '{resultData.Formula}' skipped due to unrecognizable formula.");
					continue;
				}

				// init entity item
				var resultItem = new DFLSiriusFormulaItem
				{
					ID = EntityDataService.NextId<DFLSiriusFormulaItem>(),
					ElementalCompositionFormula = parsedComposition,
					MolecularWeight = MolecularMassCalculator.CalculateMonoisotopicMass(parsedComposition, 0),
					Rank = resultData.Rank,
					SiriusScore = resultData.SiriusScore,
					TreeScore = resultData.TreeScore,
					IsotopeScore = resultData.IsotopeScore,
					MS2ErrorInPPM = resultData.MS2ErrorPpm,
					ExplainablePeaksCount = resultData.ExplainablePeaksCount,
					ExplainedPeaksCount = resultData.ExplainedPeaksCount,
					ExplainedIntensity = resultData.ExplainedIntensity,
					IdentifyingNodeNumber = ProcessingNodeNumber,
				};

				// calc delta masses
				var measuredMass = m_compoundMWs[resultData.ExternalID];
				resultItem.DeltaMassInDa = MassError.CalculateErrorInU(measuredMass, resultItem.MolecularWeight);
				resultItem.DeltaMassInPPM = MassError.CalculateErrorInPpm(measuredMass, resultItem.MolecularWeight);

				// store item
				entityItems.Add(resultItem);

				// add to main map
				m_formulaIDs[resultData.SiriusFormulaID] = resultItem.GetIDs();

				// get or init compound connection
				if (compoundConnections.TryGetValue(resultData.ExternalID, out var compoundConnection) == false)
				{
					compoundConnection = new EntityConnectionItemList<ConsolidatedUnknownCompoundItem, DFLSiriusFormulaItem>(m_compoundIDs[resultData.ExternalID]);
					compoundConnections.Add(resultData.ExternalID, compoundConnection);
				}

				// add connection data
				compoundConnection.AddConnection(resultItem.GetIDs());
			}

			// persist results
			if (entityItems.Any())
			{
				EntityDataService.InsertItems(entityItems);
				EntityDataService.ConnectItems(compoundConnections.Values);
			}

			SendAndLogVerboseMessage($"Processing {entityItems.Count} formulas took {timer.ElapsedToDisplayString()}.");
		}

		/// <summary>
		/// Loads and persists unique classes.
		/// </summary>
		private void LoadAndPersistClasses(string outputDir)
		{
			// check if needed
			if (PredictCompoundClasses.Value == false)
			{
				return;
			}

			// init progress
			SendAndLogTemporaryMessage("Processing compound classes results...");
			var timer = Stopwatch.StartNew();

			// init containers
			var entityItems = new Dictionary<int, DFLSiriusClassItem>();
			var compoundConnections = new Dictionary<int, EntityConnectionItemList<ConsolidatedUnknownCompoundItem, DFLSiriusClassItem>>();
			var formulaConnections = new Dictionary<object[], EntityConnectionItemList<DFLSiriusFormulaItem, DFLSiriusClassItem>>(IdArrayComparer.Instance);

			// init reader
			var reader = LoadResultJSON<DFLSiriusClassResult>(outputDir, "sirius_classes");

			// load formulas
			foreach (var resultData in reader)
			{
				JobCancellationToken.ThrowIfCancellationRequested();

				// check if formula available
				if (m_formulaIDs.ContainsKey(resultData.SiriusFormulaID) == false)
				{
					continue;
				}

				// get or init entity item
				if (entityItems.TryGetValue(resultData.ClassID, out var resultItem) == false)
				{
					// init entity item
					resultItem = new DFLSiriusClassItem
					{
						ID = EntityDataService.NextId<DFLSiriusClassItem>(),
						LevelIndex = resultData.LevelIndex,
						Level = resultData.Level,
						Name = resultData.Name,
						Description = resultData.Description,
						ClassyFireID = resultData.ClassID,
						IdentifyingNodeNumber = ProcessingNodeNumber,
					};

					// store item
					entityItems.Add(resultData.ClassID, resultItem);
				}

				// get or init compound connection
				if (compoundConnections.TryGetValue(resultData.ExternalID, out var compoundConnection) == false)
				{
					compoundConnection = new EntityConnectionItemList<ConsolidatedUnknownCompoundItem, DFLSiriusClassItem>(m_compoundIDs[resultData.ExternalID]);
					compoundConnections.Add(resultData.ExternalID, compoundConnection);
				}

				// add compound connection data
				compoundConnection.AddConnection(
					resultItem.GetIDs(),
					new object[] { resultData.TreeID },
					new object[] { resultData.Probability });

				// get or init formula connection
				var formulaIDs = m_formulaIDs[resultData.SiriusFormulaID];
				if (formulaConnections.TryGetValue(formulaIDs, out var formulaConnection) == false)
				{
					formulaConnection = new EntityConnectionItemList<DFLSiriusFormulaItem, DFLSiriusClassItem>(formulaIDs);
					formulaConnections.Add(formulaIDs, formulaConnection);
				}

				// add formula connection
				formulaConnection.AddConnection(
					resultItem.GetIDs(),
					new object[] { resultData.TreeID },
					new object[] { resultData.Probability });
			}

			// persist results
			if (entityItems.Any())
			{
				EntityDataService.InsertItems(entityItems.Values);
				EntityDataService.ConnectItems(compoundConnections.Values);
				EntityDataService.ConnectItems(formulaConnections.Values);
			}

			SendAndLogVerboseMessage($"Processing {entityItems.Count} unique classes took {timer.ElapsedToDisplayString()}.");
		}

		/// <summary>
		/// Loads and persists unique structures.
		/// </summary>
		private void LoadAndPersistStructures<TItem>(string outputDir, bool enabled, string outputFile, string label)
			where TItem : DFLSiriusStructureItemBase, new()
		{
			// check if needed
			if (enabled == false)
			{
				return;
			}

			// init progress
			SendAndLogTemporaryMessage($"Processing {label} results...");
			var timer = Stopwatch.StartNew();
			var totalCount = 0;
			var uniqueCount = 0;

			// init containers
			var entityItems = new Dictionary<string, TItem>();
			var compoundConnections = new Dictionary<int, EntityConnectionItemList<ConsolidatedUnknownCompoundItem, TItem>>();
			var formulaConnections = new Dictionary<object[], EntityConnectionItemList<DFLSiriusFormulaItem, TItem>>(IdArrayComparer.Instance);
			var formulaConnectionsChecks = new Dictionary<object[], HashSet<int>>(IdArrayComparer.Instance);

			// init reader
			var reader = LoadResultJSON<DFLSiriusStructureResult>(outputDir, outputFile);

			// load formulas
			foreach (var resultData in reader)
			{
				JobCancellationToken.ThrowIfCancellationRequested();
				++totalCount;

				// check if formula available
				if (m_formulaIDs.TryGetValue(resultData.SiriusFormulaID, out var formulaIDs) == false)
				{
					continue;
				}

				// get or init entity item
				if (entityItems.TryGetValue(resultData.InChIKey, out var resultItem) == false)
				{
					// validate and format formula
					if (ChemicalCompositionCalculator.TryCalculateElementalCompositionFormula(resultData.Formula, string.Empty, null, out var parsedComposition) == false)
					{
						WriteLogMessage(MessageLevel.Warn, $"Result '{resultData.Formula}' skipped due to unrecognizable formula.");
						continue;
					}

					// init entity item
					resultItem = new TItem
					{
						ID = EntityDataService.NextId<DFLSiriusStructureItem>(),
						Name = resultData.Name,
						ElementalCompositionFormula = parsedComposition,
						MolecularWeight = MolecularMassCalculator.CalculateMonoisotopicMass(parsedComposition, 0),
						LogKow = resultData.LogKow,
						PubChemID = resultData.PUBCHEM,
						HmdbID = resultData.HMDB,
						KeggID = resultData.KEGG,
						DSSToxID = resultData.DSSTOX,
						InChIKey = resultData.InChIKey,
						SMILES = resultData.SMILES,
						IdentifyingNodeNumber = ProcessingNodeNumber,
					};

					// convert structure
					var molString = StructureUtilities.CreateStructureFromSmilesString(resultData.SMILES, false, false)?.MolStructure;
					if (molString.IsNullOrEmpty() == false)
					{
						resultItem.MolStructure = molString;
						resultItem.InChI = StructureUtilities.CreateInChIFromMolString(molString, false);
					}

					// store item
					entityItems.Add(resultData.InChIKey, resultItem);
					++uniqueCount;
				}

				// calc delta masses
				var measuredMass = m_compoundMWs[resultData.ExternalID];
				var deltaMassInDa = MassError.CalculateErrorInU(measuredMass, resultItem.MolecularWeight);
				var deltaMassInPpm = MassError.CalculateErrorInPpm(measuredMass, resultItem.MolecularWeight);

				// get or init compound connection
				if (compoundConnections.TryGetValue(resultData.ExternalID, out var compoundConnection) == false)
				{
					compoundConnection = new EntityConnectionItemList<ConsolidatedUnknownCompoundItem, TItem>(m_compoundIDs[resultData.ExternalID]);
					compoundConnections.Add(resultData.ExternalID, compoundConnection);
				}

				// add compound connection data
				compoundConnection.AddConnection(
					resultItem.GetIDs(),
					new object[]
					{
						deltaMassInDa,
						deltaMassInPpm,
						resultData.Rank,
						resultData.CSIScore,
						resultData.TanimotoSimilarity,
					});

				// get or init formula connection
				if (formulaConnections.TryGetValue(formulaIDs, out var formulaConnection) == false)
				{
					formulaConnection = new EntityConnectionItemList<DFLSiriusFormulaItem, TItem>(formulaIDs);
					formulaConnections.Add(formulaIDs, formulaConnection);
					formulaConnectionsChecks.Add(formulaIDs, new HashSet<int>());
				}

				// add formula connection
				if (formulaConnectionsChecks[formulaIDs].Contains(resultItem.ID) == false)
				{
					formulaConnection.AddConnection(resultItem.GetIDs());
					formulaConnectionsChecks[formulaIDs].Add(resultItem.ID);
				}
			}

			// persist results
			if (entityItems.Any())
			{
				EntityDataService.InsertItems(entityItems.Values);
				EntityDataService.ConnectItems(compoundConnections.Values);
				EntityDataService.ConnectItems(formulaConnections.Values);
			}

			SendAndLogVerboseMessage($"Processing {totalCount} results with {uniqueCount} unique {label}s took {timer.ElapsedToDisplayString()}.");
		}

		/// <summary>
		/// Loads and persists top annotations.
		/// </summary>
		private void LoadAndPersistTopAnnotations(string outputDir)
		{
			// init progress
			SendAndLogTemporaryMessage("Processing top annotations...");
			var timer = Stopwatch.StartNew();

			// get registered properties
			var properties = new List<PropertyAccessor<ConsolidatedUnknownCompoundItem>>()
				{
					m_compoundTopScoreAccessor,
					m_compoundTopSimilarityAccessor,
					m_compoundTopConfidenceExactAccessor,
					m_compoundTopConfidenceApproxAccessor,
					m_compoundTopClassLevel1Accessor,
					m_compoundTopClassLevel2Accessor,
					m_compoundTopClassLevel3Accessor,
					m_compoundTopClassLevel4Accessor,
					m_compoundTopClassLevel5Accessor,
					m_compoundTopClassLevel6Accessor,
				}
				.Where(w => w != null)
				.ToArray();

			// check if any property registered
			if (properties.Any() == false)
			{
				SendAndLogVerboseMessage("No properties to read from top annotations.");
				return;
			}

			// init containers
			var itemsToUpdate = new List<ConsolidatedUnknownCompoundItem>();

			// load top annotations
			var annotations = LoadResultJSON<DFLSiriusTopAnnotationResult>(outputDir, "sirius_top_annotations")
				.ToDictionary(k => m_compoundIDs[k.ExternalID], comparer: IdArrayComparer.Instance);

			// init reader
			var reader = EntityDataService.CreateEntityItemReader();

			// load annotations
			foreach (var compound in reader.ReadAll<ConsolidatedUnknownCompoundItem>(readerSettings: ReaderSettings.CreateForProperties()))
			{
				JobCancellationToken.ThrowIfCancellationRequested();

				// get top annotation
				if (annotations.TryGetValue(compound.GetIDs(), out var annotation) == false)
				{
					continue;
				}

				// validate and format formula
				if (ChemicalCompositionCalculator.TryCalculateElementalCompositionFormula(annotation.Formula, string.Empty, null, out var parsedComposition) == false)
				{
					WriteLogMessage(MessageLevel.Warn, $"Result '{annotation.Formula}' skipped due to unrecognizable formula.");
					continue;
				}

				// set scores to compound
				m_compoundTopScoreAccessor?.SetValue(compound, annotation.CSIFingerIDScore);
				m_compoundTopSimilarityAccessor?.SetValue(compound, annotation.CSIFingerIDTanimotoSimilarity);
				m_compoundTopConfidenceExactAccessor?.SetValue(compound, annotation.CSIFingerIDConfidenceExact);
				m_compoundTopConfidenceApproxAccessor?.SetValue(compound, annotation.CSIFingerIDConfidenceApprox);

				// set classes to compound
				m_compoundTopClassLevel1Accessor?.SetValue(compound, annotation.ClassyFireLevel1);
				m_compoundTopClassLevel2Accessor?.SetValue(compound, annotation.ClassyFireLevel2);
				m_compoundTopClassLevel3Accessor?.SetValue(compound, annotation.ClassyFireLevel3);
				m_compoundTopClassLevel4Accessor?.SetValue(compound, annotation.ClassyFireLevel4);
				m_compoundTopClassLevel5Accessor?.SetValue(compound, annotation.ClassyFireLevel5);
				m_compoundTopClassLevel6Accessor?.SetValue(compound, annotation.ClassyFireLevel6);

				// store compound
				itemsToUpdate.Add(compound);
			}

			// update items
			if (itemsToUpdate.Any())
			{
				EntityDataService.UpdateItems(itemsToUpdate, properties);
			}

			SendAndLogVerboseMessage($"Processing {itemsToUpdate.Count} top annotations took {timer.ElapsedToDisplayString()}.");
		}

		/// <summary>
		/// Loads results JSON file.
		/// </summary>
		private IEnumerable<TResult> LoadResultJSON<TResult>(string outputDir, string outputName)
		{
			// check if exists
			var path = Path.Combine(outputDir, $"{outputName}.json");
			if (File.Exists(path) == false)
			{
				SendAndLogErrorMessage($"Output file not found: '{path}'");
				yield break;
			}

			// read records
			using var reader = new StreamReader(path);
			while (reader.ReadLine() is { } line)
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				var obj = JsonConvert.DeserializeObject<TResult>(line);
				if (obj != null)
				{
					yield return obj;
				}
			}
		}
	}
}
