using UnityEngine;
using System.Collections;

public class MagicUIButton : MagicUIControl
{
	protected UISprite _sprite;
	protected MagicUILabel _label;

	protected int? _linkedPage = null;

	protected override void setLink(int dest)
	{
		_linkedPage = dest;
	}

	protected override string valueFromControl()
	{
		return _isPressed.ToString();
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

	protected bool _isPressed;
	void OnPress(bool isDown)
	{
		if (_linkedPage.HasValue)
		{
			if (!isDown)
				MagicUIManager.Instance.ShowPage(_linkedPage.Value);
		}
		else
		{
			_isPressed = isDown;
			submit();
		}
	}
	
	protected override void setExtents(Rect extents)
	{
		Vector3 pos = Vector3.zero;
		pos.x = extents.x + (extents.width * 0.5f);
		pos.y = extents.y - (extents.height * 0.5f);
		transform.localPosition = pos;

		bool iconOnly = false;

		if (_sprite != null)
		{
			_sprite.width = Mathf.RoundToInt(extents.width * _chrome.x);
			_sprite.height = Mathf.RoundToInt(extents.height * _chrome.y);
		}
		else
		{
			iconOnly = true;
		}

		Rect centeredExtents = extents;
		centeredExtents.width -= _margin.x;
		centeredExtents.height -= _margin.y;
		centeredExtents.x = centeredExtents.width * -0.5f;
		centeredExtents.y = centeredExtents.height * 0.5f;

		if (_label != null)
		{
			_label.SetExtents(centeredExtents);
		}

		if (_icon != null)
		{
			if (iconOnly)
				_icon.SetExtents(extents);
			else
				_icon.SetExtents(centeredExtents);
		}
	}
	
	protected override void setZOrder(int z)
	{
		if (_sprite != null)
			_sprite.depth = z;

		if (_label != null)
			_label.SetZOrder(z + 2);

		if (_icon != null)
			_icon.SetZOrder(z + 1);
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
			return MagicUIControl.ControlType.Button;
		}
	}

	protected MagicUIImage _icon;
	protected Vector2 _chrome;
	protected Vector2 _margin;
	protected override void initialize(JSONObject markup)
	{
		_chrome = Vector2.one;
		_margin = Vector2.zero;

		_key = markup.GetStringSafely("name", "");

		bool? iconOnly = markup.GetBoolSafely("iconOnly", false);

		if (iconOnly.HasValue && iconOnly.Value)
		{
			JSONObject icon = markup["icon"];
			if (icon != null)
			{
				_icon = MagicUIImage.CreateAsComponent(gameObject);
				_icon.Initialize(icon);
			}
		}
		else
		{
			_sprite = gameObject.AddComponent<UISprite>();
			_sprite.atlas = MagicUIManager.Instance.Skin.Atlas;
			_sprite.type = UISprite.Type.Sliced;
			_sprite.autoResizeBoxCollider = true;
			_sprite.color = MagicUIManager.Instance.Skin.PrimaryColor;
			
			_label = MagicUILabel.CreateAsComponent(gameObject);
			_label.Initialize(MagicUIManager.Instance.GetString(_key), true, markup);
			
			JSONObject frameData = MagicUIManager.Instance.Skin.GetFrameData(ControlType.Button);
			_sprite.spriteName = frameData["on"].str;
			
			if (_label != null)
			{
				_label.Color = MagicUIManager.Instance.Skin.FontParameters.DefaultColor;
				bool? invert = frameData.GetBoolSafely("fgInvert", false);
				if (invert.HasValue && invert.Value)
					_label.Color = MagicUIManager.Instance.Skin.FontParameters.AlternateColor;
			}
			
			UISpriteData data = MagicUIManager.Instance.Skin.Atlas.GetSprite(_sprite.spriteName);

			if (frameData.keys.Contains("margin"))
				_margin = frameData["margin"].GetVector2();
			
			if (frameData.keys.Contains("chrome"))
			{
				_chrome = frameData["chrome"].GetVector2();
				_chrome.x = data.width / (data.width - _chrome.x);
				_chrome.y = data.height / (data.height - _chrome.y);
			}
		}

		NGUITools.AddWidgetCollider(gameObject);
	}
	
	public static MagicUIButton Create()
	{
		GameObject go = new GameObject();
		MagicUIButton result = go.AddComponent<MagicUIButton>();
		return result;
	}
}
