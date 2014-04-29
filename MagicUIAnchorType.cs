using UnityEngine;
using System.Collections;

[System.Flags]
public enum MagicUIAnchorType
{
	None = 0,
	Left = 1,
	Right = 2,
	Top = 4,
	Bottom = 8
}

public static class MagicUIAnchorTypeExtensions
{
	public static bool GetAnchor(this MagicUIAnchorType me, MagicUIAnchorType side)
	{
		if (side == MagicUIAnchorType.None)
			return false;

		return (me & side) == side;
	}

	public static MagicUIAnchorType SetAnchor(this MagicUIAnchorType me, MagicUIAnchorType side, bool isSet)
	{
		MagicUIAnchorType result = me & ~side;

		if (isSet)
			result = result | side;

		return result;
	}

	public static string GetAnchorAsString(this MagicUIAnchorType me)
	{
		int left = me.GetAnchor(MagicUIAnchorType.Left) ? 1 : 0;
		int right = me.GetAnchor(MagicUIAnchorType.Right) ? 1 : 0;
		int top = me.GetAnchor(MagicUIAnchorType.Top) ? 1 : 0;
		int bottom = me.GetAnchor(MagicUIAnchorType.Bottom) ? 1 : 0;
		
		return string.Format("{0}{1}{2}{3}", left, right, top, bottom);
	}

	public static MagicUIAnchorType SetAnchor(this MagicUIAnchorType me, string all)
	{
		MagicUIAnchorType result = MagicUIAnchorType.None;

		if (!string.IsNullOrEmpty(all))
		{
			result = result.SetAnchor(MagicUIAnchorType.Left, all.Length > 0 && all[0] == '1');
			result = result.SetAnchor(MagicUIAnchorType.Right, all.Length > 1 && all[1] == '1');
			result = result.SetAnchor(MagicUIAnchorType.Top, all.Length > 2 && all[2] == '1');
			result = result.SetAnchor(MagicUIAnchorType.Bottom, all.Length > 3 && all[3] == '1');
		}

		return result;
	}
}
