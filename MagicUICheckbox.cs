using UnityEngine;
using System.Collections;

public class MagicUICheckbox : MagicUIControl
{
	protected UISprite _sprite;
	protected string _onFrame;
	protected string _offFrame;

	protected override void setLink(int dest)
	{
		// DO NOTHING
	}

	protected bool isOn
	{
		get { return _sprite.spriteName == _onFrame; }
		set { _sprite.spriteName = value ? _onFrame : _offFrame; }
	}

	protected override string valueFromControl()
	{
		return isOn.ToString();
	}

	protected override void controlFromValue(string val)
	{
		bool on = false;
		if (val != null && val.Length > 0 && val.ToLower()[0] == 't')
			on = true;

		isOn = on;
	}

	void OnClick()
	{
		isOn = !isOn;
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
	}
	
	protected override void setZOrder(int z)
	{
		_sprite.depth = z;
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
			return MagicUIControl.ControlType.Checkbox;
		}
	}
	
	public override string DefaultValue
	{
		get
		{
			return "false";
		}
	}

	public override string Value
	{
		get
		{
			return baseValueHandler;
		}
		
		set
		{
			baseValueHandler = value;
		}
	}
	
	protected Vector2 _chrome;
	protected override void initialize(JSONObject markup)
	{
		_sprite = gameObject.AddComponent<UISprite>();
		_sprite.atlas = MagicUIManager.Instance.Skin.Atlas;
		_sprite.type = UISprite.Type.Sliced;
		NGUITools.AddWidgetCollider(gameObject);
		_sprite.autoResizeBoxCollider = true;
		
		_key = markup.GetStringSafely("name", "");

		JSONObject frameData = MagicUIManager.Instance.Skin.GetFrameData(ControlType.Checkbox);
		_onFrame = frameData["checkedOn"].str;
		_offFrame = frameData["uncheckedOn"].str;

		UISpriteData data = MagicUIManager.Instance.Skin.Atlas.GetSprite(_sprite.spriteName);
		
		_chrome = Vector2.one;
		if (frameData.keys.Contains("chrome"))
		{
			_chrome = frameData["chrome"].GetVector2();
			_chrome.x = data.width / (data.width - _chrome.x);
			_chrome.y = data.height / (data.height - _chrome.y);
		}
	}
	
	public static MagicUICheckbox Create()
	{
		GameObject go = new GameObject();
		MagicUICheckbox result = go.AddComponent<MagicUICheckbox>();
		return result;
	}
}
