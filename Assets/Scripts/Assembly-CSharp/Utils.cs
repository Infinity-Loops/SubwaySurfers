using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Utils
{
	[CompilerGenerated]
	private static Converter<Component, string> _003C_003Ef__am_0024cache0;

	public static T FindObject<T>(this MonoBehaviour obj) where T : class
	{
		T val = UnityEngine.Object.FindObjectOfType(typeof(T)) as T;
		if (val == null)
		{
			Debug.LogWarning(string.Format("Game object '{0}' could not find object of type {1}.", obj.gameObject.name, typeof(T).Name));
		}
		return val;
	}

	public static T FindComponentInParents<T>(this MonoBehaviour obj) where T : Component
	{
		return FindComponentInThisOrParents<T>(obj.transform.parent);
	}

	public static T FindComponentInThisOrParents<T>(Transform t) where T : Component
	{
		Transform transform = t;
		while (transform != null)
		{
			T component = t.GetComponent<T>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return (T)null;
	}

	public static string GetLongName(Transform transform)
	{
		return (!(transform == null)) ? (GetLongName(transform.parent) + "/" + transform.name) : string.Empty;
	}

	public static string GetLongNameList(Component[] components)
	{
		List<Component> list = new List<Component>(components);
		if (_003C_003Ef__am_0024cache0 == null)
		{
			_003C_003Ef__am_0024cache0 = _003CGetLongNameList_003Em__43;
		}
		return string.Join(", ", list.ConvertAll(_003C_003Ef__am_0024cache0).ToArray());
	}

	public static void Bar(string text, float ratio, int offset, Color color)
	{
		float num = 10f;
		float num2 = 20f;
		GUI.color = color;
		GUI.Button(new Rect(num, (float)Screen.height - num2 - num - (float)offset * num2, ((float)Screen.width - 2f * num) * ratio, num2), text);
	}

	[CompilerGenerated]
	private static string _003CGetLongNameList_003Em__43(Component c)
	{
		return GetLongName(c.transform);
	}
}
