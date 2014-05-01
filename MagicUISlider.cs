using UnityEngine;
using System.Collections;

public class MagicUISlider : MagicUIControl
{
	protected UISprite _bar;
	protected UISprite _fg;
	protected UISprite _thumb;

	protected BoxCollider _collider;

	protected MagicUIImage _leftIcon;
	protected MagicUIImage _rightIcon;

	protected override void setLink(int dest)
	{
		// DO NOTHING
	}
	
	protected override string valueFromControl()
	{
		return _level.ToString();
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
	

	protected override void controlFromValue(string val)
	{
		float parsed = _level;
		if (float.TryParse(val, out parsed))
		{
			_level = parsed;
			setToLevel();
		}
	}

	protected float _level;

	protected void setToLevel()
	{
		_fg.fillAmount = _level;
		_thumb.transform.localPosition = new Vector3(_minX + (_level * (_maxX - _minX)), 0, 0);
	}

	public override string DefaultValue
	{
		get
		{
			return "0";
		}
	}

	protected void syncThumb()
	{
		if (UICamera.lastHit.collider == _collider)
		{
			float x = UICamera.lastHit.point.x * _collider.size.x;
			
			if (x < _minX)
				x = _minX;
			
			float delta = x - _minX;
			float max = _maxX - _minX;

			_level = Mathf.Clamp01(delta / max);
			setToLevel();
			submit();
		}
	}

	void OnPress(bool isDown)
	{
		if (isDown)
			syncThumb();
	}

	void OnDrag(Vector2 ignored)
	{
		syncThumb();
	}

	protected float _minX;
	protected float _maxX;
	protected override void setExtents(Rect extents)
	{
		float icons = 0;
		float offset = 0;

		_maxX = extents.width * 0.5f;
		_minX = -_maxX;

		if (_leftIcon != null)
		{
			icons += extents.height;
			offset += 0.5f * extents.height;
			_minX += extents.height;
			_leftIcon.SetExtents(new Rect(-extents.width * 0.5f, extents.height * 0.5f, extents.height, extents.height));
		}

		if (_rightIcon != null)
		{
			icons += extents.height;
			offset -= 0.5f * extents.height;
			_maxX -= extents.height;
			_rightIcon.SetExtents(new Rect((extents.width * 0.5f) - extents.height, extents.height * 0.5f, extents.height, extents.height));
		}

		Vector3 pos = Vector3.zero;
		pos.x = extents.x + (extents.width * 0.5f);
		pos.y = extents.y - (extents.height * 0.5f);
		transform.localPosition = pos;

		_bar.transform.localPosition = new Vector3(offset, 0, 0);
		_bar.width = Mathf.RoundToInt(extents.width - icons);
		_bar.height = Mathf.RoundToInt(extents.height * 0.2f);

		_fg.transform.localPosition = new Vector3(offset, 0, 0);
		_fg.width = Mathf.RoundToInt(extents.width - icons);
		_fg.height = Mathf.RoundToInt(extents.height * 0.2f);

		_thumb.transform.localPosition = Vector3.zero;
		_thumb.width = Mathf.RoundToInt(extents.height * 0.5f);
		_thumb.height = _thumb.width;

		_collider.size = new Vector3(extents.width, extents.height, 0.1f);
	}
	
	protected override void setZOrder(int z)
	{
		_bar.depth = z;

		_fg.depth = z + 1;

		if (_leftIcon != null)
			_leftIcon.SetZOrder(z + 2);

		if (_rightIcon != null)
			_rightIcon.SetZOrder(z + 3);


		_thumb.depth = z + 4;
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
			return MagicUIControl.ControlType.Slider;
		}
	}
	
	protected MagicUIImage _icon;
	protected override void initialize(JSONObject markup)
	{
		_collider = NGUITools.AddWidgetCollider(gameObject);

		GameObject child = new GameObject();
		child.name = "Bar";
		child.layer = gameObject.layer;
		child.transform.parent = transform;
		child.transform.localPosition = Vector3.zero;
		child.transform.localScale = Vector3.one;
		_bar = child.AddComponent<UISprite>();
		_bar.atlas = MagicUIManager.Instance.Skin.Atlas;
		_bar.type = UISprite.Type.Sliced;
		_bar.color = MagicUIManager.Instance.Skin.PrimaryColor;

		child = new GameObject();
		child.name = "FG";
		child.layer = gameObject.layer;
		child.transform.parent = transform;
		child.transform.localPosition = Vector3.zero;
		child.transform.localScale = Vector3.one;
		_fg = child.AddComponent<UISprite>();
		_fg.atlas = MagicUIManager.Instance.Skin.Atlas;
		_fg.type = UISprite.Type.Filled;
		_fg.fillDirection = UISprite.FillDirection.Horizontal;
		_fg.color = MagicUIManager.Instance.Skin.PrimaryColor;

		child = new GameObject();
		child.name = "Thumb";
		child.layer = gameObject.layer;
		child.transform.parent = transform;
		child.transform.localPosition = Vector3.zero;
		child.transform.localScale = Vector3.one;
		_thumb = child.AddComponent<UISprite>();
		_thumb.atlas = MagicUIManager.Instance.Skin.Atlas;
		_thumb.type = UISprite.Type.Simple;
		_thumb.color = MagicUIManager.Instance.Skin.PrimaryColor;

		JSONObject frameData = MagicUIManager.Instance.Skin.GetFrameData(ControlType.Slider);
		_bar.spriteName = frameData["bar"].str;
		_fg.spriteName = frameData["fg"].str;
		_thumb.spriteName = frameData["thumb"].str;

		JSONObject icon = markup["fromIcon"];
		if (icon != null)
		{
			_leftIcon = MagicUIImage.CreateAsChild(gameObject);
			_leftIcon.Initialize(icon);
		}

		icon = markup["toIcon"];
		if (icon != null)
		{
			_rightIcon = MagicUIImage.CreateAsChild(gameObject);
			_rightIcon.Initialize(icon);
		}

		// Bogus code to get around fact that NinjaMock doesn't provide us a name...
		if (_rightIcon != null)
			_key = _rightIcon.name;
		else if (_leftIcon != null)
			_key = _leftIcon.name;
		else
			_key = "slider" + name;
	}
	
	public static MagicUISlider Create()
	{
		GameObject go = new GameObject();
		MagicUISlider result = go.AddComponent<MagicUISlider>();
		return result;
	}
}