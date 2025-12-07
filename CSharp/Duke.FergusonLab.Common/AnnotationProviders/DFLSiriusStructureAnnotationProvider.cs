//-----------------------------------------------------------------------------
// Copyright (c) 2025, Lee Ferguson Lab @ Duke
// All rights reserved
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Thermo.Magellan.EntityDataFramework;
using Thermo.Metabolism.DataObjects.Constants;
using Thermo.Metabolism.DataObjects.EntityDataObjects;
using Thermo.Metabolism.Services;
using Thermo.Metabolism.Services.Interfaces;
using Duke.FergusonLab.Common.EntityItems;

namespace Duke.FergusonLab.Common.AnnotationProviders
{
	/// <summary>
	/// Handles compound annotations for <see cref="DFLSiriusStructureItem"/> source.
	/// </summary>
	public abstract class DFLSiriusStructureAnnotationProvider<TCompound>
		: CompoundAnnotationProvider<TCompound, DFLSiriusStructureItem>
		where TCompound : DynamicEntity
	{
		public const string sourceName = "SIRIUS Structures";
		public const string semanticTerms = "CompoundAnnotationProvider/Duke/FergusonLab/SiriusStructures";

		private readonly Lazy<string> m_rankPropertyName;
		private readonly Lazy<PropertyAccessor[]> m_additionalProperties;

		/// <summary>
		/// Initializes a new instance of compound annotation provider.
		/// </summary>
		/// <param name="entityDataService">The entity data service.</param>
		/// <param name="structureComparer">A comparer that compares two structures for equality.</param>
		protected DFLSiriusStructureAnnotationProvider(IEntityDataService entityDataService,
			IMolecularStructureComparer structureComparer)
			: base(entityDataService, structureComparer)
		{
			m_rankPropertyName = new Lazy<string>(
				() =>
				{
					var property = EntityDataService
						.GetConnectionProperties<DFLSiriusStructureItem, TCompound>(CDEntityDataPurpose.Rank)
						.FirstOrDefault();

					if (property == null)
					{
						throw new Exception("Missing rank property in DFLSiriusStructureItem.");
					}

					return property.Name;
				});

			m_additionalProperties = new Lazy<PropertyAccessor[]>(
				() =>
				{
					// check if item exists
					if (EntityDataService.ContainsEntity<DFLSiriusStructureItem>() == false)
					{
						return Array.Empty<PropertyAccessor>();
					}

					// init accessors
					var accessors = new List<PropertyAccessor>();

					// set properties
					var properties = new[]
					{
						CDEntityDataPurpose.Rank,
						CDEntityDataPurpose.PubChemId,
						CDEntityDataPurpose.HMDB,
						CDEntityDataPurpose.KeggCompoundID,
						DFLDataPurpose.DSSToxID,
						CDEntityDataPurpose.InChIKey,
						CDEntityDataPurpose.InChI,
						CDEntityDataPurpose.SMILES,
					};

					// init position
					var position = 10 + EntityDataService.GetProperties<TCompound>(CDEntityDataPurpose.AreaSumMax).FirstOrDefault()?.GridDisplayOptions.VisiblePosition ?? 100;

					// get accessors
					for (var i = 0; i < properties.Length; i++)
					{
						// get main property
						var accessor = EntityDataService.GetProperties<DFLSiriusStructureItem>(properties[i]).Cast<PropertyAccessor>().SingleOrDefault();

						// get connection property
						if (accessor == null)
						{
							accessor = EntityDataService.GetConnectionProperties<TCompound, DFLSiriusStructureItem>(properties[i]).SingleOrDefault();
						}
						
						// add property
						if (accessor != null)
						{
							accessor.GridDisplayOptions.VisiblePosition = position + i;
							accessors.Add(accessor);
						}
					}

					return accessors.ToArray();
				});
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
		/// Gets the additional property accessors to be propagated to compound level for this annotation type.
		/// </summary>
		public override PropertyAccessor[] AdditionalPropertyAccessors => m_additionalProperties.Value;

		/// <summary>
		/// Selects the top-N annotations from given items.
		/// </summary>
		/// <param name="annotations">The annotation data.</param>
		/// <param name="count">Number of top annotations to get.</param>
		protected override List<CompoundAnnotation> SelectTopNAnnotations(IList<HierarchicalEntity<DFLSiriusStructureItem>> annotations, int count)
		{
			return annotations
				.OrderBy(o => o.ConnectionProperties.GetValue(m_rankPropertyName.Value))
				.Take(count)
				.Select(ConvertToAnnotation)
				.ToList();
		}

		/// <summary>
		/// Selects the best annotation from given items.
		/// </summary>
		/// <param name="annotations">The annotation data.</param>
		protected override CompoundAnnotation SelectBestAnnotation(IList<HierarchicalEntity<DFLSiriusStructureItem>> annotations)
		{
			// select the best annotation
			var annotationData = annotations.OrderBy(o => o.ConnectionProperties.GetValue(m_rankPropertyName.Value)).First();

			// make annotation
			return ConvertToAnnotation(annotationData);
		}
	}

	/// <summary>
	/// Handles annotation data for <see cref="ConsolidatedUnknownCompoundItem"/> using <see cref="DFLSiriusStructureItem"/>.
	/// </summary>
	[CompoundAnnotationProviderExport(
		sourceName,
		typeof(ICompoundAnnotationProvider<ConsolidatedUnknownCompoundItem>),
		Thermo.Metabolism.Services.Interfaces.AnnotationType.Identification,
		UseMzLogicScoring = false,
		UseSpectralDistanceScoring = false,
		AdditionalProperties = new[] { "CSI Rank", "PubChem CID", "HMDB ID", "KEGG ID", "DSSTox ID", "InChIKey", "InChI", "SMILES" })]

	public class DFLSiriusStructureUnknownCompoundAnnotationProvider : DFLSiriusStructureAnnotationProvider<ConsolidatedUnknownCompoundItem>
	{
		[ImportingConstructor]
		public DFLSiriusStructureUnknownCompoundAnnotationProvider(
			[Import(Source = ImportSource.Local, AllowDefault = true)] IEntityDataService entityDataService,
			[Import] IMolecularStructureComparer structureComparer)
			: base(entityDataService, structureComparer)
		{ }
	}
}
