#  CDSirius

CDSirius is an implementation of the [SIRIUS](https://v6.docs.sirius-ms.io/) suite maintained by the Böcker lab at University of Jena within
[Compound Discoverer 3.5](https://mycompounddiscoverer.com/). This software is intended to allow annotation of metabolites and other small molecules with molecular formulas, 2D structures, and compound classes based on acquisition of high-resolution, accurate mass MS/MS through data-dependent LC-MS/MS analysis of complex samples.  Note that cdSirius will only work with high-resolution MS and MS/MS data, and has only been tested with data-dependent analysis (DDA) results.  It may work with DIA data, but no guarantees are given.

Implementation of Sirius within Compound Discoverer is done through C# wrappers of core functions written in Python.  Within this implementation, Sirius runs on the host PC and is called as a background service during processing through the Sirius API.  Mass spectral data for discrete compounds detected by Compound Discoverer are passed to Sirius for processing, and results are reported back to CD and persisted to the result file for viewing and interpretation.  Linkages among CD compounds and Sirius results are maintained within the resulting tables.  Sirius results (formulas and structures) may be selected for use as annotation sources.

When using CDSirius results in a publication, please be sure to cite the work that enabled creation of this resource.  Visit the Sirius development group's site referenced above for detailed citation information, and use the primary citation as follows:

Kai Dührkop, Markus Fleischauer, Marcus Ludwig, Alexander A. Aksenov, Alexey V. Melnik, Marvin Meusel, Pieter C. Dorrestein, Juho Rousu and Sebastian Böcker. [SIRIUS 4: Turning tandem mass spectra into metabolite structure information](https://doi.org/10.1038/s41592-019-0344-8). _Nature Methods_ 16, 299–302, 2019.


## Project Info

- *CDSirius* - Python part of the node. This part is responsible for connecting or initializing new SIRIUS service,
search data submission, results retrieval and basic preprocessing for the main C# node. All the inputs and outputs
are handled via JSON files.

- *CSharp* - C# part if the node. This part contains VisualStudio solution implementing the main node, all necessary
data types and annotation source providers. All the dependencies are linked to default installation folder of
CompoundDiscoverer 3.5.

- *Data* - Example JSON input for the Python part of the node. To simplify development and debugging, a simple search
can be executed by running the *node.py* script directly. Similarly, the running the *sirius.py* directly can be used
to destroy running SIRIUS service if still running.


## Dependencies

- Fully licensed installation of Compound Discoverer 3.5.
- SIRIUS user account.
- [SIRIUS Suite 6.3.3](https://v6.docs.sirius-ms.io/)
- [Python 3.11+](https://www.python.org)
- [Numpy](https://pypi.org/project/numpy/)
- [pandas](https://pypi.org/project/pandas/)
- [PySirius 6.3.3](https://github.com/sirius-ms/sirius-client-openAPI)


## Installation

1) Install [SIRIUS](https://v6.docs.sirius-ms.io/) program and create user account.

2) Install [Python](https://www.python.org) and required dependencies.

3) Inside the *CDSirius* folder locate the *settings.json* file and open it in a plain text editor. Modify the Python
and SIRIUS paths according to your installation. Specify your SIRIUS account username and password. The SIRIUS REST API
port can be changed as well if needed.

4) Copy the whole *CDSirius* folder to Compound Discoverer installation tools folder:
*C:\Program Files\Thermo\Compound Discoverer 3.5\Tools\\*

5) From the *CSharp\bin\* folder copy all files to Compound Discoverer server installation folder:
*C:\Program Files\Thermo\Compound Discoverer 3.5\Thermo.CompoundDiscoverer.Server\\*

6) From the *CSharp\bin\* folder copy *Duke.FergusonLab.Common.dll* and *Duke.FergusonLab.Common.pdb* files to Compound
Discoverer client installation folder:
*C:\Program Files\Thermo\Compound Discoverer 3.5\Thermo.CompoundDiscoverer\\*

7) Make sure the copied files are not blocked by your system. Right-click individual files, select properties and
check the "Unblock" checkbox if available.

8) Launch Compound Discoverer and navigate to the Help -> License Manager dialogue. Run "Scan for Missing Features".

9) Restart Compound Discoverer to complete installation and allow new nodes to be registered.

## Using CDSirius within a Compound Discoverer workflow
The CDSirius node is a "Compound Identification" node that can be included in an existing full processing workflow, or it can be used in a "reprocessing" workflow to retrospectively add Sirius results to the cdResult file.  Either way, you will find the Search by SIRIUS node within the Workflow Editor Node menu, in the _7. Compound Identification_ sub menu:

   <img width="194" alt="CDSirius icon" src="https://github.com/user-attachments/assets/7c79ea76-a1dc-41f3-9905-7e460650efb6" />

After adding the node to the workflow, the processing configuration dialogue is available for editing.  Default parameters are provided for most settings:

   <img width="650" alt="CDSirius params" src="https://github.com/user-attachments/assets/b084e2b2-27ff-429b-ab1c-a6453443ac01" />

   **Figure 1.** Sirius node parameter configuration.

### CDSirius parameter settings
The range of possible settings for Sirius is very large and the corresponding job configurations can become quite complicated.  The settings available within cdSirius represent a subset of possible parameters, chosen based on their general applicability and typical use cases.  A complete guide for Sirius job parameters is beyond the scope of this program, but extensive documentation is available for Sirius [elsewhere](https://v6.docs.sirius-ms.io/methods-background/).
1.  **General Settings:**  These are global settings for the Sirius program service.
   - <ins>MS1 Mass Tolerance</ins>: The known mass accuracy threshold (in ppm) for your MS1 data.  Default = 2 ppm.  _This is a critical parameter and must be set accordingly.  If your instrument is equipped with EasyIC, it is suggested that you use it_`
   - <ins>MS2 Mass Tolerance</ins>: The known mass accuracy threshold (in ppm) for your MS2 data.  Default = 5 ppm.  _This is also critical and is often a less accurate measure than for MS1, especially when using lower resolutions (e.g. 15K).  It is not recommended to use EasyIC for Orbitrap MS2 with resolutions < 60K_
   - <ins>Predict Compound Classes</ins>: When this parameter is set to "True", the CANOPUS implementation of the ClassyFire algorithm is used to predict compound classes from molecular fingerprints.
   - <ins>Predict Structures</ins>: Toggle enabling CSI:FingerID database search
   - <ins>Predict de-Novo Structures</ins>: Enables or disables MSNovelist processing.  **Caution:** MSNovelist is quite computationally intensive.  Be careful when using this toolset with full (unfiltered) compound sets from Compound Discoverer, as the job compute times can become extremely long.
   - <ins>Molecular Weight Threshold</ins>: Sirius computation becomes extremely slow for large molecules, so it is best to limit the upper MW range to only those of interest for a particular analysis.  Default = 1500
2.  **Formula Prediction:**  This set of parameters controls the "base" molecular formula calculation functions within Sirius and is necessary for all further processing
   - <ins>Max Formula Candidates</ins>: Adjust to increase or decrease the allowable formula candidates that can be considered by Sirius.  Default = 10.
   - <ins>Elemental Constraints</ins>: Use this string to specify which elements should be considered for _de novo_ formula prediction.  Numbers represent maximum possible element counts.  Elements without numbers are given unlimited maximum counts.  **Note:** Do not include B, Cl, Br, S, or Se in this list, as those elements are detected automatically using observed isotope patterns in the MS1 spectra.
   - <ins>Check Isotope Pattern</ins>: Enabling this parameter will use isotope pattern measurement as a pre-filter to exclude formulas that are inconsistent with the measured isotope pattern, regardless of MS/MS tree score.
   - <ins>Enforce Recognized Lipids</ins>: This setting enables an internal Sirius algorithm that attempts to detect fragmentation patterns characteristic of lipids.  When detected, the corresponding lipid-like molecular formula will be prioritized as a candidate.
   - <ins>Bottom-Up Search</ins>: This setting allows for the use of a "bottom-up" formula candidate selection strategy, which uses combinations of known formulas for potential sub-fragments to build candidate molecular formulas.  It is less restrictive than searching a database for candidate formulas but is also less computationally-intensive than a true _de novo_ formula prediction strategy.
   - <ins>_De-novo_ Mass Threshold</ins>: Below this _m/z_, all molecular formulas are calculated using a _de novo_ approach, which maximizes the chance to observe novel formulas (which are not present in any databases).  Above this _m/z_, formula candidates are predicted using the "bottom-up" strategy if enabled above.
3.  **Class Prediction:**  This parameter set controls the CANOPUS classification algorithm.
   - <ins>Classes to Push</ins>: Select the ClassyFire taxonomy levels of interest that will be pushed to the Compounds table in the Compound Discoverer Results.   
4.  **Structure Prediction:**  These settings control the CSI:FingerID structure prediction toolset within Sirius.
   - <ins>Max Structure Candidates</ins>: This parameter limits the number of structure candidates scored by CSI:FingerID (when enabled) and reported back to Compound Discoverer.  Default = 10, Max = 500.  _This is a critical parameter and it is recommended that the maximum value of 500 be used in most cases_
   - <ins>Max De-Novo Structure Candidates</ins>: This parameter limits the number of structure candidates scored by MSNovelist (when enabled) and reported back to Compound Discoverer.  Default = 10, Max = 500.  _Large numbers of de novo structure candidates may cause a significant increase in processing time.  This parameter should be increased with caution_
   - <ins>Databases</ins>: Select from an abbreviated list of databases for providing structure candidates to score.
   - <ins>PubChem as Fallback</ins>: CDSirius uses defined structure databases for searching compound structure candidates.  When PubChem is not selected as a database and this parameter is set to "True", Sirius will search PubChem for structure candidates in the event that no viable structure candidates were found within the target database(s).
5.  **SIRIUS Settings:**  These settings control handling of Sirius project space output.  
   - <ins>Keep Project Space</ins>: Setting this to `True` will enable the .sirius workspace to be persisted as a permanent file, saved to the same directory with the cdResult file.  This is useful if e.g. you plan to re-open and analyze data using the Sirius GUI at a later time.
   - <ins>Keep Fingerprints</ins>: This setting will toggle saving tab-separated ASCII files (.txt) containing the predicted fingerprints for compounds processed in Sirius as well as the fingerprint definition key used by Sirius.  These files will be saved to the same directory as the cdResult file.
6.  **Reprocessing:**
   -  <ins>Checked Only</ins>: This is a switch that allows for down-selection of only compounds of interest when CDSirius is used in reprocessing of existing cdResult files.  Selecting "Checked" will pass only "checked" compounds to Sirius for calculation. The default of "All" will pass all compounds to the Sirius service.  **Note**: The default setting of "False" must be used when including the CDSirius node in a full processing workflow.  Checked status is only available for reprocessing of previously processed results.

## Changelog

### v1.8
- Plotting options set to items properties.

### v1.7
- DLLs renamed to be consistent with CD and future ready.
- Renamed "Class ID" to "ClassyFire ID".
- Renamed "Rank" to "CSI Rank" in structures.
- Added option to push "CSI Rank" of structures to compounds level.
- Added option to push "ClassyFire classes" to compounds level.
- Added web link editors for "DSSTox ID" and "ClassyFire ID" (needs CD 3.5 SP1).

### v1.6
- Fixed crash if denovo search is disabled.
- Changed structure conversion methods to ensure CD compatibility.

### v1.5
- Added extra columns for compound top score, similarity and confidence.

### v1.4
- Ensure M+H or M-H m/z is always within the MS1 spectrum export.

### v1.3
- Removed consolidation of formulas.
- Added HMDB and KEGG searches.
- Added parameter for database selection.
- Keep hierarchy of classes.
- Removing project even for crashed search.
- Split candidates limits for structures and de-novo structures.

## Disclaimer

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
