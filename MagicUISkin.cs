using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicUISkin : MonoBehaviour
{
	public TextAsset Definition;

	[System.Serializable]
	public class FontParametersFields
	{
		public float ReferenceHeight = 266;
		public float ReferenceDelta = -48;
		public Font Font;

		public Color DefaultColor = Color.black;
		public Color AlternateColor = Color.white;
	}
	public FontParametersFields FontParameters;

	public float TargetDPI = 150.0f;

	public Texture2D Background;

	public Color PrimaryColor = Color.white;

	protected UIAtlas _atlas;

	protected Dictionary<MagicUIControl.ControlType, JSONObject> _frameData;

	protected void setData(JSONObject parent, string key, MagicUIControl.ControlType type)
	{
		if (parent[key] == null)
			return;

		_frameData.Add(type, parent[key]);
	}

	protected void init()
	{
		_atlas = GetComponent<UIAtlas>();

		string text = Definition.text;

		JSONObject json = new JSONObject(text);
		JSONObject def = json["skin"];

		bool? safe = def.GetBoolSafely("copyrightSafe", true);
		if (safe.HasValue && !safe.Value && !Debug.isDebugBuild)
		{
			Debug.LogError("COPYRIGHT NOT VALID FOR PRODUCTION USE");
			Application.Quit();

			// Force the app to die, in case Quit() doesn't work
			throw new System.InvalidProgramException();
		}

		_frameData = new Dictionary<MagicUIControl.ControlType, JSONObject>();
		JSONObject mapping = def["mapping"];
		setData(mapping, "container", MagicUIControl.ControlType.Container);
		setData(mapping, "rectangle", MagicUIControl.ControlType.Rectangle);
		setData(mapping, "button", MagicUIControl.ControlType.Button);
		setData(mapping, "checkbox", MagicUIControl.ControlType.Checkbox);
		setData(mapping, "slider", MagicUIControl.ControlType.Slider);
		setData(mapping, "label", MagicUIControl.ControlType.Label);
		setData(mapping, "textbox", MagicUIControl.ControlType.TextBox);
		setData(mapping, "image", MagicUIControl.ControlType.Image);
	}

	public UIAtlas Atlas
	{
		get
		{
			if (_atlas == null || _frameData == null)
				init();

			return _atlas;
		}
	}

	public JSONObject GetFrameData(MagicUIControl.ControlType control)
	{
		if (_atlas == null || _frameData == null)
			init();

		if (!_frameData.ContainsKey(control))
			return null;

		return _frameData[control];
	}
}
