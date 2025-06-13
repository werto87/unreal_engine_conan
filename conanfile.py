from conan import ConanFile
from conan.tools.files import save
import json
import glob
import re
import os

class UnrealDepsConan(ConanFile):
    name = "unreal_deps"
    version = "1.0"
    settings = "os", "compiler", "build_type", "arch"
    generators = "CMakeDeps"

    def requirements(self):
        self.requires("my_web_socket/0.1.3")

    def generate(self):
        # --- Direct dependencies ---
        direct_deps_info = []
        for dep_ref, dep in self.dependencies.direct_host.items():
            name = str(dep_ref).split(',')[0].split('/')[0]
            version = str(dep_ref).split(',')[0].split('/')[1]
            if "@" in version:
                version = version.split("@")[0]
            direct_deps_info.append({"name": name, "version": version})
        save(self, "DirectDeps.json", json.dumps(direct_deps_info))

        # --- All dependencies (direct + transitive) ---
        deps_info = []
        for dep_ref, dep in self.dependencies.items():
            name = str(dep_ref).split(',')[0].split('/')[0]
            version = str(dep_ref).split(',')[0].split('/')[1]
            if "@" in version:
                version = version.split("@")[0]
            deps_info.append({"name": name, "version": version})
        save(self, "TransitivAndDirectDeps.json", json.dumps(deps_info))

        # --- Extract Defines from Cmake ---
        pattern = os.path.join('**', '*-*-*-data.cmake')
        print(pattern)
        definitions_regex = re.compile(
            r'set\([^\s]+_(?:COMPILE_DEFINITIONS|DEFINITIONS)_[^\s]+\s+"([^"]+)"(?:\s+"([^"]+)")?\)',
            re.MULTILINE
        )

        results = set()
        for cmake_file in glob.glob(pattern, recursive=True):
            with open(cmake_file, 'r', encoding='utf-8') as f:
                content = f.read()
            for match in definitions_regex.finditer(content):
                for i in (1, 2):
                    val = match.group(i)
                    if val and not val.startswith('-D'):
                        results.add(val.strip())

        sorted_defines = sorted(results)
        save(self, "defines.json", json.dumps(sorted_defines, indent=2))
