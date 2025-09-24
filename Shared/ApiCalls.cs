using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Shared;

public static class ApiCalls
{
	private static readonly Assembly APIAssembly = LoadAssembly();
	public static readonly bool useSteam = API_M4();
	public static Dictionary<string, bool> API_M3()
	{
		var className = "TogetherModAPI.MainApi";

		var myClassType = APIAssembly.GetType(className);
		if (myClassType != null)
		{
			var apiMethod1 = myClassType.GetMethod("StaticMethod3", BindingFlags.Static | BindingFlags.Public);
			if (apiMethod1 != null) return (Dictionary<string, bool>)apiMethod1.Invoke(null, null);
		}

		return null;
	}
	
	public static bool API_M4()
	{
		var className = "TogetherModAPI.MainApi";

		var myClassType = APIAssembly.GetType(className);
		if (myClassType != null)
		{
			var apiMethod1 = myClassType.GetMethod("StaticMethod4", BindingFlags.Static | BindingFlags.Public);
			if (apiMethod1 != null)
			{
				return (bool)apiMethod1.Invoke(null, null);
			}
		}
		return true;
	}

	public static void API_M2(object c)
	{
		var className = "TogetherModAPI.MainApi";

		var myClassType = APIAssembly.GetType(className);
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
		var className = "TogetherModAPI.MainApi";

		var myClassType = APIAssembly.GetType(className);
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

	public static Assembly LoadAssembly()
	{
		var stream = DataHelper.LoadContent("CMS21Together.Assets.TogetherModAPI.dll");

		if (stream == null)
		{
			MelonLogger.Msg("Failed to load resource stream for 'TogetherModAPI.dll'.");
			return null;
		}
		
		var assemblyData = new byte[stream.Length];
		stream.Read(assemblyData, 0, assemblyData.Length);

		return Assembly.Load(assemblyData);
	}
}