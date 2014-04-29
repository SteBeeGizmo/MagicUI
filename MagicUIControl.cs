using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MagicUIControl : MonoBehaviour
{
	public enum ControlType
	{
		Unknown,
		Container,
		Rectangle,
		Button,
		Checkbox,
		Slider,
		Label,
		TextBox,
		Image
	}

	public abstract ControlType Type { get; }
	public MagicUIPage ThePage { get; protected set; }

	public virtual string Key
	{
		get
		{
			return null;
		}
	}

	public string DebugValue;
	public virtual string Value
	{
		get { return DebugValue; }
		set
		{
			DebugValue = value;
			controlFromValue(DebugValue);
			// DON'T notify of change here, only if in response to click
		}
	}

	public interface IMagicUIPanel
	{
		Vector2 ExpectedSize { get; }
		Vector2 ExtraSize { get; }
		Transform Container { get; }
		void Populate(List<JSONObject> markups);
	}

	protected IMagicUIPanel _container;

	protected void register()
	{
		MagicUIManager dest = MagicUIManager.Instance;
		if (dest != null)
		{
			controlFromValue(dest.ControlCreated(this));
		}
	}

	public virtual string DefaultValue
	{
		get
		{
			return null;
		}
	}

	protected virtual string valueFromControl()
	{
		// TODO IN DERIVED CLASSES
		return null;
	}

	protected virtual void controlFromValue(string val)
	{
		// TODO IN DERIVED CLASSES
	}

	// You have to copy these in every child class, since Unity's call mechanism doesn't honor inheritance.
	void Start() { onStart(); }

	protected virtual void onStart()
	{
	}

	protected virtual void submit()
	{
		MagicUIManager dest = MagicUIManager.Instance;
		if (dest != null)
		{
			Value = valueFromControl();
			dest.Write(this, Value);
		}
	}

	protected virtual Rect calculateExtents(Rect bounds, MagicUIAnchorType anchors)
	{
		Vector2 expected = _container.ExpectedSize;
		Vector2 extra = _container.ExtraSize;

		float left = bounds.x * expected.x;
		float top = bounds.y * expected.y;
		float width = bounds.width * expected.x;
		float height = bounds.height * expected.y;

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
		//else if (anchors.GetAnchor(MagicUIAnchorType.Top))	Do nothing

		Vector2 actual = expected + extra;
		return new Rect(left - (0.5f * actual.x), (0.5f * actual.y) - top, width, height);
	}

	protected virtual void setContainer(IMagicUIPanel container)
	{
		_container = container;

		gameObject.layer = container.Container.gameObject.layer;
		transform.parent = container.Container;
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
	}

	protected abstract void setLink(int dest);

	protected abstract void setExtents(Rect extents);

	protected abstract void setZOrder(int z);

	protected abstract void initialize(JSONObject markup);

	public static MagicUIControl CreateFromMarkup(string id, JSONObject markup, IMagicUIPanel page)
	{
		MagicUIControl.ControlType type = markup.GetControlType();

		MagicUIControl control = null;
		switch (type)
		{
			case ControlType.Label:
				control = MagicUILabelControl.Create();
				break;

			case ControlType.Image:
				control = MagicUIImageControl.Create();
				break;

			case ControlType.Button:
				control = MagicUIButton.Create();
				break;

			case ControlType.Container:
				control = MagicUIContainer.Create();
				break;

			case ControlType.Rectangle:
				control = MagicUIRectangle.Create();
				break;

			case ControlType.Checkbox:
				control = MagicUICheckbox.Create();
				break;

			case ControlType.TextBox:
				control = MagicUITextBox.Create();
				break;

			case ControlType.Slider:
				control = MagicUISlider.Create();
				break;

			default:
				break;
		}

		if (control != null)
		{
			control.name = id;
			control.setContainer(page);

			control.initialize(markup);
			control.setExtents(control.calculateExtents(markup.GetRect(), markup.GetAnchors()));
			int z = markup.GetIntSafely("zOrder", 0);
			control.setZOrder(z * 10);

			int link = markup.GetIntSafely("link", int.MinValue);
			if (link >= -1)
				control.setLink(link);

			if (control is IMagicUIPanel)
				((IMagicUIPanel)control).Populate(markup["contents"]["children"].list);

			control.register();
		}

		return null;
	}
}
