using System.Collections.Generic;
using System;
using System.Reflection;

namespace EventSystem{
    public static class PredefinedAssemblyUtil
    {
        enum AssemblyType
        {
            AssemblyCSharp,
            AssemblyCSharpEditor,
            AssemblyCSharpFirstPass,
            AssemblyCSharpFirstPassEditor,
        }

        static AssemblyType? GetAssemblyType(string assemblyName)
        {
            return assemblyName switch
            {
                "Assembly-CSharp" => AssemblyType.AssemblyCSharp,
                "Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
                "Assembly-CSharp-firstPass" => AssemblyType.AssemblyCSharpFirstPass,
                "Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpFirstPassEditor,
                _ => null
            };
        }

        static void AddTypesFromAssembly(Type[] assembly, ICollection<Type> types, Type interfaceType)
        {
            if (assembly == null) return;
            for(int i = 0; i < assembly.Length; i++)
            {
                Type type = assembly[i];
                if(type != interfaceType && interfaceType.IsAssignableFrom(type))
                {
                    types.Add(type);
                }
            }
        }

        /// <summary>
        /// Retrieves a list of types that implement or inherit from the specified interface or base type.
        /// </summary>
        /// <remarks>This method scans the assemblies loaded in the current application domain and filters
        /// the types based on the specified <paramref name="interfaceType"/>. Only types from specific assemblies
        /// (e.g., Assembly-CSharp and Assembly-CSharp-FirstPass) are considered.</remarks>
        /// <param name="interfaceType">The interface or base type to search for. This parameter cannot be <see langword="null"/>.</param>
        /// <returns>A list of <see cref="Type"/> objects representing the types that implement or inherit from the specified
        /// <paramref name="interfaceType"/>. The list will be empty if no matching types are found.</returns>
        public static List<Type> GetTypes(Type interfaceType)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            Dictionary<AssemblyType, Type[]> assemblyTypes = new Dictionary<AssemblyType, Type[]>();
            List<Type> types = new List<Type>();

            for(int i = 0; i < assemblies.Length; i++)
            {
                AssemblyType? assemblyType = GetAssemblyType(assemblies[i].GetName().Name);
                if(assemblyType != null)
                {
                    assemblyTypes.Add((AssemblyType)assemblyType, assemblies[i].GetTypes());
                }
            }

            AddTypesFromAssembly(assemblyTypes.GetValueOrDefault(AssemblyType.AssemblyCSharp), types, interfaceType);
            AddTypesFromAssembly(assemblyTypes.GetValueOrDefault(AssemblyType.AssemblyCSharpFirstPass), types, interfaceType);

            return types;
        }
    }
}
