using System;
using System.Collections.Generic;
using System.Reflection;

namespace DeepDreams.Utils
{
    public static class UserDefinedAssemblies
    {
        private static readonly List<Assembly> _unityCompiledAssemblies;

        // We cannot use UnityEditor.CompilationPipeline to automate this programatically because that class is not available in release builds.
        // Maybe there is still a way to do this programatically in release builds but I haven't found a solution.
        private static readonly HashSet<string> internalAssemblyNames = new HashSet<string>
        {
            "Bee.BeeDriver",
            "ExCSS.Unity",
            "Mono.Security",
            "mscorlib",
            "netstandard",
            "Newtonsoft.Json",
            "Newtonsoft.Json.UnityConverters",
            "Newtonsoft.Json.UnityConverters.Editor",
            "nunit.framework",
            "ReportGeneratorMerged",
            "Unrelated",
            "SyntaxTree.VisualStudio.Unity.Bridge",
            "SyntaxTree.VisualStudio.Unity.Messaging",
            "CameraComposition",
            "SolidUtilities",
            "SolidUtilities.Editor",
            "BakeryRuntimeAssembly",
            "BakeryEditorAssembly",
            "RBG.Mulligan",
            "Cinemachine",
            "com.unity.cinemachine.editor",
            "Ems.MainSceneAutoLoading.Editor",
            "PPv2URPConverters",
            "Wooshii.HierarchyDecorator",
            "Wooshii.HierarchyDecorator.Editor",
            "FastScriptReload.Editor",
            "FastScriptReload.Runtime",
            "MyBox",
            "Needle.CompilationVisualizer",
            "FMODUnity",
            "FMODUnityEditor",
            "FMODUnityResonance",
            "FMODUnityResonanceEditor",
            "ConsolePro.Editor",
            "ImmersiveVRTools.Common.Runtime",
            "ImmersiveVRTools.Common.Editor",
            "0Harmony",
            "Microsoft.CodeAnalysis.CSharp",
            "Microsoft.CodeAnalysis",
            "PlayerBuildProgramLibrary.Data",
            "WebGLPlayerBuildProgram.Data",
            "HarmonySharedState",
            "JetBrains.Rider.Unity.Editor.Plugin.Net46.Repacked",
            "Anonymously Hosted DynamicMethods Assembly"
        };

        static UserDefinedAssemblies()
        {
            _unityCompiledAssemblies = new List<Assembly>();
        }

        public static List<Assembly> GetUserCreatedAssemblies()
        {
            if (_unityCompiledAssemblies.Count == 0)
            {
                return FindUserCreatedAssemblies();
            }

            return _unityCompiledAssemblies;
        }

        private static List<Assembly> FindUserCreatedAssemblies()
        {
            var systemAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly systemAssembly in systemAssemblies)
            {
                if (systemAssembly.GetName().Name.StartsWith("System") ||
                    systemAssembly.GetName().Name.StartsWith("Unity") ||
                    systemAssembly.GetName().Name.StartsWith("UnityEditor") ||
                    systemAssembly.GetName().Name.StartsWith("UnityEngine") ||
                    internalAssemblyNames.Contains(systemAssembly.GetName().Name))
                {
                    continue;
                }

                _unityCompiledAssemblies.Add(systemAssembly);
            }

            return _unityCompiledAssemblies;
        }
    }
}