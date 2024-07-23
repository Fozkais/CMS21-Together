using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;

namespace CMS21Together.Shared
{
    public static class ApiCalls
    {
        public static Dictionary<string, bool> API_M3()
        {
            Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(asm => asm.GetName().Name == "TogetherModAPI");
            string className = "TogetherModAPI.MainApi"; 
            
            Type myClassType = loadedAssembly.GetType(className);
            if (myClassType != null)
            {
                MethodInfo apiMethod1 = myClassType.GetMethod("StaticMethod3", BindingFlags.Static | BindingFlags.Public);
                if (apiMethod1 != null)
                {
                    return (Dictionary<string, bool>)apiMethod1.Invoke(null, null);
                }
            }

            return null;
        }
        
        public static void API_M2(object c)
        {
            Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(asm => asm.GetName().Name == "TogetherModAPI");
            string className = "TogetherModAPI.MainApi";  
            
            Type myClassType = loadedAssembly.GetType(className);
            if (myClassType != null)
            {
                MethodInfo apiMethod1 = myClassType.GetMethod("StaticMethod2", BindingFlags.Static | BindingFlags.Public);
                if (apiMethod1 != null)
                {
                    object[] parameters = new object[] { c };
                    
                    apiMethod1.Invoke(null, parameters);
                }
            }
        }
        
        public static Dictionary<string, bool> API_M1(object c, object h)
        {
            Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(asm => asm.GetName().Name == "TogetherModAPI");
            string className = "TogetherModAPI.MainApi";
            
            Type myClassType = loadedAssembly.GetType(className);
            if (myClassType != null)
            {
                MethodInfo apiMethod1 = myClassType.GetMethod("StaticMethod1", BindingFlags.Static | BindingFlags.Public);
                if (apiMethod1 != null)
                {
                    object[] parameters = new object[] { c, h };
                    
                    var a = apiMethod1.Invoke(null, parameters);
                    return (Dictionary<string, bool>)a;
                }
            }
            
            return null;
        }
        
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Stream stream = DataHelper.LoadContent("CMS21Together.Assets.TogetherModAPI.dll");

            byte[] assemblyData = new byte[stream.Length];
            stream.Read(assemblyData, 0, assemblyData.Length);
            
            return Assembly.Load(assemblyData);
        }
    }
}