#if UNITY_EDITOR
using System.IO;
using UnityEditor;

public class SlnGenerator : AssetPostprocessor
{
    private static void OnGeneratedCSProjectFiles()
    {
        string projectDir = Directory.GetCurrentDirectory();
        string projectName = Path.GetFileName(projectDir);
        string slnPath = Path.Combine(projectDir, projectName + ".sln");

        string slnContent = @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31612.314
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""Assembly-CSharp"", ""Assembly-CSharp.csproj"", ""{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
	EndGlobalSection
EndGlobal";

        File.WriteAllText(slnPath, slnContent);
    }
}
#endif