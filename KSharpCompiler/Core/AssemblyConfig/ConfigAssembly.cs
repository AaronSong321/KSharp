using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler.Core.AssemblyConfig
{
    public class ConfigAssembly
    {
        public static dyobj RuntimeConfigDev(AssemblyDefinition assembly)
        {
            dynamic rt = new dyobj();
            rt.runtimeOptions = new dyobj();
            rt.runtimeOptions.addtionalProbingPaths = new[] { "~/.dotnet/store/|arch|/|tfm|", "~/.nuget/packages" };
            return rt;
        }
        public static dyobj RuntimeConfig(AssemblyDefinition assembly)
        {
            dynamic rt = new dyobj();
            rt.runtimeOptions = new dyobj();
            rt.runtimeOptions.tfm = "netcorecpp3.1";
            rt.framework = new dyobj();
            rt.framework.name = "Microsoft.NETCore.App";
            rt.framework.version = "3.1.0";
            return rt;
        }
        static string AssemblyString(AssemblyNameReference assemblyName)
        {
            return $"{assemblyName.Name}/{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
        }
        static string AssemblyPath(AssemblyNameReference assemblyName)
        {
            return $"{assemblyName.Name.ToLower()}/{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
        }
        
        public static dyobj Deps(AssemblyDefinition assembly)
        {
            dynamic rt = new dyobj();
            rt.runtimeTarget = new dyobj();
            rt.runtimeTarget.name = ".NETCoreApp,Version=v3.1";
            rt.runtimeTarget.signature = "";
            rt.compilationOptions = new dyobj();
            rt.targets = new dyobj();
            rt.libraries = new dyobj();
            
            string assemblyNameKey = AssemblyString(assembly.Name);
            dynamic assemblyNameValue = new dyobj();
            assemblyNameValue.type = "project";
            assemblyNameValue.serviceable = false;
            assemblyNameValue.sha512 = "";
            rt.libraries.dictionary[assemblyNameKey] = assemblyNameValue;

            foreach (var re in assembly.MainModule.AssemblyReferences) {
                string nk = AssemblyString(re);
                dynamic nv = new dyobj();
                nv.type = "package";
                nv.serviceable = true;
                nv.sha512 = re.Hash;
                nv.path = AssemblyPath(re);
                nv.hashPath = $"{re.Name.ToLower()}.{re.Version.Major}.{re.Version.Minor}.{re.Version.Build}.nupkg.sha512";
                rt.libraries.dictionary[nk] = nv;
            }
            return rt;
        }
    }
}
