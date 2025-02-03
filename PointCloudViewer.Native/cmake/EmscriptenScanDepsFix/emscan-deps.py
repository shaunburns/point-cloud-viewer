#!/usr/bin/env python3
# Copyright 2024 The Emscripten Authors.  All rights reserved.
# Emscripten is available under two separate licenses, the MIT license and the
# University of Illinois/NCSA Open Source License.  Both these licenses can be
# found in the LICENSE file.

"""emscan-deps - clang-scan-deps helper script

This script acts as a frontend replacement for clang-scan-deps.
"""
import sys
import os
from os.path import join
sys.path.insert(0, join(os.environ["EMSDK"], "upstream", "emscripten"))
from tools import shared, cache

CLANG_SCAN_DEPS = shared.build_llvm_tool_path(shared.exe_suffix('clang-scan-deps'))

args = sys.argv[1:]
args.append('--sysroot=' + cache.get_sysroot(absolute=True))
shared.exec_process([CLANG_SCAN_DEPS] + args)
