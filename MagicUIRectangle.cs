using UnityEngine;
using System.Collections;

public class MagicUIRectangle : MagicUIControl
{
	protected UISprite _sprite;

	protected override void setLink(int dest)
	{
		// DO NOTHING
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

	public override ControlType Type
	{
		get
		{
			return MagicUIControl.ControlType.Rectangle;
		}
	}
	
	protected Vector2 _chrome;
	protected override void initialize(JSONObject markup)
	{
		_sprite = gameObject.AddComponent<UISprite>();
		_sprite.atlas = MagicUIManager.Instance.Skin.Atlas;
		_sprite.type = UISprite.Type.Sliced;

		JSONObject frameData = MagicUIManager.Instance.Skin.GetFrameData(ControlType.Rectangle);
		_sprite.spriteName = frameData["on"].str;
		
		UISpriteData data = MagicUIManager.Instance.Skin.Atlas.GetSprite(_sprite.spriteName);
		
		_chrome = Vector2.one;
		if (frameData.keys.Contains("chrome"))
		{
			_chrome = frameData["chrome"].GetVector2();
			_chrome.x = data.width / (data.width - _chrome.x);
			_chrome.y = data.height / (data.height - _chrome.y);
		}
	}
	
	public static MagicUIRectangle Create()
	{
		GameObject go = new GameObject();
		MagicUIRectangle result = go.AddComponent<MagicUIRectangle>();
		return result;
	}
}
