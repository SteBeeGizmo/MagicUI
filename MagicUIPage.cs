using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicUIPage : MonoBehaviour, MagicUIControl.IMagicUIPanel
{
	protected JSONObject _markup;

	protected const float _kOffscreenX = 100000;
	public bool IsVisible
	{
		get
		{
			return (transform.localPosition.x == 0);
		}

		set
		{
			if (value)
				transform.localPosition = Vector3.zero;
			else
				transform.localPosition = new Vector3(_kOffscreenX, 0, 0);
		}
	}

	protected int _dialogDepth;
	public int DialogDepth
	{
		get { return _dialogDepth; }
		set
		{
			_dialogDepth = value;
			Depth = _baseDepth;
		}
	}

	protected int _baseDepth;
	public int Depth
	{
		get { return _baseDepth; }
		set
		{
			_baseDepth = value;
			_panel.depth = _baseDepth + (_dialogDepth * 1000);
		}
	}

	public bool IsDialog
	{
		// TODO
		get { return false; }
	}

	protected UIPanel _panel;
	protected bool _isReady;

	void Awake()
	{
		_panel = GetComponent<UIPanel>();
	}

	protected void clearMarkup()
	{
		while (transform.childCount > 0)
		{
			Transform child = transform.GetChild (0);
			child.parent = null;
			Destroy(child.gameObject);
		}
	}

	public int ID { get; protected set; }

	protected Vector2 _expectedSize;
	public Vector2 ExpectedSize { get { return _expectedSize; } }

	protected Vector2 _extraSize;
	public Vector2 ExtraSize { get { return _extraSize; } }

	public Transform Container { get { return transform; } }

	protected static int _nextPanelDepth = 1;
	public static MagicUIPage CreateFromMarkup(string pageName, JSONObject markup)
	{
		GameObject go = new GameObject();
		go.name = pageName;

		go.transform.parent = MagicUIManager.Instance.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
		go.layer = MagicUIManager.Instance.gameObject.layer;

		UIPanel panel = go.AddComponent<UIPanel>();
		panel.depth = _nextPanelDepth;
		++_nextPanelDepth;

		MagicUIPage page = go.AddComponent<MagicUIPage>();

		Vector2 pageSize = MagicUIManager.Instance.ScreenSize;
		float aspect = markup["aspect"].n;

		page.ID = Mathf.RoundToInt(markup["id"].n);

		page._expectedSize = pageSize;
		page._extraSize = Vector2.zero;
		if (!Mathf.Approximately(MagicUIManager.Instance.Aspect, aspect))
		{
			if (MagicUIManager.Instance.Aspect > aspect)
			{
				// Screen is more rectangular than expected
				// Keep our short dimension fixed and add to the long dimension
				if (MagicUIManager.Instance.IsPortrait)
				{
					// Long dimension is y
					page._expectedSize.y = page._expectedSize.x * aspect;
				}
				else
				{
					// Long dimension is x
					page._expectedSize.x = page._expectedSize.y / aspect;
				}
			}
			else
			{
				// Screen is squarer than expected
				// Keep our long dimension fixed and add to the short dimension
				if (MagicUIManager.Instance.IsPortrait)
				{
					// Short dimension is x
					page._expectedSize.x = page._expectedSize.y / aspect;
				}
				else
				{
					// Short dimension is y
					page._expectedSize.y = page._expectedSize.x * aspect;
				}
			}
			page._extraSize = pageSize - page._expectedSize;
		}

		panel.clipping = UIDrawCall.Clipping.SoftClip;
		panel.clipSoftness = Vector2.one;
		panel.baseClipRegion = new Vector4(0, 0, pageSize.x, pageSize.y);

		page.Populate(markup["children"].list);

		return page;
	}

	public void Populate(List<JSONObject> markups)
	{
		for (int i = 0; i < markups.Count; i++)
		{
			MagicUIControl.CreateFromMarkup(i.ToString(), markups[i], this);
		}
	}
}
