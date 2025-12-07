#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# import modules
import io
import os
import sys
import time
import numpy
import pandas

import PySirius
from PySirius.models.feature_import import FeatureImport
from PySirius.models.formula_candidate import FormulaCandidate
from PySirius.models.structure_candidate import StructureCandidate

from silencer import Silencer


class SiriusConfig(object):
    """Defines configuration for SIRIUS search."""
    
    
    def __init__(self):
        """Initializes a new instance of SiriusConfig."""
        
        # main service
        self.service_path: str = r"C:\Program Files\Sirius\sirius.exe"
        self.service_port: int | None = 8080
        
        # project space
        self.project_name: str = ""
        self.project_path: str = ""
        
        # user account
        self.account_username: str = ""
        self.account_password: str = ""
        
        # formula search
        self.formula_id_enabled: bool = True
        self.formula_id_profile: str = "ORBITRAP"
        self.formula_id_mass_accuracy_ms2ppm: float = 5.0
        self.formula_id_filter_by_isotope_pattern: bool = True
        self.formula_id_enforce_el_gordo_formula: bool = True
        self.formula_id_perform_bottom_up_search: bool = True
        self.formula_id_perform_denovo_below_mz: float = 400.0
        self.formula_id_enforced_formula_constraints: str = "HCNOP[4]F[40]"
        self.formula_id_detectable_elements: list = ['B', 'S', 'Cl', 'Se', 'Br']
        self.formula_id_formula_search_dbs = None
        self.ms1_mass_deviation_allowed = 5.0
        
        # compound class search
        self.canopus_enabled: bool = True
        
        # fingerprints
        self.fingerprint_prediction_enabled: bool = True
        self.fingerprint_prediction_save: bool = False
        
        # structure search
        self.structure_db_search_enabled: bool = True
        self.structure_db_search_dbs: list = ['DSSTOX', 'PUBCHEM', 'HMDB', 'KEGG']
        self.structure_db_search_pubchem_fallback: bool = True
        self.structure_db_search_candidates: int = 10
        
        # denovo structure search
        self.ms_novelist_enabled: bool = True
        self.ms_novelist_candidates: int = 10


