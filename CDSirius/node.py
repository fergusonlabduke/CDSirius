#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# import modules
import os
import re
import sys
import json

from sirius import Sirius, SiriusConfig


class CDSirius(object):
    """Main CD SIRIUS node implementation."""
    
    
    def __init__(self, workdir):
        """Initializes a new instance of CDSirius."""
        
        self._workdir = workdir
        self._params = {}
    
    
    def log(self, message, level='INFO'):
        """Logs processing message."""
        
        # format level
        level = level.upper()
        
        # show all as standard output
        sys.stdout.write(f"CDS {level}: {message}\n")
        sys.stdout.flush()
    
    
    def run(self):
        """Runs the search."""
        
        # init Sirius config
        sirius_config = self._load_params()
        
        # run Sirius
        with Sirius(sirius_config, self.log) as sirius:
            
            project = None
            
            try:
                # initialize project
                project = sirius.init_project()
                
                # load features
                features = self._load_features()
                sirius.set_features(project, features)
                
                # submit job
                job = sirius.init_job()
                success = sirius.start_job(job, project)
                if not success:
                    raise RuntimeError("SIRIUS processing failed.")
                
                # retrieve results
                results = sirius.retrieve_results(job, project)
            
            except Exception as e:
                self.log(e, "ERROR")
                exit(1)
            
            # close project
            finally:
                if project is not None:
                    sirius.close_project(project)
        
        # export results
        self._export_results(results)
    
    
    def _load_params(self):
        """Loads all params and initializes SIRIUS config."""
        
        self.log("Loading SIRIUS config...", "TEMP")
        
        # load main settings from JSON
        path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "settings.json")
        with open(path) as f:
            settings = json.load(f)
        
        # load params from JSON
        config_path = os.path.join(self._workdir, "sirius_config.json")
        with open(config_path) as f:
            self._params = json.load(f)
        
        # make paths absolute
        if not os.path.isabs(self._params["ProjectPath"]):
            path = os.path.join(self._workdir, self._params["ProjectPath"])
            self._params["ProjectPath"] = os.path.abspath(path)
        
        if not os.path.isabs(self._params["FingerprintsPath"]):
            path = os.path.join(self._workdir, self._params["FingerprintsPath"])
            self._params["FingerprintsPath"] = os.path.abspath(path)
        
        # init config
        config = SiriusConfig()
        
        # set SIRIUS service
        config.service_path = settings["SiriusPath"]
        config.service_port = settings["SiriusPort"]
        
        # set credentials
        config.account_username = settings["AccountUsername"]
        config.account_password = settings["AccountPassword"]
        
        # init project space
        project_name = self._params["ProjectName"]
        project_name = re.sub(r"[^a-zA-Z0-9_-]", "", project_name)
        config.project_name = project_name
        config.project_path = self._params["ProjectPath"]
        
        # set formula search
        config.ms1_mass_deviation_allowed = float(self._params["MS1MassTolerance"])
        config.formula_id_mass_accuracy_ms2ppm = float(self._params["MS2MassTolerance"])
        config.formula_id_filter_by_isotope_pattern = bool(self._params["CheckIsotopePattern"])
        config.formula_id_enforce_el_gordo_formula = bool(self._params["EnforceRecognizedLipids"])
        config.formula_id_perform_bottom_up_search = bool(self._params["BottomUpSearch"])
        config.formula_id_perform_denovo_below_mz = float(self._params["DeNovoMassThreshold"])
        
        # init elements constraints
        constraints = re.findall("([A-Z][a-z]*)(\d*)?", self._params["ElementalConstraints"])
        config.formula_id_enforced_formula_constraints = "".join(f"{e}[{c}]" for e, c in constraints)
        
        # set compound class search
        config.canopus_enabled = bool(self._params["PredictCompoundClasses"])
        
        # set fingerprints
        config.fingerprint_prediction_enabled = bool(self._params["PredictStructures"])
        config.fingerprint_prediction_save = bool(self._params["SaveFingerprints"])
        
        # set structure search
        config.structure_db_search_dbs = list(self._params["StructuresDatabases"])
        config.structure_db_search_enabled = bool(self._params["PredictStructures"])
        config.structure_db_search_pubchem_fallback = bool(self._params["PubChemAsFallback"])
        config.structure_db_search_candidates = int(self._params["StructuresMaxCandidates"])
        
        # set denovo structure search
        config.ms_novelist_enabled = bool(self._params["PredictDeNovoStructures"])
        config.ms_novelist_candidates = int(self._params["DeNovoStructuresMaxCandidates"])
        
        return config
    
    
    def _load_features(self):
        """Initializes SIRIUS features."""
        
        self.log("Loading SIRIUS features...", "TEMP")
        
        # load features from JSON
        features_path = os.path.join(self._workdir, "sirius_features.json")
        with open(features_path) as f:
            features_data = json.load(f)
        
        # initialize features
        features = []
        for feat in features_data:
            
            # normalize adduct
            mass, charge, adduct = self._normalize_adduct(
                feat['Mass'],
                feat['MW'],
                feat['Charge'],
                feat['Adduct'])
            
            # init feature
            feat_input = {}
            feat_input['name'] = f"{feat['MW']:.5f}@{feat['ApexRT']:.3f}"
            feat_input['externalFeatureId'] = str(feat["ExternalID"])
            feat_input['ionMass'] = mass
            feat_input['charge'] = charge
            feat_input['detectedAdducts'] = [adduct]
            feat_input['rtStartSeconds'] = feat["LeftRT"] * 60
            feat_input['rtEndSeconds'] = feat["RightRT"] * 60
            
            # set MS1
            ms1_input = {}
            ms1_input['name'] = feat["MS1Spectrum"]["Name"]
            ms1_input['msLevel'] = feat["MS1Spectrum"]["MSLevel"]
            ms1_input['scanNumber'] = feat["MS1Spectrum"]["ScanNumber"]
            
            ms1_peaks = zip(feat["MS1Spectrum"]["Masses"], feat["MS1Spectrum"]["Intensities"])
            ms1_input['peaks'] = [{'mz': d[0], 'intensity': d[1]} for d in ms1_peaks]
            feat_input['mergedMs1'] = ms1_input
            
            # set MS2
            feat_input['ms2Spectra'] = []
            for spectrum in feat["MS2Spectra"]:
                
                # set MS2
                ms2_input = {}
                ms2_input['name'] = spectrum["Name"]
                ms2_input['msLevel'] = spectrum["MSLevel"]
                ms2_input['collisionEnergy'] = str(spectrum["CollisionEnergies"][0])
                ms2_input['precursorMz'] = spectrum["PrecursorMass"]
                ms2_input['scanNumber'] = spectrum["ScanNumber"]
                
                ms2_peaks = zip(spectrum["Masses"], spectrum["Intensities"])
                ms2_input['peaks'] = [{'mz': d[0], 'intensity': d[1]} for d in ms2_peaks]
                feat_input['ms2Spectra'].append(ms2_input)
            
            # add input
            features.append(feat_input)
        
        return features
    
    
    def _normalize_adduct(self, mz, mw, z, adduct):
        """Normalizes ion mass and adduct to supported adducts."""
        
        invalid_adducts = ("2M", "+2", "+3", "-2", "-3", "MeOH", "ACN", "-e", "+e")
        if any(d in adduct for d in invalid_adducts):
            
            if z < 0:
                mz = mw - 1.00727663
                adduct = "[M-H]-1"
                z = -1
            else:
                mz = mw + 1.00727663
                adduct = "[M+H]+1"
                z = 1
        
        return mz, z, adduct[:-1]
    
    
    def _export_results(self, results):
        """Exports results tables."""
        
        self.log(f"Exporting SIRIUS results...", "TEMP")
        
        # export top annotations
        annotations = results['SiriusTopAnnotations']
        if annotations is not None:
            path = os.path.join(self._workdir, "sirius_top_annotations")
            self._export_table_json(annotations, path)
        
        # export formulas
        formulas = results['SiriusFormulas']
        if formulas is not None:
            path = os.path.join(self._workdir, "sirius_formulas")
            self._export_table_json(formulas, path)
        
        # export classes
        classes = results['SiriusClasses']
        if classes is not None:
            path = os.path.join(self._workdir, "sirius_classes")
            self._export_table_json(classes, path)
        
        # export structures
        structures = results['SiriusStructures']
        if structures is not None:
            path = os.path.join(self._workdir, "sirius_structures")
            self._export_table_json(structures, path)
        
        # export denovo
        denovo = results['SiriusDeNovoStructures']
        if denovo is not None:
            path = os.path.join(self._workdir, "sirius_denovo_structures")
            self._export_table_json(denovo, path)
        
        # export fingerprints
        fingerprints = results['SiriusFingerprints']
        if fingerprints is not None:
            path = self._params["FingerprintsPath"] + "_fingerprints"
            self._export_table_csv(fingerprints, path, index=True)
        
        # export fingerprints defs
        definitions = results['SiriusFingerprintDefinitions']
        if definitions is not None:
            path = self._params["FingerprintsPath"] + "_FPkey"
            self._export_table_csv(definitions, path)
    
    
    def _export_table_csv(self, table, path, index=False):
        """Exports results pandas tables to CSV."""
        
        path = path + '.txt'
        
        table.to_csv(path,
            sep = '\t',
            encoding = 'utf-8',
            index = index,
            header = True,
            quoting = 1,
            na_rep = '')
    
    
    def _export_table_json(self, table, path):
        """Exports results pandas tables to JSON."""
        
        path = path + '.json'
        
        table.to_json(path,
            orient = 'records',
            lines = True)


if __name__ == '__main__':
    
    # run simple test search
    search = CDSirius(r"../Data/")
    search.run()
