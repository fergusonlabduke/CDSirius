#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# import modules
import sys
from node import CDSirius


if __name__ == '__main__':
    
    # execute the search
    search = CDSirius(sys.argv[1])
    search.run()
