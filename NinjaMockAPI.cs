using UnityEngine;
using System.Collections;
using System.Text;

public class NinjaMockAPI : MonoBehaviour, MagicUIManager.IMagicUISource
{
	protected const string _kQuote = "\"";
	protected const string _kEscapedQuote = "\\\"";
	protected string unescape(string input)
	{
		string working = input.Trim();
		if (working.StartsWith(_kQuote) && working.EndsWith(_kQuote))
		{
			working = working.Substring(1, working.Length - 2);
			if (working.Contains(_kEscapedQuote))
			{
				working = working.Replace(_kEscapedQuote, _kQuote);
			}
		}
		
		return working;
	}
	
	protected IEnumerator loadProject(string projectId, string auth, MagicUIManager.GenericCallback<JSONObject> callback)
	{
		Hashtable headers = new Hashtable();
		headers["Cookie"] = auth;
		headers["Content-Type"] = "application/json";
		
		JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
		json.AddField("projectId", projectId);
		json.AddField("sharedPagesOnly", true);
		string postText = json.ToString();
		byte[] postData = Encoding.UTF8.GetBytes(postText);
		
		WWW request = new WWW(_kGetProjectDataURL, postData, headers);
		DebugManager.Log ("Fetch {0}", _kGetProjectDataURL);
		yield return request;
		
		JSONObject response = null;
		if (!string.IsNullOrEmpty(request.error))
			Debug.LogWarning(request.error);
		else
		{
			string unescaped = unescape(request.text);
			response = new JSONObject(unescaped);
			if (!response.IsObject)
				callback("PROJECT PARSE FAILED", null);
			else
				callback(null, response);
		}
	}
	
	protected const string _kProjectIdMarker = "\"appId\":";
	protected const string _kServer = "http://www.ninjamock.com";
	protected const string _kGetProjectDataURL = _kServer + "/api/Designer/GetProjectData";
	
