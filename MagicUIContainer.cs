using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicUIContainer : MagicUIControl, MagicUIControl.IMagicUIPanel
{
	protected UIPanel _panel;

	protected override void setExtents(Rect extents)
	{
		Vector3 pos = Vector3.zero;
		pos.x = extents.x + (extents.width * 0.5f);
		pos.y = extents.y - (extents.height * 0.5f);
		transform.localPosition = pos;
		
		_panel.baseClipRegion = new Vector4(0, 0, extents.width, extents.height);
	}

	protected override void setLink(int dest)
	{
		// DO NOTHING
		return;
	}

	public Vector2 ExpectedSize
	{
		get { return _expected; }
	}

	public Vector2 ExtraSize
	{
		get { return _extra; }
	}

	public Transform Container
	{
		get { return transform; }
	}

	public void Populate(List<JSONObject> markups)
	{
		for (int i = 0; i < markups.Count; i++)
		{
			MagicUIControl.CreateFromMarkup(string.Format("{0}.{1}", name, i), markups[i], this);
		}
	}

	protected Vector2 _expected;
	protected Vector2 _extra;
	protected override Rect calculateExtents(Rect bounds, MagicUIAnchorType anchors)
	{
		Vector2 expected = _container.ExpectedSize;
		Vector2 extra = _container.ExtraSize;
		
		float left = bounds.x * expected.x;
		float top = bounds.y * expected.y;
		float width = bounds.width * expected.x;
		float height = bounds.height * expected.y;

		_expected = new Vector2(width, height);
		
		if (anchors.GetAnchor(MagicUIAnchorType.Left) && anchors.GetAnchor(MagicUIAnchorType.Right))
			width += extra.x;
		else if (!anchors.GetAnchor(MagicUIAnchorType.Left) && !anchors.GetAnchor(MagicUIAnchorType.Right))
			left += extra.x * 0.5f;
		else if (anchors.GetAnchor(MagicUIAnchorType.Right))
			left += extra.x;
		//else if (anchors.GetAnchor(MagicUIAnchorType.Left))	Do nothing
		
		if (anchors.GetAnchor(MagicUIAnchorType.Top) && anchors.GetAnchor(MagicUIAnchorType.Bottom))
			height += extra.y;
		else if (!anchors.GetAnchor(MagicUIAnchorType.Top) && !anchors.GetAnchor(MagicUIAnchorType.Bottom))
			top += extra.y * 0.5f;
		else if (anchors.GetAnchor(MagicUIAnchorType.Bottom))
			top += extra.y;
		//else if (anchors.GetAnchor(MagicUIAnchorType.Left))	Do nothing

		_extra = new Vector2(width, height) - _expected;
		
		Vector2 actual = expected + extra;
		return new Rect(left - (0.5f * actual.x), (0.5f * actual.y) - top, width, height);
	}

	protected override void setZOrder(int z)
	{
		_panel.depth = z;
	}
	
	public override ControlType Type
	{
		get
		{
			return MagicUIControl.ControlType.Container;
		}
	}
	
	protected override void initialize(JSONObject markup)
	{
		_panel.clipping = UIDrawCall.Clipping.None;
//		_panel.clipSoftness = Vector2.one;
	}
	
	public static MagicUIContainer Create()
	{
		GameObject go = new GameObject();
		MagicUIContainer result = go.AddComponent<MagicUIContainer>();
		result._panel = go.AddComponent<UIPanel>();
		return result;
	}
}
