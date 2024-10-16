using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CMS21Together.Shared;

public static class ApiCalls
{
	public static Dictionary<string, bool> API_M3()
	{
		var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(asm => asm.GetName().Name == "TogetherModAPI");
		var className = "TogetherModAPI.MainApi";

		var myClassType = loadedAssembly.GetType(className);
		if (myClassType != null)
		{
			var apiMethod1 = myClassType.GetMethod("StaticMethod3", BindingFlags.Static | BindingFlags.Public);
			if (apiMethod1 != null) return (Dictionary<string, bool>)apiMethod1.Invoke(null, null);
		}

		return null;
	}

	public static void API_M2(object c)
	{
		var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(asm => asm.GetName().Name == "TogetherModAPI");
		var className = "TogetherModAPI.MainApi";

		var myClassType = loadedAssembly.GetType(className);
		if (myClassType != null)
		{
			var apiMethod1 = myClassType.GetMethod("StaticMethod2", BindingFlags.Static | BindingFlags.Public);
			if (apiMethod1 != null)
			{
				object[] parameters = { c };

				apiMethod1.Invoke(null, parameters);
			}
		}
	}

	public static Dictionary<string, bool> API_M1(object c, object h)
	{
		var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(asm => asm.GetName().Name == "TogetherModAPI");
		var className = "TogetherModAPI.MainApi";

		var myClassType = loadedAssembly.GetType(className);
		if (myClassType != null)
		{
			var apiMethod1 = myClassType.GetMethod("StaticMethod1", BindingFlags.Static | BindingFlags.Public);
			if (apiMethod1 != null)
			{
				object[] parameters = { c, h };

				var a = apiMethod1.Invoke(null, parameters);
				return (Dictionary<string, bool>)a;
			}
		}

		return null;
	}

	public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		var stream = DataHelper.LoadContent("CMS21Together.Assets.TogetherModAPI.dll");

		var assemblyData = new byte[stream.Length];
		stream.Read(assemblyData, 0, assemblyData.Length);

		return Assembly.Load(assemblyData);
	}
}