	protected IEnumerator landingPage(string url, MagicUIManager.GenericCallback<JSONObject> callback)
	{
		string id = null;
		string auth = null;
		string body = null;

#if UNIWEB
		DebugManager.Log ("Fetch {0} via UniWeb", url);
		var request = new HTTP.Request("GET", url);
		yield return request.Send();

		if (request.exception != null)
			callback(request.exception.Message, null);
		else
		{
			if (request.response.headers.Contains("SET-COOKIE"))
			{
				foreach (string cookie in request.response.headers.GetAll("SET-COOKIE"))
				{
					if (cookie.StartsWith(".ASPXAUTH"))
					{
						auth = cookie.Substring(0, auth.IndexOf(";"));;
						body = request.response.Text;
						break;
					}
				}

				if (auth == null)
					callback("COOKIE NOT FOUND", null);
			}
		}



#else
		DebugManager.Log ("Fetch {0} via WWW", url);

		WWW request = new WWW(url);
		yield return request;
		
		if (!string.IsNullOrEmpty(request.error))
		{
			callback(request.error, null);
		}
		else
		{
			if (request.responseHeaders.ContainsKey("SET-COOKIE"))
			{
				auth = request.responseHeaders["SET-COOKIE"];
				if (!auth.StartsWith(".ASPXAUTH"))
				{
					Debug.Log (request.responseHeaders["SET-COOKIE"]);
					callback("COOKIE BURIED", null);
				}
				else
				{
					auth = auth.Substring(0, auth.IndexOf(";"));
					body = request.text;
				}
			}
		}
#endif
		if (body != null)
		{
			int start = body.IndexOf(_kProjectIdMarker);
			if (start >= 0)
			{
				int count = body.IndexOf(',', start + _kProjectIdMarker.Length) - (start + _kProjectIdMarker.Length);
				if (count >= 0)
				{
					id = body.Substring(start + _kProjectIdMarker.Length, count).Trim();
				}
			}
		}
		if (id == null)
			callback("BODY PARSE FAILED", null);

		if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(auth))
			yield return StartCoroutine(loadProject(id, auth, callback));
	}

	public void Setup(MagicUIManager.GenericCallback<bool> callback)
	{
		// Nothing to do!
		if (callback != null)
			callback(null, true);
	}

	public void Fetch(string layout, MagicUIManager.GenericCallback<JSONObject> callback)
	{
		if (callback == null)
			return;

		StartCoroutine(landingPage(layout, (error, json) => { 
			if (!string.IsNullOrEmpty(error))
				callback(error, null);
			else
			{
				int firstId = (int)json["pageGroups"].list[0]["properties"]["pageIds"].list[0].n;
				JSONObject parsed = parseToIntermediateFormat(json["pages"], firstId);
				if (parsed == null)
					callback("PREPARSE FAILED", null);
				else
					callback(null, parsed);
			}
		}));
	}

	#region Parsers for specific control types
	protected JSONObject parseCommon(JSONObject raw, Vector2 containerSize, MagicUIControl.ControlType type)
	{
		JSONObject control = JSONObject.obj;

		Rect bounds = raw.GetRect();
		bounds.x /= containerSize.x;
		bounds.width /= containerSize.x;
		bounds.y /= containerSize.y;
		bounds.height /= containerSize.y;
		control.SetRect(bounds);

		control.SetAnchors(raw.GetAnchors());

		control.AddField("zOrder", raw.GetFloatSafely("zOrder", 0));

		int link = raw.GetIntSafely("pageLink", -1);
		if (link > 0)
			control.AddField("link", link);

		control.SetControlType(type);
		
		return control;
	}

	protected JSONObject parseContainer(JSONObject raw, JSONObject children, Vector2 containerSize, bool isDialog)
	{
		JSONObject control = parseCommon(raw, containerSize, MagicUIControl.ControlType.Container);

		string controlName = raw.GetStringSafely("text", null);
		if (controlName == null)
			controlName = raw.GetStringSafely("label", "");
		control.AddField("name", controlName);
		
		Rect bounds = control.GetRect();
		Vector2 mySize = new Vector2(bounds.width * containerSize.x, bounds.height * containerSize.y);
		control.AddField("contents", parsePage(children, mySize));

		control.AddField("isDialog", isDialog);
		
		return control;
	}

	protected JSONObject parseRectangle(JSONObject raw, Vector2 containerSize)
	{
		JSONObject control = parseCommon(raw, containerSize, MagicUIControl.ControlType.Rectangle);

		return control;
	}

	protected JSONObject parseSubIcon(string key, string source, JSONObject raw, Vector2 containerSize)
	{
		JSONObject obj = JSONObject.obj;

		float h = raw.GetFloatSafely(string.Format("{0}.height", key), containerSize.y);
		h = h / containerSize.y;
		obj.AddField("height", h);
		
		float w = raw.GetFloatSafely(string.Format("{0}.width", key), containerSize.y);
		w = w / containerSize.x;
		obj.AddField("width", w);

		string url = source;
		if (url.StartsWith("/"))
			url = _kServer + url;
		obj.AddField("frame", url);

		return obj;
	}
	
	protected JSONObject parseSlider(JSONObject raw, Vector2 containerSize)
	{
		JSONObject control = parseCommon(raw, containerSize, MagicUIControl.ControlType.Slider);

		Rect bounds = control.GetRect();
		Vector2 mySize = new Vector2(bounds.width * containerSize.x, bounds.height * containerSize.y);

		string from = raw.GetStringSafely("fromImage.url", null);
		if (from != null)
			control.AddField("fromIcon", parseSubIcon("fromImage", from, raw, mySize));

		string to = raw.GetStringSafely("toImage.url", null);
		if (from != null)
			control.AddField("toIcon", parseSubIcon("toImage", to, raw, mySize));

		return control;
	}

	protected JSONObject parseButton(JSONObject raw, Vector2 containerSize)
	{
		Debug.Log (raw);
		JSONObject control = parseCommon(raw, containerSize, MagicUIControl.ControlType.Button);
		
		string controlName = raw.GetStringSafely("text", null);
		if (controlName == null)
			controlName = raw.GetStringSafely("label", "");
		control.AddField("name", controlName);

		string icon = raw.GetStringSafely("image.url", null);
		if (icon != null)
		{
			Rect bounds = control.GetRect();
			Vector2 mySize = new Vector2(bounds.width * containerSize.x, bounds.height * containerSize.y);
			control.AddField("icon", parseSubIcon("image", icon, raw, mySize));
		}

		control.AddField("iconOnly", (raw.GetStringSafely("type", null) == "imageOnly"));

		return control;
	}
	
	protected JSONObject parseTextBox(JSONObject raw, Vector2 containerSize)
	{
		JSONObject control = parseCommon(raw, containerSize, MagicUIControl.ControlType.TextBox);

		control.AddField("name", raw.GetStringSafely("text", ""));

		return control;
	}

	protected JSONObject parseLabel(JSONObject raw, Vector2 containerSize)
	{
		JSONObject control = parseCommon(raw, containerSize, MagicUIControl.ControlType.Label);

		string text = raw.GetStringSafely("text", null);
		if (text == null)
			text = raw.GetStringSafely("name", "unnamed");
		control.AddField("name", text);

		string align = null;
		bool? scale = raw.GetBoolSafely("scaleWithFontSize", false);
		if (scale.HasValue && scale.Value)
		{
			align = "center";
		}
		else
		{
			align = raw.GetStringSafely("textalign", "left");
		}
		control.AddField("halign", align);

		bool? multiline = raw.GetBoolSafely("multiline", false);
		control.AddField("multiline", multiline.HasValue ? multiline.Value : false);

		string valign = raw.GetStringSafely("textverticalalign", "top");
		control.AddField("valign", valign);

		float size = raw.GetFloatSafely("font.size", 25);
		control.AddField("fontsize", size);

		return control;
	}

	protected JSONObject parseCheckbox(JSONObject raw, Vector2 containerSize)
	{
		JSONObject control = parseCommon(raw, containerSize, MagicUIControl.ControlType.Checkbox);

		string controlName = raw.GetStringSafely("labelEnabled", null);
		if (controlName == null)
			controlName = raw.GetStringSafely("labelDisabled", "");
		control.AddField("name", controlName);

		return control;
	}
	
	protected JSONObject parseImage(JSONObject raw, Vector2 containerSize)
	{
		JSONObject control = parseCommon(raw, containerSize, MagicUIControl.ControlType.Image);
		
		control.AddField("name", raw.GetStringSafely("text", ""));

		string url = raw.GetStringSafely("source.url", "");
		if (url.StartsWith("/"))
			url = _kServer + url;
		control.AddField("frame", url);

		return control;
	}
	#endregion

	protected JSONObject parsePage(JSONObject raw, Vector2 pageSize)
	{
		JSONObject page = new JSONObject(JSONObject.Type.OBJECT);

		float aspect = pageSize.x / pageSize.y;
		if (aspect < 1)
			aspect = 1 / aspect;
		page.AddField("aspect", aspect);

		JSONObject children = new JSONObject(JSONObject.Type.ARRAY);
		page.AddField("children", children);

		for (int i = 0; i < raw.list.Count; i++)
		{
			JSONObject child = raw.list[i];
			string type = child["type"].str;
			type = type.Substring(type.LastIndexOf('.') + 1).ToLower();

			switch (type)
			{
				case "slider":
					children.Add(parseSlider(child["properties"], pageSize));
					break;

				case "rectangle":
					children.Add(parseRectangle(child["properties"], pageSize));
					break;
					
				case "button":
				case "baritem":
					children.Add(parseButton(child["properties"], pageSize));
					break;
					
				case "image":
				case "icon":
					children.Add(parseImage(child["properties"], pageSize));
					break;

				case "groupcontainer":
					children.Add(parseContainer(child["properties"], child["children"], pageSize, false));
					break;

				case "dialog":
					children.Add(parseContainer(child["properties"], child["children"], pageSize, true));
					break;

				case "switch":
					children.Add(parseCheckbox(child["properties"], pageSize));
					break;

				case "textbox":
					children.Add(parseTextBox(child["properties"], pageSize));
					break;

				case "textblock":
				case "label":
					children.Add(parseLabel(child["properties"], pageSize));
					break;

				default:
					break;
			}
		}

		return page;
	}

	protected JSONObject parseToIntermediateFormat(JSONObject raw, int firstPageId)
	{
		Debug.Log (raw);

		JSONObject parsed = new JSONObject(JSONObject.Type.OBJECT);
		JSONObject pages = new JSONObject(JSONObject.Type.OBJECT);
		parsed.AddField("pages", pages);

		for (int i = 0; i < raw.list.Count; i++)
		{
			JSONObject pageEntry = raw.list[i];
			if (pageEntry["isDeleted"].b)
				continue;

			if (pageEntry["type"].str != "sketch.ui.pages.PortableDevicePage")
				continue;

			string name = pageEntry["name"].str;
			JSONObject rawPage = pageEntry["children"].list[0]["children"];
			Vector2 pageSize = pageEntry["children"].list[0]["panelProperties"]["properties"].GetVector2();

			int id = (int)pageEntry["id"].n;
			if (id == firstPageId)
				parsed.AddField("firstPage", name);

			JSONObject parsedPage = parsePage(rawPage, pageSize);
			parsedPage.AddField("id", id);
			pages.AddField(name, parsedPage);
		}
		
		return parsed;
	}

	void Start()
	{
		//StartCoroutine(doTest());
	}

	protected IEnumerator doTest()
	{
		bool uniweb = true;
		
		string url = "http://ninjamock.com/s/pzlltf";
		//string url = "http://www.mit.edu";
		
		if (uniweb)
		{
			var request = new HTTP.Request("GET", url);
			yield return request.Send();
			
			if (request.exception != null)
				Debug.Log("FAIL: " + request.exception.Message);
			else
				Debug.Log("WIN: " + request.response.Text);
		}
		else
		{
			WWW request = new WWW(url);
			yield return request;
			
			if (!string.IsNullOrEmpty(request.error))
				Debug.Log("FAIL: " + request.error);
			else
				Debug.Log("WIN: " + request.text);
		}
	}
}