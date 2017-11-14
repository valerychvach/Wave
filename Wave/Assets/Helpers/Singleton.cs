using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static object _lock = new object();

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			lock (_lock)
			{
				if (_instance == null)
				{
					_instance = (T)FindObjectOfType(typeof(T));
					bool isOnScene = _instance != null;
					if (!Application.isPlaying)
					{
						_instance = null;
					}
					if (FindObjectsOfType(typeof(T)).Length > 1)
					{
						Debug.LogError("[Singleton] Something went really wrong " +
							" - there should never be more than 1 singleton!" +
							" Reopening the scene might fix it.");
						return _instance;
					}
					if (_instance == null)
					{
						if (!isOnScene)
						{
							GameObject singleton = new GameObject();
							_instance = singleton.AddComponent<T>();
							singleton.name = AddSpacesToSentence(typeof(T).ToString(), true);
						}
						if (Application.isPlaying)
						{
							DontDestroyOnLoad(_instance);
						}
						Debug.Log("[Singleton] An instance of " + typeof(T) +
							" is needed in the scene, so '" + _instance +
							"' was created with DontDestroyOnLoad.");
					}
//					else
//					{
//						Debug.Log("[Singleton] Using instance already created: " +
//							_instance.gameObject.name);
//					}
				}
				return _instance;
			}
		}
	}

	private static bool applicationIsQuitting = false;

	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it have been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	public void OnDestroy()
	{
		applicationIsQuitting = true;
	}

	private static string AddSpacesToSentence(string text, bool preserveAcronyms)
	{
		if (text == "")
			return "";
		System.Text.StringBuilder newText = new System.Text.StringBuilder(text.Length * 2);
		newText.Append(text[0]);
		for (int i = 1; i < text.Length; i++)
		{
			if (char.IsUpper(text[i]))
			if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
			    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
			    i < text.Length - 1 && !char.IsUpper(text[i + 1])))
				newText.Append(' ');
			newText.Append(text[i]);
		}
		return newText.ToString();
	}
}