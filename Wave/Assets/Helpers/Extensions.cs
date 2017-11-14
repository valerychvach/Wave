using System;
using System.Collections.Generic;


public static class Extensions {

	public static void InvokeSafely(this Action action){
		if (action != null)
		{
			action.Invoke();
		}
	}
	public static void InvokeSafely<T1>(this Action<T1> action, T1 property_1){
		if (action != null)
		{
			action.Invoke(property_1);
		}
	}

	public static void InvokeSafely<T1, T2>(this Action<T1, T2> action, T1 property_1, T2 property_2){
		if (action != null)
		{
			action.Invoke(property_1, property_2);
		}
	}

	public static void InvokeSafely<T1, T2, T3>(this Action<T1, T2, T3> action, T1 property_1, T2 property_2, T3 property_3){
		if (action != null)
		{
			action.Invoke(property_1, property_2, property_3);
		}
	}
}
