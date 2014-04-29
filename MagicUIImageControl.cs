using UnityEngine;
using System.Collections;

public class MagicUIImageControl : MagicUIControl
{
	protected MagicUIImage _image;

	protected int? _linkedPage = null;

	protected override void setLink(int dest)
	{
		_linkedPage = dest;
		NGUITools.AddWidgetCollider(gameObject);
	}
	void OnClick()
	{
		if (_linkedPage.HasValue)
			MagicUIManager.Instance.ShowPage(_linkedPage.Value);
	}

	protected override void setExtents(Rect extents)
	{
		_image.SetExtents(extents);
	}

	protected override void setZOrder(int z)
	{
		_image.SetZOrder(z);
	}

	public override ControlType Type
	{
		get
		{
			return MagicUIControl.ControlType.Image;
		}
	}

	protected string _missingFrame;
	protected override void initialize(JSONObject markup)
	{
		_image = MagicUIImage.CreateAsComponent(gameObject);
		_image.Initialize(markup);
	}

	public static MagicUIImageControl Create()
	{
		GameObject go = new GameObject();
		MagicUIImageControl result = go.AddComponent<MagicUIImageControl>();
		return result;
	}
}
