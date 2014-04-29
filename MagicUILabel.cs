using UnityEngine;
using System.Collections;

public class MagicUILabel : MonoBehaviour
{
	protected UILabel _label;

	protected bool _isButton;
	protected NGUIText.Alignment _horizontal;
	// _vertical
	protected bool _isMultiline;
	protected float _fontSize;

	protected bool _isTextBox;
	public UILabel SetForTextBox()
	{
		_isTextBox = true;
		_label.overflowMethod = UILabel.Overflow.ClampContent;
		return _label;
	}

	public Color Color
	{
		get { return _label.color; }
		set { _label.color = value; }
	}

	public void Initialize(string text, bool isButton, JSONObject markup)
	{
		_isTextBox = false;

		_label.text = text;
		_label.trueTypeFont = MagicUIManager.Instance.Skin.FontParameters.Font;
		_label.color = Color.black;
		_label.keepCrispWhenShrunk = UILabel.Crispness.Always;

		if (isButton)
		{
			_isButton = true;
			_isMultiline = false;
			_horizontal = NGUIText.Alignment.Center;
			// _vertical
			_fontSize = 999;
		}
		else
		{
			_isButton = false;

			bool? multiline = markup.GetBoolSafely("multiline", false);
			if (multiline.HasValue)
				_isMultiline = multiline.Value;

			switch (markup.GetStringSafely("halign", "center").ToLower()[0])
			{
				case 'l':
					_horizontal = NGUIText.Alignment.Left;
					break;

				case 'r':
					_horizontal = NGUIText.Alignment.Right;
					break;

				case 'c':
				default:
					_horizontal = NGUIText.Alignment.Center;
					break;
			}
			_label.alignment = _horizontal;

			//control.AddField("valign", valign);

			_fontSize = markup.GetFloatSafely("fontsize", 25);
		}
	}

	public void SetExtents(Rect extents)
	{
		if (!_isTextBox)
		{
			_label.overflowMethod = UILabel.Overflow.ShrinkContent;
		}

		_label.width = Mathf.RoundToInt(extents.width);
		_label.height = Mathf.RoundToInt(extents.height);
		_label.maxLineCount = (_isMultiline && !_isTextBox) ? 0 : 1;

		// TODO Scale font
		_label.fontSize = Mathf.RoundToInt(_fontSize);
	}

	public void SetZOrder(int z)
	{
		_label.depth = z;
	}

	public static MagicUILabel CreateAsChild(GameObject parent)
	{
		GameObject child = new GameObject();
		child.name = "Label";
		child.layer = parent.layer;
		child.transform.parent = parent.transform;
		child.transform.localPosition = Vector3.zero;
		child.transform.localScale = Vector3.one;
		
		return CreateAsComponent(child);
	}
	
	public static MagicUILabel CreateAsComponent(GameObject me)
	{
		MagicUILabel result = me.AddComponent<MagicUILabel>();
		result._label = me.AddComponent<UILabel>();
		return result;
	}
}
