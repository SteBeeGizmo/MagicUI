using UnityEngine;
using System.Collections;

public class MagicUITextBox : MagicUIControl
{
	protected UISprite _sprite;
	protected MagicUILabel _label;
	protected UIInput _input;

	protected override void setLink(int dest)
	{
		// DO NOTHING
	}
	
	protected override string valueFromControl()
	{
		return _input.value;
	}
	
	void OnSubmit()
	{
		submit();
	}
	
	protected override void setExtents(Rect extents)
	{
		Vector3 pos = Vector3.zero;
		pos.x = extents.x + (extents.width * 0.5f);
		pos.y = extents.y - (extents.height * 0.5f);
		transform.localPosition = pos;
		
		_sprite.width = Mathf.RoundToInt(extents.width * _chrome.x);
		_sprite.height = Mathf.RoundToInt(extents.height * _chrome.y);
		
		Rect centeredExtents = extents;
		centeredExtents.width -= _margin.x;
		centeredExtents.height -= _margin.y;
		centeredExtents.x = centeredExtents.width * -0.5f;
		centeredExtents.y = centeredExtents.height * 0.5f;
		_label.SetExtents(centeredExtents);
	}
	
	protected override void setZOrder(int z)
	{
		_sprite.depth = z;
		_label.SetZOrder(z + 2);
	}
	
	protected string _key;
	public override string Key
	{
		get
		{
			return _key;
		}
	}
	
	public override ControlType Type
	{
		get
		{
			return MagicUIControl.ControlType.TextBox;
		}
	}
	
	protected MagicUIImage _icon;
	protected Vector2 _chrome;
	protected Vector2 _margin;
	protected override void initialize(JSONObject markup)
	{
		_sprite = gameObject.AddComponent<UISprite>();
		_sprite.atlas = MagicUIManager.Instance.Skin.Atlas;
		_sprite.type = UISprite.Type.Sliced;
		_sprite.color = MagicUIManager.Instance.Skin.PrimaryColor;

		NGUITools.AddWidgetCollider(gameObject);
		_sprite.autoResizeBoxCollider = true;

		_key = markup.GetStringSafely("name", "");
		
		_label = MagicUILabel.CreateAsChild(gameObject);
		_label.Initialize(MagicUIManager.Instance.GetString(_key), false, markup);
		_input.label = _label.SetForTextBox();

		JSONObject frameData = MagicUIManager.Instance.Skin.GetFrameData(ControlType.TextBox);

		_label.Color = MagicUIManager.Instance.Skin.FontParameters.DefaultColor;
		bool? invert = frameData.GetBoolSafely("fgInvert", false);
		if (invert.HasValue && invert.Value)
			_label.Color = MagicUIManager.Instance.Skin.FontParameters.AlternateColor;

		_input.activeTextColor = _input.label.color;
		EventDelegate.Set(_input.onSubmit, OnSubmit);

		_sprite.spriteName = frameData["on"].str;
		
		UISpriteData data = MagicUIManager.Instance.Skin.Atlas.GetSprite(_sprite.spriteName);
		
		if (frameData.keys.Contains("margin"))
			_margin = frameData["margin"].GetVector2();

		_chrome = Vector4.zero;
		
		_chrome.x = frameData.GetFloatSafely("left", 0) + frameData.GetFloatSafely("right", 0);
		_chrome.x = data.width / (data.width - _chrome.x);
		
		_chrome.y = frameData.GetFloatSafely("top", 0) + frameData.GetFloatSafely("bottom", 0);
		_chrome.y = data.height / (data.height - _chrome.y);
	}
	
	public static MagicUITextBox Create()
	{
		GameObject go = new GameObject();
		MagicUITextBox result = go.AddComponent<MagicUITextBox>();
		result._input = go.AddComponent<UIInput>();

		return result;
	}
}