class Sirius(object):
    """SIRIUS search tool."""
    
    
    def __init__(self, config, logger=None):
        """Initializes a new instance of Sirius."""
        
        self._config: SiriusConfig = config
        self._api = None
        self._logger = logger
    
    
    def __enter__(self):
        """Implements context manager."""
        
        # silence errors from SIRIUS
        with Silencer(stdout=False, stderr=True):
            
            # connect to existing service
            if not self.connect():
                
                # start new service
                if not self.start():
                    raise RuntimeError("Unable to connect or start SIRIUS service!")
            
            # login
            if not self.login():
                raise RuntimeError("Unable to login to SIRIUS account!")
        
        return self
    
    
    def __exit__(self, exc_ty, exc_val, tb):
        """Implements context manager."""
        
        # silence errors from SIRIUS
        with Silencer(stdout=False, stderr=True):
            self.shutdown()
    
    
    def log(self, message, level="INFO"):
        """Logs processing message."""
        
        # use specified logger
        if self._logger is not None:
            self._logger(message, level)
            return
        
        # format level
        level = level.upper()
        
        # show all as standard output
        sys.stdout.write(f"{level}: {message}\n")
        sys.stdout.flush()
    
    
    def connect(self):
        """Initializes SIRIUS API by connecting to existing service."""
        
        self.log("Connecting to SIRIUS service...", "TEMP")
        
        # init API
        try:
            # init SDK
            sdk = PySirius.SiriusSDK()
            
            # find existing service
            self._api = sdk.attach_to_sirius(sirius_port=self._config.service_port)
            
            # check connection
            if self._api:
                self.log("Connected to running SIRIUS service.", "VERBOSE")
                return True
            else:
                self.log("Unable to find running SIRIUS service.", "VERBOSE")
                return False
        
        except Exception as err:
            self.log("Unable to connect to SIRIUS service!", "ERROR")
            raise err
    
    
    def start(self):
        """Initializes SIRIUS API by starting new service."""
        
        self.log("Starting new SIRIUS service...", "TEMP")
        
        # init API
        try:
            # init SDK
            sdk = PySirius.SiriusSDK()
            
            # start service
            self._api = sdk.start_sirius(
                sirius_path = os.path.abspath(self._config.service_path),
                port = self._config.service_port,
                headless = True)
            
            # wait for all to be initialized
            time.sleep(10)
            
            # check connection
            if self._api:
                self.log("SIRIUS service started.", "VERBOSE")
                return True
            else:
                self.log("Unable to start SIRIUS service.", "VERBOSE")
                return False
        
        except Exception as err:
            self.log("Unable to start SIRIUS service!", "ERROR")
            raise err
    
    
    def login(self):
        """Connecting SIRIUS account."""
        
        self.log("Connecting to SIRIUS user account...", "TEMP")
        
        # check if logged in
        if self._api.account().is_logged_in():
            self.log("SIRIUS account already connected.", "VERBOSE")
            return True
    
        # check if credentials provided
        if not self._config.account_username or not self._config.account_password:
            self.log("SIRIUS account credentials not specified!", "WARNING")
            return False
        
        # init login credentials
        credentials = PySirius.AccountCredentials().from_dict({
            'username': self._config.account_username,
            'password': self._config.account_password,
            'refreshToken': None})
        
        try:
            api_response = self._api.account().login(
                True,
                credentials,
                fail_when_logged_in = False,
                include_subs = False)
            
            self.log("SIRIUS account connected.", "VERBOSE")
            return True
        
        except Exception as err:
            self.log(f"Unable to login to SIRIUS account!", "ERROR")
            raise err
    
    
    def shutdown(self):
        """Shuts down SIRIUS service."""
        
        self.log("Shutting down SIRIUS service...", "TEMP")
        
        # check if other projects are running
        if self._api.projects().get_projects():
            self.log("SIRIUS service cannot be shut down due to running projects!", "WARNING")
            return
        
        # shut down the service
        try:
            response = PySirius.ActuatorApi(self._api.get_client()).shutdown_with_http_info()
            if response.status_code == 200:
                self.log("SIRIUS service stopped.", "VERBOSE")
        
        except Exception as err:
            self.log(f"Unable to shut down SIRIUS service!", "ERROR")
            raise err
    
    
    def init_project(self):
        """Initializes SIRIUS project in scratch folder."""
        
        self.log("Creating SIRIUS project...", "TEMP")
        
        # remove if exists
        if os.path.exists(self._config.project_path):
            os.remove(self._config.project_path)
        
        # init SIRIUS project
        try:
            project = self._api.projects().create_project(self._config.project_name, path_to_project=self._config.project_path)
            self.log(f"SIRIUS project space created.", "VERBOSE")
        
        except Exception as err:
            self.log(f"Unable to initialize SIRIUS project space '{self._config.project_name}' at '{self._config.project_path}'!", "ERROR")
            raise err
        
        return project
    
    
    def init_job(self):
        """Configures SIRIUS job."""
        
        self.log("Creating SIRIUS job...", "TEMP")
        
        try:
            # get default template
            job = self._api.jobs().get_default_job_config()
            
            # set params
            job.spectra_search_params.enabled = False
            job.formula_id_params.enabled = self._config.formula_id_enabled
            job.formula_id_params.profile = self._config.formula_id_profile
            job.formula_id_params.mass_accuracy_ms2ppm = self._config.formula_id_mass_accuracy_ms2ppm
            job.formula_id_params.filter_by_isotope_pattern = self._config.formula_id_filter_by_isotope_pattern
            job.formula_id_params.enforce_el_gordo_formula = self._config.formula_id_enforce_el_gordo_formula
            job.formula_id_params.perform_bottom_up_search = self._config.formula_id_perform_bottom_up_search
            job.formula_id_params.perform_denovo_below_mz = self._config.formula_id_perform_denovo_below_mz
            job.formula_id_params.enforced_formula_constraints = self._config.formula_id_enforced_formula_constraints
            job.formula_id_params.detectable_elements = self._config.formula_id_detectable_elements
            job.formula_id_params.formula_search_dbs = self._config.formula_id_formula_search_dbs
            job.formula_id_params.ilp_timeout = {"numberOfSecondsPerDecomposition": 0, "numberOfSecondsPerInstance": 0}
            
            job.fingerprint_prediction_params.enabled = self._config.fingerprint_prediction_enabled
            job.structure_db_search_params.enabled = self._config.structure_db_search_enabled
            job.structure_db_search_params.structure_search_dbs = self._config.structure_db_search_dbs
            
            if self._config.structure_db_search_pubchem_fallback:
                job.structure_db_search_params.expansive_search_confidence_mode = self._api.models().ConfidenceMode.APPROXIMATE
            else:
                job.structure_db_search_params.expansive_search_confidence_mode = self._api.models().ConfidenceMode.OFF
            
            if job.structure_db_search_params.enabled:
                job.canopus_params.enabled = self._config.canopus_enabled
                job.ms_novelist_params.enabled = self._config.ms_novelist_enabled
                job.ms_novelist_params.number_of_candidate_to_predict = self._config.ms_novelist_candidates
            else:
                job.canopus_params.enabled = False
                job.ms_novelist_params.enabled = False
            
            job.config_map = {'MS1MassDeviation.allowedMassDeviation': f"{self._config.ms1_mass_deviation_allowed} ppm"}
        
        except Exception as err:
            self.log("Unable to initialize SIRIUS job!", "ERROR")
            raise err
        
        return job
    
    
    def start_job(self, job, project):
        """Executes SIRIUS job."""
        
        self.log("Submitting SIRIUS job...", "TEMP")
        
        try:
            # submit job
            submission = self._api.jobs().start_job(project_id=project.project_id, job_submission=job)
            self.log("SIRIUS job started.", "VERBOSE")
        
        except Exception as err:
            self.log("Could not submit SIRIUS job!", "ERROR")
            raise err
        
        # wait while processing
        while True:
            
            # get current state
            state = self._api.jobs().get_job(project.project_id, submission.id).progress.state
            
            # show progress
            progress = self._api.jobs().get_job(project.project_id, submission.id).progress.current_progress
            progress_max = self._api.jobs().get_job(project.project_id, submission.id).progress.max_progress
            if progress_max:
                self.log(f"{progress/progress_max*100:.0f}", "PROGRESS")
            
            # job done
            if state == 'DONE':
                self.log("SIRIUS job completed successfully.", "VERBOSE")
                return True
            
            # job canceled
            if state == 'CANCELED':
                self.log("SIRIUS job cancelled.", "ERROR")
                return False
            
            # job failed
            if state == 'FAILED':
                self.log("SIRIUS job failed.", "ERROR")
                return False
            
            # just wait
            time.sleep(10)
    
    
    def close_project(self, project):
        """Deletes related jobs and closes specified project."""
        
        self.log("Closing SIRIUS jobs and project...", "TEMP")
        
        try:
            self._api.jobs().delete_jobs(project.project_id)
            self._api.projects().close_project(project.project_id)
        
        except Exception as err:
            self.log("Could not close the SIRIUS project!", "ERROR")
            raise err
    
    
    def close_all_projects(self):
        """Deletes related jobs and closes all projects."""
        
        self.log("Closing all SIRIUS jobs and projects...", "TEMP")
        
        # get all projects
        try:
            projects = self._api.projects().get_projects()
        
        except Exception as err:
            self.log(f"Could not get current SIRIUS projects!", "ERROR")
            raise err
            
        # delete jobs
        for project in projects:
            self.close_project(project)
    
    
    def set_features(self, project, features):
        """Set features into SIRIUS API."""
        
        self.log("Setting features to SIRIUS...", "TEMP")
        
        # init buff
        sirius_features = []
        
        # convert to SIRIUS feature
        try:
            for feat in features:
                sirius_feature = FeatureImport.from_dict(feat)
                sirius_features.append(sirius_feature)
        
        except Exception as err:
            self.log("Unable to convert features for SIRIUS service!", 'ERROR')
            raise err
        
        # set features to API
        try:
            self._api.features().add_aligned_features(
                project.project_id,
                sirius_features,
                profile = PySirius.InstrumentProfile("ORBITRAP"),
                opt_fields = ["msData"])
        
        except Exception as err:
            self.log("Unable to set features to SIRIUS service!", 'ERROR')
            raise err
    
    
    def retrieve_results(self, job, project):
        """Retrieves SIRIUS results."""
        
        self.log(f"Retrieving SIRIUS results...", "TEMP")
        
        # init results
        results = {
            'SiriusTopAnnotations': None,
            'SiriusFormulas': None,
            'SiriusClasses': None,
            'SiriusStructures': None,
            'SiriusDeNovoStructures': None,
            'SiriusFingerprints': None,
            'SiriusFingerprintDefinitions': None}
        
        # init buffs
        annotations = []
        formulas = []
        structures = []
        classes = []
        denovo = []
        class_tree_id = [1]
        
        # get feature IDs
        feature_ids = [fid.aligned_feature_id for fid in self._api.features().get_aligned_features(project.project_id)]
        
        # retrieve results
        for feat_id in feature_ids:
            
            # get feature info with top annotation information
            feature = self._api.features().get_aligned_feature(project.project_id, feat_id, opt_fields=["topAnnotations"])
            if feature.top_annotations.formula_annotation is None:
                continue
            
            # get external ID
            ext_id = feature.external_feature_id
            
            # get top formula ID
            top_formula_id = feature.top_annotations.formula_annotation.formula_id
            
            # retrieve top annotation
            top_annotation = self._retrieve_annotation(ext_id, feature)
            annotations.append(top_annotation)
            
            # retrieve all formula predictions
            table = self._retrieve_formulas(project, ext_id, feat_id, top_annotation, top_formula_id)
            if table is not None:
                formulas.append(table)
            
            # retrieve all compound class predictions
            tables = self._retrieve_classes(job, project, ext_id, feat_id, class_tree_id)
            if tables is not None:
                classes += tables
            
            # retrieve all CSI:FingerID database structure predictions
            table = self._retrieve_structures(job, project, ext_id, feat_id)
            if table is not None:
                structures.append(table)
            
            # retrieve all MSNovelist de novo structure predictions
            table = self._retrieve_denovo(job, project, ext_id, feat_id)
            if table is not None:
                denovo.append(table)
        
        # retrieve fingerprints
        predictions, definitions = self._retrieve_fingerprints(project, annotations)
        results['SiriusFingerprints'] = predictions
        results['SiriusFingerprintDefinitions'] = definitions
        
        # finalize tables
        res = self._finalize_results(annotations, formulas, classes, structures, denovo)
        results.update(res)
        
        return results
    
    
    def _retrieve_annotation(self, ext_id, feat):
        """Retrieves top annotation."""
        
        # init annotation
        annotation = {}
        
        # assign external ID
        annotation['ExternalID'] = ext_id
        
        # get feature name
        annotation['FeatureName'] = feat.name
        
        # get top formula annotation information
        annotation['Formula'] = feat.top_annotations.formula_annotation.molecular_formula
        annotation['FormulaScore'] = feat.top_annotations.formula_annotation.sirius_score
        
        # get top structure annotation
        has_structure = feat.top_annotations.structure_annotation is not None
        
        annotation['CSIFingerIDName'] = feat.top_annotations.structure_annotation.structure_name if has_structure else ""
        annotation['CSIFingerIDInChIKey'] = feat.top_annotations.structure_annotation.inchi_key if has_structure else ""
        annotation['CSIFingerIDScore'] = feat.top_annotations.structure_annotation.csi_score if has_structure else ""
        annotation['CSIFingerIDTanimotoSimilarity'] = feat.top_annotations.structure_annotation.tanimoto_similarity if has_structure else ""
        annotation['CSIFingerIDConfidenceExact'] = feat.top_annotations.confidence_exact_match if has_structure else ""
        annotation['CSIFingerIDConfidenceApprox'] = feat.top_annotations.confidence_approx_match if has_structure else ""
        
        # get top ClassyFire classification
        has_class = feat.top_annotations.compound_class_annotation is not None
        lineage = feat.top_annotations.compound_class_annotation.classy_fire_lineage if has_class else None
        lineage_len = len(feat.top_annotations.compound_class_annotation.classy_fire_lineage) if lineage else 0
        
        annotation['ClassyFireLevel1'] = feat.top_annotations.compound_class_annotation.classy_fire_lineage[0].name if lineage_len > 0 else ""
        annotation['ClassyFireLevel2'] = feat.top_annotations.compound_class_annotation.classy_fire_lineage[1].name if lineage_len > 1 else ""
        annotation['ClassyFireLevel3'] = feat.top_annotations.compound_class_annotation.classy_fire_lineage[2].name if lineage_len > 2 else ""
        annotation['ClassyFireLevel4'] = feat.top_annotations.compound_class_annotation.classy_fire_lineage[3].name if lineage_len > 3 else ""
        annotation['ClassyFireLevel5'] = feat.top_annotations.compound_class_annotation.classy_fire_lineage[4].name if lineage_len > 4 else ""
        annotation['ClassyFireLevel6'] = feat.top_annotations.compound_class_annotation.classy_fire_lineage[5].name if lineage_len > 5 else ""
        
        return annotation
    
    
    def _retrieve_formulas(self, project, ext_id, feat_id, top_annotation, top_formula_id):
        """Retrieves all SIRIUS formula predictions."""
        
        # get all candidates
        candidates = self._api.features().get_formula_candidates(project.project_id, feat_id, opt_fields=["statistics", "predictedFingerprint"])
        candidates_dicts = [FormulaCandidate.to_dict(d) for d in candidates]
        candidates_df = pandas.DataFrame.from_dict(candidates_dicts)
        
        # check candidates
        if candidates_df.empty:
            return None, None
        
        # assign external ID
        candidates_df['ExternalID'] = ext_id
        
        # set MS2 errors
        candidates_df['MS2ErrorPpm'] = pandas.DataFrame(candidates_df['medianMassDeviation'].tolist())['ppm']
        
        # assign top annotation fingerprint if requested
        if self._config.fingerprint_prediction_save:
            top_fingerprint = candidates_df.loc[candidates_df['formulaId'] == top_formula_id, 'predictedFingerprint'].item()
            top_annotation['topFingerprint'] = [] if top_fingerprint is None else top_fingerprint
        
        # drop predictedFingerprint field
        candidates_df.drop(['predictedFingerprint'], axis=1, inplace=True)
        
        # drop extraneous table fields
        candidates_df.drop(
            columns = [
                'medianMassDeviation',
                'compoundClasses',
                'siriusScoreNormalized',
                'zodiacScore',
                'fragmentationTree',
                'annotatedSpectrum',
                'isotopePatternAnnotation',
                'lipidAnnotation',
                'canopusPrediction'],
            inplace = True)
        
        # rename table fields for CD
        candidates_df.rename(
            columns = {
                'formulaId': 'SiriusFormulaID',
                'molecularFormula': 'Formula',
                'adduct': 'Adduct',
                'rank': 'Rank',
                'siriusScore': 'SiriusScore',
                'isotopeScore': 'IsotopeScore',
                'treeScore': 'TreeScore',
                'numOfExplainedPeaks': 'ExplainedPeaksCount',
                'numOfExplainablePeaks': 'ExplainablePeaksCount',
                'totalExplainedIntensity': 'ExplainedIntensity'},
            inplace = True)
        
        return candidates_df
    
    
    def _retrieve_classes(self, job, project, ext_id, feat_id, tree_id):
        """Retrieves all compound class predictions."""
        
        # check if enabled
        if not job.canopus_params.enabled:
            return None
        
        # get all candidates
        candidates = self._api.features().get_formula_candidates(project.project_id, feat_id, opt_fields=["compoundClasses"])
        candidates_dicts = [FormulaCandidate.to_dict(d) for d in candidates]
        
        # check candidates
        if not candidates_dicts:
            return None
        
        # get all compound class predictions if available
        classes = []
        for cmpd_class in candidates_dicts:
            
            # check class
            if cmpd_class['compoundClasses'] is None:
                continue
            
            # get all classes
            cmpd_class_df = pandas.DataFrame.from_dict(cmpd_class['compoundClasses']['classyFireLineage'])
            
            # assign external ID
            cmpd_class_df['ExternalID'] = ext_id
            
            # assign formula ID
            cmpd_class_df['SiriusFormulaID'] = cmpd_class['formulaId']
            
            # assign tree ID
            cmpd_class_df['TreeID'] = tree_id[0]
            tree_id[0] += 1
            
            # drop extraneous table fields
            cmpd_class_df.drop(
                columns = [
                    'type',
                    'index',
                    'parentId',
                    'parentName'],
                inplace = True)
            
            # rename table fields for CD
            cmpd_class_df.rename(
                columns = {
                    'id': 'ClassID',
                    'name': 'Name',
                    'level': 'Level',
                    'levelIndex': 'LevelIndex',
                    'description': 'Description',
                    'probability': 'Probability'},
                inplace = True)
            
            # store class
            classes.append(cmpd_class_df)
        
        return classes
    
    
    def _retrieve_structures(self, job, project, ext_id, feat_id):
        """Retrieves all CSI:FingerID database structure predictions."""
        
        # check if enabled
        if not job.structure_db_search_params.enabled:
            return None
        
        # get all candidates
        candidates = self._api.features().get_structure_candidates(project.project_id, feat_id, opt_fields=["dbLinks"])
        candidates_dicts = [StructureCandidate.to_dict(d) for d in candidates]
        candidates_df = pandas.DataFrame.from_dict(candidates_dicts)
        
        # check candidates
        if candidates_df.empty:
            return None
        
        # limit number of matches per compound
        max_rank = self._config.structure_db_search_candidates
        candidates_df.drop(candidates_df[candidates_df['rank'] > max_rank].index, inplace=True)
        
        # assign external ID
        candidates_df['ExternalID'] = ext_id
        
        # add DB links to main results
        db_links = self._retrieve_db_ids(candidates_df)
        link_df = pandas.DataFrame(db_links)
        candidates_df = pandas.concat([candidates_df, link_df], axis=1)
        
        # drop extraneous table fields
        candidates_df.drop([
            'dbLinks',
            'spectralLibraryMatches',
            'mcesDistToTopHit'],
            axis = 1,
            inplace = True)
        
        # rename table fields for CD
        candidates_df.rename(
            columns = {
                'formulaId': 'SiriusFormulaID',
                'molecularFormula': 'Formula',
                'inchiKey': 'InChIKey',
                'smiles': 'SMILES',
                'structureName': 'Name',
                'xlogP': 'LogKow',
                'rank': 'Rank',
                'csiScore': 'CSIScore',
                'tanimotoSimilarity': 'TanimotoSimilarity',
                'adduct': 'Adduct'},
            inplace = True)
        
        return candidates_df
    
    
    def _retrieve_denovo(self, job, project, ext_id, feat_id):
        """Retrieves all MSNovelist de novo structure predictions."""
        
        # check if enabled
        if not job.ms_novelist_params.enabled:
            return None
        
        # get all candidates
        candidates = self._api.features().get_de_novo_structure_candidates(project.project_id, feat_id, opt_fields=["dbLinks"])
        candidates_dicts = [StructureCandidate.to_dict(d) for d in candidates]
        candidates_df = pandas.DataFrame.from_dict(candidates_dicts)
        
        # check candidates
        if candidates_df.empty:
            return None
        
        # limit number of matches per compound
        max_rank = self._config.ms_novelist_candidates
        candidates_df.drop(candidates_df[candidates_df['rank'] > max_rank].index, inplace=True)
        
        # assign external ID
        candidates_df['ExternalID'] = ext_id
        
        # add DB links to main results
        db_links = self._retrieve_db_ids(candidates_df)
        link_df = pandas.DataFrame(db_links)
        candidates_df = pandas.concat([candidates_df, link_df], axis=1)
        
        # drop extraneous table fields
        candidates_df.drop([
            'dbLinks',
            'spectralLibraryMatches'],
            axis = 1,
            inplace = True)
        
        # rename table fields for CD
        candidates_df.rename(
            columns = {
                'formulaId': 'SiriusFormulaID',
                'molecularFormula': 'Formula',
                'inchiKey': 'InChIKey',
                'smiles': 'SMILES',
                'structureName': 'Name',
                'xlogP': 'LogKow',
                'rank': 'Rank',
                'csiScore': 'CSIScore',
                'tanimotoSimilarity': 'TanimotoSimilarity',
                'adduct': 'Adduct'},
            inplace = True)
        
        return candidates_df
    
    
    def _retrieve_fingerprints(self, project, annotations):
        """Retrieves all compound class predictions."""
        
        # check if enabled
        if not self._config.fingerprint_prediction_save:
            return None, None
        
        # get data
        data = self._api.projects().get_finger_id_data(project.project_id, 1)
        definitions = io.StringIO(data)
        definitions_df = pandas.read_csv(definitions, sep='\t')
        
        # get fingerprint predictions
        annotations_df = pandas.DataFrame.from_dict(annotations)
        annotations_df['ExternalID'] = annotations_df['ExternalID'].astype('Int64')
        predictions = annotations_df['topFingerprint'].tolist()
        predictions_df = pandas.DataFrame(predictions, index=annotations_df['ExternalID'])
        
        predictions_df.insert(
            loc = 0,
            column = 'SiriusFeatureName',
            value = annotations_df['FeatureName'].values)
        
        return predictions_df, definitions_df
    
    
    def _retrieve_db_ids(self, candidates_df):
        """Retrieves database IDs."""
        
        db_links = []
        for links in candidates_df['dbLinks']:
            
            ids = {}
            for db_name in self._config.structure_db_search_dbs:
                db_name = db_name.upper()
                db_id = next((d for d in links if d["name"].upper() == db_name), None)
                if db_id:
                    db_id = db_id['id']
                ids[db_name] = db_id
            db_links.append(ids)
        
        return db_links
    
    
    def _finalize_results(self, annotations, formulas, classes, structures, denovo):
        """Finalizes results tables."""
        
        self.log(f"Finalizing SIRIUS results...", "TEMP")
        
        # init results
        results = {}
        
        # finalize annotations
        table = self._finalize_annotations(annotations)
        results['SiriusTopAnnotations'] = table
        
        # finalize formulas
        table = self._finalize_formulas(formulas)
        results['SiriusFormulas'] = table
        
        # finalize classes
        table = self._finalize_classes(classes)
        results['SiriusClasses'] = table
        
        # finalize structures
        table = self._finalize_structures(structures)
        results['SiriusStructures'] = table
        
        # finalize denovo
        table = self._finalize_denovo(denovo)
        results['SiriusDeNovoStructures'] = table
        
        return results
    
    
    def _finalize_annotations(self, annotations):
        """Finalizes all top annotations."""
        
        # check items
        if not annotations:
            return None
        
        # finalize table
        table = pandas.DataFrame.from_dict(annotations)
        table['ExternalID'] = table['ExternalID'].astype('Int64')
        table['CSIFingerIDScore'] = pandas.to_numeric(table['CSIFingerIDScore'], errors='coerce')
        table['CSIFingerIDTanimotoSimilarity'] = pandas.to_numeric(table['CSIFingerIDTanimotoSimilarity'], errors='coerce')
        table['CSIFingerIDConfidenceExact'] = pandas.to_numeric(table['CSIFingerIDConfidenceExact'], errors='coerce')
        table['CSIFingerIDConfidenceApprox'] = pandas.to_numeric(table['CSIFingerIDConfidenceApprox'], errors='coerce')
        
        # replace invalid numbers
        table.replace([numpy.inf, -numpy.inf], numpy.nan, inplace=True)
        
        # remove fingerprints
        if 'topFingerprint' in table:
            table.drop(['topFingerprint'], axis=1, inplace=True)
        
        return table
    
    
    def _finalize_formulas(self, formulas):
        """Finalizes all SIRIUS formula predictions."""
        
        # check items
        if not formulas:
            return None
        
        # finalize table
        table = pandas.concat(formulas, ignore_index=True)
        table['SiriusFormulaID'] = table['SiriusFormulaID'].astype('Int64')
        table['ExternalID'] = table['ExternalID'].astype('Int64')
        table['Rank'] = table['Rank'].astype('Int64')
        
        return table
    
    
    def _finalize_classes(self, classes):
        """Finalizes all compound class predictions."""
        
        # check items
        if not classes:
            return None
        
        # finalize table
        table = pandas.concat(classes, ignore_index=True)
        table['SiriusFormulaID'] = table['SiriusFormulaID'].astype('Int64')
        table['ExternalID'] = table['ExternalID'].astype('Int64')
        table['ClassID'] = table['ClassID'].astype('Int64')
        
        return table
    
    
    def _finalize_structures(self, structures):
        """Finalizes all CSI:FingerID database structure predictions."""
        
        # check items
        if not structures:
            return None
        
        # finalize table
        table = pandas.concat(structures, ignore_index=True)
        table['SiriusFormulaID'] = table['SiriusFormulaID'].astype('Int64')
        table['ExternalID'] = table['ExternalID'].astype('Int64')
        table['Rank'] = table['Rank'].astype('Int64')
        
        return table
    
    
    def _finalize_denovo(self, structures):
        """Finalizes all MSNovelist de novo structure predictions."""
        
        # check items
        if not structures:
            return None
        
        # finalize table
        table = pandas.concat(structures, ignore_index=True)
        table['SiriusFormulaID'] = table['SiriusFormulaID'].astype('Int64')
        table['ExternalID'] = table['ExternalID'].astype('Int64')
        table['Rank'] = table['Rank'].astype('Int64')
        
        return table


if __name__ == '__main__':
    
    # init config
    config = SiriusConfig()
    config.service_path = r"C:\Sirius\sirius.exe"
    config.service_port = 8080
    
    # closes all projects and shutdown the service
    with Sirius(config) as sirius:
        sirius.close_all_projects()
        sirius.shutdown()
