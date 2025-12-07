#  CDSirius

CDSirius is an implementation of the [SIRIUS](https://v6.docs.sirius-ms.io/) suite within
[Compound Discoverer 3.5](https://mycompounddiscoverer.com/). This software is intended to allow annotation of
metabolites and other small molecules with molecular formulas, 2D structures, and compound classes based on acquisition
of high-resolution, accurate mass MS/MS through data-dependent LC-MS/MS analysis of complex samples.


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


## Requirements

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


## Changelog

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