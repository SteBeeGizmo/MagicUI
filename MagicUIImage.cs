using UnityEngine;
using System.Collections;

public class MagicUIImage : MonoBehaviour
{
	protected UISprite _sprite;
	protected UITexture _texture;

	protected bool _imageReady;
	protected bool _extentsReady;
	protected bool _zOrderReady;

	protected Rect _pendingExtents;
	protected int _pendingZOrder;

	public bool IsReady
	{
		get { return _imageReady; }
	}

	protected bool _isChild;

	public void SetExtents(Rect extents)
	{
		if (_imageReady)
		{
			_extentsReady = false;

			if (_sprite != null)
			{
				_sprite.width = Mathf.RoundToInt(extents.width);
				_sprite.height = Mathf.RoundToInt(extents.height);
			}
			else if (_texture != null)
			{
				_texture.width = Mathf.RoundToInt(extents.width);
				_texture.height = Mathf.RoundToInt(extents.height);
			}

			Vector3 pos = Vector3.zero;
			pos.x = extents.center.x;
			pos.y = extents.y - (extents.height * 0.5f);

			transform.localPosition = pos;
		}
		else
		{
			_extentsReady = true;
			_pendingExtents = extents;
		}
	}

	public void SetZOrder(int z)
	{
		if (_imageReady)
		{
			_zOrderReady = false;
			if (_sprite != null)
				_sprite.depth = z;
			else if (_texture != null)
				_texture.depth = z;
		}
		else
		{
			_zOrderReady = true;
			_pendingZOrder = z;
		}
	}

	protected void setImageReady()
	{
		_imageReady = true;

		if (_extentsReady)
			SetExtents(_pendingExtents);
		if (_zOrderReady)
			SetZOrder(_pendingZOrder);
	}

	protected IEnumerator fetchTexture(string url)
	{
		WWW www = new WWW(url);
		yield return www;

		if (!string.IsNullOrEmpty(www.error))
			createSprite(_missingFrame);
		else
		{
			_texture = gameObject.AddComponent<UITexture>();
			_texture.mainTexture = www.texture;
			_texture.autoResizeBoxCollider = true;
			setImageReady();
		}
	}

	protected void createSprite(string frame)
	{
		_sprite = gameObject.AddComponent<UISprite>();
		_sprite.atlas = MagicUIManager.Instance.Skin.Atlas;
		_sprite.spriteName = frame;
		_sprite.type = UISprite.Type.Simple;
		_sprite.autoResizeBoxCollider = true;
		setImageReady();
	}

	protected string _missingFrame;
	public void Initialize(JSONObject markup)
	{
		_sprite = null;
		_texture = null;
		_imageReady = false;
		_extentsReady = false;
		_zOrderReady = false;

		string rawFrame = markup.GetStringSafely("frame", null);

		JSONObject frameData = MagicUIManager.Instance.Skin.GetFrameData(MagicUIControl.ControlType.Image);
		_missingFrame = frameData["missing"].str;

		string frame = null;
		if (rawFrame == null)
			frame = _missingFrame;
		else
		{
			frame = rawFrame.Substring(rawFrame.LastIndexOf('/') + 1);
			if (frame.Contains("."))
				frame = frame.Substring(0, frame.LastIndexOf('.'));

			if (string.IsNullOrEmpty(frame))
			{
				frame = _missingFrame;
				if (_isChild)
					gameObject.name = frame;
			}
			else
			{
				if (_isChild)
					gameObject.name = frame;

				UISpriteData spriteData = MagicUIManager.Instance.Skin.Atlas.GetSprite(frame);
				if (spriteData == null)
				{
					if (Debug.isDebugBuild)
					{
						frame = null;
						DebugManager.Log("TODO: BAKE {0} INTO ATLAS AS FRAME {1}", rawFrame, frame);
						StartCoroutine(fetchTexture(rawFrame));
					}
					else
					{
						frame = _missingFrame;
					}
				}
			}
		}

		if (frame != null)
		{
			createSprite(frame);
		}
	}

	public static MagicUIImage CreateAsChild(GameObject parent)
	{
		GameObject child = new GameObject();
		child.name = "Image";
		child.layer = parent.layer;
		child.transform.parent = parent.transform;
		child.transform.localPosition = Vector3.zero;
		child.transform.localScale = Vector3.one;

		MagicUIImage result = CreateAsComponent(child);
		result._isChild = true;
		return result;
	}
	
	public static MagicUIImage CreateAsComponent(GameObject me)
	{
		MagicUIImage result = me.AddComponent<MagicUIImage>();
		result._isChild = false;
		return result;
	}
}
