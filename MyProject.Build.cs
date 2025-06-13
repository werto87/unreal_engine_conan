using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnrealBuildTool;


public class MyProject : ModuleRules
{
	public MyProject(ReadOnlyTargetRules Target) : base(Target)
	{
		bEnableExceptions = true;
		CppStandard = CppStandardVersion.Cpp20;
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "EnhancedInput" });
		string pathToConanBuild=Path.Combine(ModuleDirectory, "..", "..", "conanbuild");
		string fullDeployPath = Path.Combine(pathToConanBuild, "full_deploy", "host");
		foreach (var nameAndVersion in ParseUnrealDepsJson(Path.Combine(pathToConanBuild, "DirectDeps.json")))
		{
			AddLibrary(fullDeployPath, nameAndVersion.name, nameAndVersion.version);
		}
		string transitivAndDirectDeps = Path.Combine(pathToConanBuild, "TransitivAndDirectDeps.json");
		foreach (var nameAndVersion in ParseUnrealDepsJson(transitivAndDirectDeps))
		{
			AddInclude(fullDeployPath, nameAndVersion.name, nameAndVersion.version);
		}
		string DefinesFile = Path.Combine(pathToConanBuild, "defines.json");
		if (File.Exists(DefinesFile))
		{
			try
			{
				string[] lines = File.ReadAllText(DefinesFile)
					.Trim('[', ']') // remove brackets
					.Split(',')     // split by comma
					.Select(s => s.Trim().Trim('"')) // remove whitespace and quotes
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.ToArray();

				foreach (var define in lines)
				{
					PublicDefinitions.Add(define);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error reading defines.json: {ex.Message}");
			}
		}
		else
		{
			Console.WriteLine(ModuleDirectory);
			Console.WriteLine("NO DEFINE JSON");
		}
	}
	private void AddInclude(string fullDeployPath, string name, string version, string config = "Release", string arch = "x86_64")
	{
		string BasePath = Path.Combine(fullDeployPath, name, version, config, arch);
		string IncludePath = Path.Combine(BasePath, "include");		
		if (Directory.Exists(IncludePath))
		{
			Console.WriteLine($"{name} FOUND");
			PublicIncludePaths.Add(IncludePath);
		}
		else
		{
			string HeaderOnlyPath = Path.Combine(fullDeployPath, name, version,"include");
			if (Directory.Exists(HeaderOnlyPath))
			{
				Console.WriteLine($"{name} FOUND");
				PublicIncludePaths.Add(HeaderOnlyPath);
			}
			else
			{
				Console.WriteLine($"{name} NOT FOUND");
				Console.WriteLine($"{name} Looked in");
				Console.WriteLine(IncludePath);
				Console.WriteLine(HeaderOnlyPath);
			}

		}
	}

	private void AddLibrary(string fullDeployPath, string name, string version, string config = "Release", string arch = "x86_64")
	{
		string BasePath = Path.Combine(fullDeployPath, name, version, config, arch);
		string LibFolderPath = Path.Combine(BasePath, "lib");
		if (Directory.Exists(LibFolderPath))
		{
			Console.WriteLine($"{name} library folder FOUND");
			string[] files = Directory.GetFiles(LibFolderPath);
			foreach (string file in files)
			{
				PublicAdditionalLibraries.Add(file);
			}
		}
		else
		{
			Console.WriteLine($"{name} library folder NOT FOUND");
			Console.WriteLine(LibFolderPath);
		}
	}

	private class NameAndVersion
	{
		public string name;
		public string version;
	}

	private List<NameAndVersion> ParseUnrealDepsJson(string depsPath)
	{
		var result = new List<NameAndVersion>();
		if (File.Exists(depsPath))
		{
			string json = File.ReadAllText(depsPath);
			int index = 0;
			while ((index = json.IndexOf("\"name\":", index)) != -1)
			{
				int nameStart = json.IndexOf('"', index + 7) + 1;
				int nameEnd = json.IndexOf('"', nameStart);
				string name = json.Substring(nameStart, nameEnd - nameStart);

				index = json.IndexOf("\"version\":", nameEnd);
				int versionStart = json.IndexOf('"', index + 10) + 1;
				int versionEnd = json.IndexOf('"', versionStart);
				string version = json.Substring(versionStart, versionEnd - versionStart);

				Console.WriteLine($"{name},{version}");
				
				result.Add(new NameAndVersion(){name= name, version=version} );
				index = versionEnd;
			}
		}
		return result;
	}
}
