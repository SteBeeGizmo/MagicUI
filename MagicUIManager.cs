using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicUIManager : Singleton<MagicUIManager>
{
	#region Singleton<T> boilerplate
	void Awake()
	{
		_sortedLayouts = new Dictionary<AspectRatioLayouts, Dictionary<float, string>>();
		_notificationTargets = new Dictionary<string, List<MagicUIControl>>();

		setupDeviceType();
		setupStringTable();
		setupValuesTable();

		onAwake();
	}

	protected override bool isGlobalScope
	{
		get
		{
			return false;
		}
	}

	void OnDestroy()
	{
		onDestroy();
	}
	#endregion

	protected UIRoot _root;
	protected Dictionary<string, string> _valuesTable;

	void Start()
	{
		_root = GetComponent<UIRoot>();

		setupApi(onAPISetup);
	}

	public float Aspect
	{
		get { return _aspect; }
	}

	public bool IsPortrait
	{
		get { return _isPortrait; }
	}

	public Vector2 ScreenSize
	{
		get
		{
			Vector2 result = Vector2.zero;
			result.y = _root.activeHeight;

			if (_isPortrait)
				result.x = result.y / _aspect;
			else
				result.x = result.y * _aspect;

			return result;
		}
	}

	#region Aspect ratios
	[System.Serializable]
	public class AspectRatioLayouts
	{
		public string Aspect4x3;
		public string Aspect3x2;
		public string Aspect16x10;
		public string Aspect5x3;
		public string Aspect16x9;
	}
	
	public AspectRatioLayouts PhoneLandscapeLayouts;
	public AspectRatioLayouts TabletLandscapeLayouts;
	public AspectRatioLayouts PhonePortraitLayouts;
	public AspectRatioLayouts TabletPortraitLayouts;

	protected float _aspect;
	protected bool _isRetina;
	protected bool _isTablet;
	protected bool _isPortrait;
	protected float _dpi;
	protected float _diagonal;

	protected void setupDeviceType()
	{
		_isPortrait = false;
		float width = Screen.width;
		float height = Screen.height;
		if (height > width)
		{
			_isPortrait = true;
			width = height;
			height = Screen.width;
		}

		_aspect = width / height;

#if UNITY_STANDALONE || UNITY_WEBPLAYER
		_dpi = 72.0f;
#else
		_dpi = 150.0f; 	// Safe default
#endif

		if (Screen.dpi > 25 && Screen.dpi < 500)	// Keep within sane bounds
		{
			_dpi = Screen.dpi;
		}

		_diagonal = Mathf.Sqrt((Screen.width * Screen.width) + (Screen.height * Screen.height)) / _dpi;
		_isTablet = (_diagonal > 6.49f);

		_isRetina = (_dpi >= 260);

		_skin = bestSkinForDPI();
	}

	protected Dictionary<AspectRatioLayouts, Dictionary<float, string>> _sortedLayouts;
	protected string bestLayoutForAspect(AspectRatioLayouts layouts)
	{
		Dictionary<float, string> list = null;
		if (!_sortedLayouts.ContainsKey(layouts))
		{
			list = new Dictionary<float, string>();

			if (!string.IsNullOrEmpty(layouts.Aspect4x3))
				list.Add(4.0f / 3.0f, layouts.Aspect4x3);
			if (!string.IsNullOrEmpty(layouts.Aspect3x2))
				list.Add(3.0f / 2.0f, layouts.Aspect3x2);
			if (!string.IsNullOrEmpty(layouts.Aspect16x10))
				list.Add(16.0f / 10.0f, layouts.Aspect16x10);
			if (!string.IsNullOrEmpty(layouts.Aspect5x3))
				list.Add(5.0f / 3.0f, layouts.Aspect5x3);
			if (!string.IsNullOrEmpty(layouts.Aspect16x9))
				list.Add(16.0f / 9.0f, layouts.Aspect16x9);

			_sortedLayouts.Add(layouts, list);
		}
		else
			list = _sortedLayouts[layouts];

		string result = null;

		float dist = float.PositiveInfinity;

		foreach (float key in list.Keys)
		{
			float test = Mathf.Abs(key - _aspect);
			if (test < dist)
			{
				result = list[key];
				dist = test;
			}
		}

		return result;
	}

	protected string bestLayoutForAspect(bool portrait)
	{
		string result = null;
		
		if (_isTablet)
		{
			result = bestLayoutForAspect(portrait ? TabletPortraitLayouts : TabletLandscapeLayouts);
			if (result == null)
				result = bestLayoutForAspect(portrait ? PhonePortraitLayouts : PhoneLandscapeLayouts);
		}
		else
		{
			result = bestLayoutForAspect(portrait ? PhonePortraitLayouts : PhoneLandscapeLayouts);
			if (result == null)
				result = bestLayoutForAspect(portrait ? TabletPortraitLayouts : TabletLandscapeLayouts);
		}
		
		return result;
	}

	protected string bestLayoutForAspect()
	{
		string result = bestLayoutForAspect(_isPortrait);
		if (result == null)
			result = bestLayoutForAspect(!_isPortrait);

		return result;
	}

	protected MagicUISkin bestSkinForDPI()
	{
		if (Skins == null)
			return null;

		MagicUISkin closest = null;
		float dist = float.PositiveInfinity;
		
		for (int i = 0; i < Skins.Length; i++)
		{
			float key = Skins[i].TargetDPI;
			float test = Mathf.Abs(key - _dpi);
			if (test < dist)
			{
				closest = Skins[i];
				dist = test;
			}
		}
		
		return closest;
	}

	protected MagicUISkin _skin;
	public MagicUISkin Skin
	{
		get { return _skin; }
	}
	#endregion

	public MagicUISkin[] Skins;


	protected void setupValuesTable()
	{
		_valuesTable = new Dictionary<string, string>();

		// TODO LOAD FROM PLAYER PREFS
	}

	public TextAsset StringTable;

	protected Dictionary<string, string> _stringTable;
	protected void setupStringTable()
	{
		_stringTable = new Dictionary<string, string>();

		string table = null;
		if (StringTable != null)
			table = StringTable.text;

		if (table != null)
		{
			JSONObject json = new JSONObject(table);
			if (json.IsObject)
			{
				string key = Application.systemLanguage.ToString();
				JSONObject strings = json["languages"][key];
				if (strings == null)
					strings = json["languages"]["English"];

				if (strings != null)
				{
					for (int i = 0; i < strings.keys.Count; i++)
					{
						_stringTable.Add(string.Intern(strings.keys[i]), strings.list[i].str);
					}
				}
			}
		}
	}

	public string GetString(string key)
	{
		string result = null;
		if (_stringTable != null && _stringTable.ContainsKey(key))
			result = _stringTable[key];

		if (result == null)
			return string.Format("\"{0}\"", key);
		else
			return result;
	}

	#region API
	public delegate void GenericCallback<T>(string error, T result);
	public interface IMagicUISource
	{
		void Setup(GenericCallback<bool> callback);
		void Fetch(string layout, GenericCallback<JSONObject> callback);
	}

	public GameObject API;

	protected IMagicUISource _api;
	protected void setupApi(GenericCallback<bool> callback)
	{
		if (callback == null)
			return;

		if (API == null)
			callback ("NO API", false);
		else
		{
			_api = API.GetComponent(typeof(IMagicUISource)) as IMagicUISource;
			if (_api == null)
				callback ("API INVALID", false);
			
			_api.Setup(callback);
		}
	}

	protected void onAPISetup(string error, bool success)
	{
		if (!string.IsNullOrEmpty(error))
			Debug.LogError(error);
		else
			_api.Fetch(bestLayoutForAspect(), onFetch);
	}

	protected Dictionary<int, MagicUIPage> _pages;
	protected Dictionary<string, int> _pagesByName;
	protected Stack<int> _pageStack;
	protected int _homePage;
	protected void onFetch(string error, JSONObject result)
	{
		if (!string.IsNullOrEmpty(error))
		{
			DebugManager.Log(error);
		}
		else
		{
			_pages = new Dictionary<int, MagicUIPage>();
			_pagesByName = new Dictionary<string, int>();
			_pageStack = new Stack<int>();

			JSONObject pages = result["pages"];
			string firstPage = result["firstPage"].str;

			for (int i = 0; i < pages.list.Count; i++)
			{
				MagicUIPage page = MagicUIPage.CreateFromMarkup(pages.keys[i], pages.list[i]);

				if (page != null)
				{
					_pages.Add(page.ID, page);
					_pagesByName.Add(page.name, page.ID);

					if (page.name == firstPage)
					{
						_homePage = page.ID;
						page.IsVisible = true;
						_pageStack.Push(_homePage);
					}
					else
						page.IsVisible = false;
				}
			}

			setupBackground();
		}
	}
	#endregion

	public UITexture Background;
	protected void setupBackground()
	{
		if (Background == null)
			return;

		Texture2D tex = Skin.Background;
		if (tex == null)
			return;

		Vector2 size = ScreenSize;

		Background.pivot = UIWidget.Pivot.TopLeft;
		Background.mainTexture = tex;
		Background.color = Skin.PrimaryColor;

		Rect uv = new Rect();
		uv.width = size.x / (float)tex.width;
		uv.height = size.y / (float)tex.height;

		Background.uvRect = uv;
		Background.depth = 0;

		Vector3 pos = Vector3.zero;
		pos.x = size.x * -0.5f;
		pos.y = size.y * 0.5f;
		Background.transform.localPosition = pos;
	}

	#region Control values system
	protected Dictionary<string, List<MagicUIControl>> _notificationTargets;
	public string ControlCreated(MagicUIControl control)
	{
		string key = control.Key;
		if (key == null)
			key = "";
		key = string.Intern(key);

		List<MagicUIControl> targets = null;
		if (_notificationTargets.ContainsKey(key))
			targets = _notificationTargets[key];
		else
		{
			targets = new List<MagicUIControl>();
			_notificationTargets.Add(key, targets);
		}

		targets.Add(control);

		string val = null;
		if (!string.IsNullOrEmpty(key))
		{
			// TODO LOAD EXISTING SAVED VALUES FROM PLAYER PREFS!

			// IF NO DEFAULT, GET DEFAULT FROM CONTROL
			val = control.DefaultValue;
			if (val != null)
				Write(key, val, control);
		}

		return val;
	}

	public void Write(MagicUIControl control, string val)
	{
		Write(control.Key, val, control);
	}
	public void Write(MagicUIControl control, float val)
	{
		Write (control.Key, val, control);
	}
	public void Write(MagicUIControl control, int val)
	{
		Write (control.Key, val, control);
	}

	public void Write(string key, string val, MagicUIControl exclude = null)
	{
		if (string.IsNullOrEmpty(key))
			return;

		if (val == null)
			return;

		_valuesTable[key] = val;

		DebugManager.Log ("{0} = {1}", key, val);

		List<MagicUIControl> targets = null;
		if (_notificationTargets.ContainsKey(key))
			targets = _notificationTargets[key];

		if (targets != null && targets.Count > 0)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				if (targets[i] == exclude)
					continue;

				targets[i].Value = val;
			}
		}
	}
	public void Write(string key, float val, MagicUIControl exclude = null)
	{
		Write (key, val.ToString(), exclude);
	}
	public void Write(string key, int val, MagicUIControl exclude = null)
	{
		Write (key, val.ToString(), exclude);
	}

	public bool ReadBool(string key)
	{
		string val = ReadString(key);
		if (string.IsNullOrEmpty(val))
			return false;

		bool result = false;
		if (bool.TryParse(val, out result))
			return result;
		else
			return !Mathf.Approximately(0, ReadFloat(key));
	}

	public int ReadInt(string key)
	{
		float val = ReadFloat(key);
		if (float.IsNaN(val))
			return 0;
		else
			return Mathf.RoundToInt(val);
	}

	public float ReadFloat(string key)
	{
		string val = ReadString (key);
		float result = float.NaN;

		if (val != null)
		{
			float.TryParse(val, out result);
		}

		return result;
	}

	public string ReadString(string key)
	{
		if (_valuesTable.ContainsKey(key))
			return _valuesTable[key];
		else
			return null;
	}

	#endregion

	public void ShowPage(string page)
	{
		if (_pagesByName != null && _pagesByName.ContainsKey(page))
			ShowPage(_pagesByName[page]);
	}

	public void ShowPage(int pageID)
	{
		if (_pages == null)
			return;

		int dest = pageID;
		if (dest < 0)
		{
			while (dest < 0)
			{
				++dest;
				_pageStack.Pop();
			}

			if (_pageStack.Count <= 0)
				_pageStack.Push(_homePage);
		}
		else if (_pages.ContainsKey(dest))
		{
			MagicUIPage page = _pages[dest];
			if (!page.IsDialog)
				_pageStack.Pop();
			_pageStack.Push(dest);
		}

		foreach (int key in _pages.Keys)
		{
			_pages[key].IsVisible = _pageStack.Contains(key);
		}
	}
}
