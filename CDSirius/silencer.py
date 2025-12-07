#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# import modules
import os


class Silencer(object):
    """Context manager to suppress stdout and/or stderr."""
    
    
    def __init__(self, stdout=True, stderr=True):
        """Initializes a new instance of Silencer."""
        
        self._stdout = stdout
        self._stdout_null = os.open(os.devnull, os.O_RDWR)
        self._stdout_save = os.dup(1)
        
        self._stderr = stderr
        self._stderr_null = os.open(os.devnull, os.O_RDWR)
        self._stderr_save = os.dup(2)
    
    
    def __enter__(self):
        """Implements context manager."""
        
        if self._stdout:
            os.dup2(self._stdout_null, 1)
        
        if self._stderr:
            os.dup2(self._stderr_null, 2)
    
    
    def __exit__(self, *_):
        """Implements context manager."""
        
        os.dup2(self._stdout_save, 1)
        os.dup2(self._stderr_save, 2)
        
        os.close(self._stdout_null)
        os.close(self._stdout_save)
        os.close(self._stderr_null)
        os.close(self._stderr_save)
