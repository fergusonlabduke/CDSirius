//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Thermo.Magellan.EntityDataFramework;
using Thermo.Metabolism.DataObjects.EntityDataObjects;
using Thermo.Metabolism.Services;
using Thermo.Metabolism.Services.Interfaces;
using Duke.FergusonLab.Common.EntityItems;

namespace Duke.FergusonLab.Common.AnnotationProviders
{
	/// <summary>
	/// Handles compound annotations for <see cref="DFLSiriusFormulaItem"/> source.
	/// </summary>
	public abstract class DFLSiriusFormulaCompoundAnnotationProvider<TCompound>
		: CompoundAnnotationProvider<TCompound, DFLSiriusFormulaItem>
		where TCompound : DynamicEntity
	{
		public const string sourceName = "SIRIUS Formulas";
		public const string semanticTerms = "CompoundAnnotationProvider/Duke/FergusonLab/SiriusFormulas";

		/// <summary>
		/// Initializes a new instance of compound annotation provider.
		/// </summary>
		/// <param name="entityDataService">The entity data service.</param>
		/// <param name="structureComparer">A comparer that compares two structures for equality.</param>
		protected DFLSiriusFormulaCompoundAnnotationProvider(IEntityDataService entityDataService,
			IMolecularStructureComparer structureComparer)
			: base(entityDataService, structureComparer)
		{
		}

		/// <summary>
		/// Gets the source name.
		/// </summary>
		public override string SourceName => sourceName;

		/// <summary>
		/// Gets the unique identifier.
		/// </summary>
		public override string SemanticTerms => semanticTerms;

		/// <summary>
		/// Selects the top-N annotations from given items.
		/// </summary>
		/// <param name="annotations">The annotation data.</param>
		/// <param name="count">Number of top annotations to get.</param>
		protected override List<CompoundAnnotation> SelectTopNAnnotations(IList<HierarchicalEntity<DFLSiriusFormulaItem>> annotations, int count)
		{
			return annotations
				.OrderBy(o => o.EntityItem.Rank)
				.Take(count)
				.Select(ConvertToAnnotation)
				.ToList(); 
		}

		/// <summary>
		/// Selects the best annotation from given items.
		/// </summary>
		/// <param name="annotations">The annotation data.</param>
		protected override CompoundAnnotation SelectBestAnnotation(IList<HierarchicalEntity<DFLSiriusFormulaItem>> annotations)
		{
			// select the best annotation
			var annotationData = annotations.FirstOrDefault(f => f.EntityItem.Rank == 1);
			if (annotationData == null)
			{
				return null;
			}

			// make annotation
			return ConvertToAnnotation(annotationData);
		}
	}

	/// <summary>
	/// Handles annotation data for <see cref="ConsolidatedUnknownCompoundItem"/> using <see cref="DFLSiriusFormulaItem"/>.
	/// </summary>
	[CompoundAnnotationProviderExport(
		sourceName,
		typeof(ICompoundAnnotationProvider<ConsolidatedUnknownCompoundItem>),
		Thermo.Metabolism.Services.Interfaces.AnnotationType.Identification,
		UseMzLogicScoring = false,
		UseSpectralDistanceScoring = false)]

	public class DFLSiriusFormulaUnknownCompoundAnnotationProvider : DFLSiriusFormulaCompoundAnnotationProvider<ConsolidatedUnknownCompoundItem>
	{
		[ImportingConstructor]
		public DFLSiriusFormulaUnknownCompoundAnnotationProvider(
			[Import(Source = ImportSource.Local, AllowDefault = true)] IEntityDataService entityDataService,
			[Import] IMolecularStructureComparer structureComparer)
			: base(entityDataService, structureComparer)
		{ }
	}
}
