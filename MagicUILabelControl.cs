using UnityEngine;
using System.Collections;

public class MagicUILabelControl : MagicUIControl
{
	protected MagicUILabel _label;
	
	protected int? _linkedPage = null;
	
	protected override void setLink(int dest)
	{
		_linkedPage = dest;
		NGUITools.AddWidgetCollider(gameObject);
	}
	
	void OnPress(bool isDown)
	{
		if (_linkedPage.HasValue)
		{
			if (!isDown)
				MagicUIManager.Instance.ShowPage(_linkedPage.Value);
		}
		else
		{
			MagicUIManager.Instance.Write(this, isDown ? 1 : 0);
		}
	}
	
	protected override void setExtents(Rect extents)
	{
		Vector3 pos = Vector3.zero;
		pos.x = extents.x + (extents.width * 0.5f);
		pos.y = extents.y - (extents.height * 0.5f);
		transform.localPosition = pos;
		
		Rect centeredExtents = extents;
		centeredExtents.x = centeredExtents.width * -0.5f;
		centeredExtents.y = centeredExtents.height * 0.5f;
		
		_label.SetExtents(centeredExtents);
	}
	
	protected override void setZOrder(int z)
	{
		_label.SetZOrder(z);
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
			return MagicUIControl.ControlType.Label;
		}
	}
	
	protected override void initialize(JSONObject markup)
	{
		_key = markup.GetStringSafely("name", "");
		_label = MagicUILabel.CreateAsComponent(gameObject);
		_label.Initialize(MagicUIManager.Instance.GetString(_key), false, markup);
	}
	
	public static MagicUILabelControl Create()
	{
		GameObject go = new GameObject();
		MagicUILabelControl result = go.AddComponent<MagicUILabelControl>();
		return result;
	}
}
