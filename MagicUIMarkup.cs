#if false
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;	

public class MagicUIMarkup 
{
	protected int _hash;
	public override int GetHashCode ()
	{
		return _hash;
	}

	protected bool _valid;
	public bool IsValid { get { return _valid; } }

	public override string ToString ()
	{
		if (_containers == null || _containers.Count == 0)
			return "";
		else if (MagicUIManager.Singleton != null)
			return dumpElements(MagicUIManager.Singleton.Aspect);
		else
		{
			return dumpElements(_containers.Keys.GetEnumerator().Current);
		}
	}

	protected void dumpAllElements()
	{
		foreach (MagicUIManager.AspectRatio ratio in _containers.Keys)
		{
			Debug.Log (dumpElements(ratio));
		}
	}


	/*
  <control controlID="3" controlTypeID="com.balsamiq.mockups::CheckBox" x="143" y="194" w="121" h="22" measuredW="78" measuredH="22" zOrder="4" locked="false" isInGroup="-1"/>
    <control controlID="5" controlTypeID="com.balsamiq.mockups::Image" x="738" y="216" w="230" h="406" measuredW="77" measuredH="79" zOrder="1" locked="false" isInGroup="-1">
      <controlProperties>
        <text>asdasd</text>
      </controlProperties>
    </control>
    <control controlID="6" controlTypeID="com.balsamiq.mockups::RadioButton" x="140" y="267" w="139" h="22" measuredW="101" measuredH="22" zOrder="5" locked="false" isInGroup="-1">
      <controlProperties>
        <text>Radio%20Button%201</text>
      </controlProperties>
    </control>
    <control controlID="7" controlTypeID="com.balsamiq.mockups::RadioButton" x="140" y="289" w="139" h="22" measuredW="104" measuredH="22" zOrder="6" locked="false" isInGroup="-1">
      <controlProperties>
        <text>Radio%20Button%202</text>
      </controlProperties>
    </control>
    <control controlID="10" controlTypeID="__group__" x="143" y="355" w="139" h="42" measuredW="139" measuredH="42" zOrder="7" locked="false" isInGroup="-1">
      <controlProperties>
        <controlName>OtherSelection</controlName>
      </controlProperties>
      <groupChildrenDescriptors>
        <control controlID="0" controlTypeID="com.balsamiq.mockups::RadioButton" x="0" y="0" w="139" h="22" measuredW="104" measuredH="22" zOrder="0" locked="false" isInGroup="10">
          <controlProperties>
            <text>Radio%20Button%203</text>
          </controlProperties>
        </control>
        <control controlID="1" controlTypeID="com.balsamiq.mockups::RadioButton" x="0" y="20" w="139" h="22" measuredW="105" measuredH="22" zOrder="1" locked="false" isInGroup="10">
          <controlProperties>
            <text>Radio%20Button%204</text>
          </controlProperties>
        </control>
      </groupChildrenDescriptors>
    </control>
  </controls>
</mockup>	 * */

	protected string dumpElements(MagicUIManager.AspectRatio ratio)
	{
		StringBuilder result = new StringBuilder();

		int count = 0;
		if (_controls.ContainsKey(ratio))
			count = _controls[ratio].Count;

		result.Append("Container " + ratio.ToString() + ": {");
		result.Append(string.Format("\"{0}\" = \"{1}\"", "ClientCenter", _containers[ratio].ClientCenter));
		result.Append(string.Format(", \"{0}\" = \"{1}\"", "ClientSize", _containers[ratio].ClientSize));
		result.Append(string.Format(", \"{0}\" = \"{1}\"", "Order", _containers[ratio].Order));
		result.Append(string.Format(", \"{0}\" = \"{1}\"", "Disproportional", _containers[ratio].Disproportional));
		result.AppendLine("} with " + count.ToString() + " controls:");


		if (count > 0)
		{
			foreach (Control control in _controls[ratio])
			{
				result.Append(string.Format("\"{0}\" = \"{1}\"", "RelativeCenter", control.RelativeCenter));
				result.Append(string.Format(", \"{0}\" = \"{1}\"", "RelativeSize", control.RelativeSize));
				foreach (string key in control.Attributes.Keys)
				{
					result.Append(string.Format(", \"{0}\" = \"{1}\"", key, control.Attributes[key]));
				}
				result.AppendLine ("");
			}
		}

		return result.ToString();
	}

	protected bool parseXml(string xml)
	{
		if (xml.Contains(".dtd"))
			return false;

		XmlReaderSettings settings = new XmlReaderSettings();
		settings.CloseInput = true;
		settings.IgnoreProcessingInstructions = true;
		settings.IgnoreWhitespace = true;
		settings.ProhibitDtd = true;

		XmlReader reader = XmlReader.Create(new StringReader(xml), settings);

		string key = null;
		Dictionary<string, string> control = null;
		Dictionary<string, string> pushed = null;

		while (reader.Read ())
		{
			if (reader.Depth < 2)
				continue;

			switch (reader.NodeType)
			{
			case XmlNodeType.EndElement:
				if (pushed != null && reader.Name == "groupChildrenDescriptors")
				{
					control = pushed;
					pushed = null;
				}
				break;
				//continue;

			case XmlNodeType.Text:
				if (key != null && control != null)
				{
					control[key] = WWW.UnEscapeURL(reader.ReadContentAsString());
					key = null;
				}
				break;

			case XmlNodeType.Element:
				if (reader.Name == "groupChildrenDescriptors")
				{
					pushed = control;
				}
				else if (reader.Name == "control")
				{
					if (reader.HasAttributes)
					{
						control = new Dictionary<string, string>();
						for (int i = 0; i < reader.AttributeCount; i++)
						{
							reader.MoveToAttribute(i);
							string attr = reader.Name;
							control.Add (attr, reader.Value);
							if (attr == "controlID")
							{
								if (pushed != null)
								{
									control[attr] = pushed[attr] + "." + control[attr];
								}
					            _elements.Add (control[attr], control);
							}
						}
						reader.MoveToElement(); 
					}
				}
				else if (reader.Name != "controlProperties")
					key = reader.Name;
				break;

			default:
				break;
				//continue;
			}
		}

		return true;
	}

	public class Container
	{
		public MagicUIManager.AspectRatio Aspect;
		public string ControlID;
		public Vector2 ClientCenter;
		public Vector2 ClientSize;
		public int Order;
		public bool Disproportional;
	}

	public class Control
	{
		public Vector2 UpperLeft;
		public Vector2 RelativeCenter;
		public Vector2 RelativeSize;
		public int Order;
		public Dictionary<string, string> Attributes;
	}

	public Control[] GetControls(MagicUIManager.AspectRatio aspect)
	{
		// TODO fallback if aspect ratio isn't present?
		if (!_controls.ContainsKey(aspect))
			return new Control[0];

		return _controls[aspect].ToArray();
	}

	public Control[] GetControls()
	{
		return GetControls(MagicUIManager.Singleton.Aspect);
	}

	protected Dictionary<MagicUIManager.AspectRatio, Container> _containers;
	protected Dictionary<MagicUIManager.AspectRatio, List<Control>> _controls;

	protected Dictionary<string, Dictionary<string, string>> _elements;


	protected const string kControlPrefix = "com.balsamiq.mockups::";
	protected const string kControlIDKey = "controlID";
	protected const string kControlTypeIDKey = "controlTypeID";
	protected const string kControlTypeIPhone = "iPhone";
	protected const string kControlTypeComponent = "Component";

	protected const float kIPhone5ClientWidthScale = 226.0f / 282.0f;
	protected const float kIPhone5ClientHeightScale = 402.0f / 573.0f;

	protected const float kIPhone4ClientWidthScale = 225.0f / 282.0f;
	protected const float kIPhone4ClientHeightScale = 338.0f / 510.0f;

	protected void preprocessIPhoneControl(Dictionary<string, string> element)
	{
		// {"controlID" = "29", "controlTypeID" = "iPhone", "x" = "13", "y" = "44", "w" = "573",
		// "h" = "282", "measuredW" = "573", "measuredH" = "282", "zOrder" = "0", "locked" = "false",
		// "isInGroup" = "-1", "bgTransparent" = "true", "model" = "IPhone5", "orientation" = "landscape", "topBar" = "false"}

		Container result = new Container();

		result.Order = element.GetInt("zOrder");

		string device = "IPhone5";
		string orientation = "landscape";
		element.TryGetValue("model", out device);
		element.TryGetValue("orientation", out orientation);
		bool landscape = (orientation == "landscape");

		Vector2 origin = element.GetVector2("x", "y");

		Vector2 framesize = element.GetVector2 ("w", "h");
		Vector2 actualsize = element.GetVector2 ("measuredW", "measuredH");
		if (framesize.x < actualsize.x)
			framesize.x = actualsize.x;
		if (framesize.y < actualsize.y)
			framesize.y = actualsize.y;

		if (device == "IPhone4")
		{
			result.Aspect = landscape ? MagicUIManager.AspectRatio.Landscape3x2 : MagicUIManager.AspectRatio.Portrait3x2;
			if (!landscape)
			{
				result.ClientSize.x = framesize.x * kIPhone4ClientWidthScale;
				result.ClientSize.y = framesize.y * kIPhone4ClientHeightScale;
			}
			else
			{
				result.ClientSize.x = framesize.x * kIPhone4ClientHeightScale;
				result.ClientSize.y = framesize.y * kIPhone4ClientWidthScale;
			}
		}
		else
		{
			result.Aspect = landscape ? MagicUIManager.AspectRatio.Landscape16x9 : MagicUIManager.AspectRatio.Portrait16x9;
			if (!landscape)
			{
				result.ClientSize.x = framesize.x * kIPhone5ClientWidthScale;
				result.ClientSize.y = framesize.y * kIPhone5ClientHeightScale;
			}
			else
			{
				result.ClientSize.x = framesize.x * kIPhone5ClientHeightScale;
				result.ClientSize.y = framesize.y * kIPhone5ClientWidthScale;
			}
		}
		result.ClientCenter = origin + (framesize * 0.5f);

		_containers.Add (result.Aspect, result);
	}

	protected const float kIPadClientWidthScale = 768.0f / 974.0f;
	protected const float kIPadClientHeightScale = 1024.0f / 1239.0f;
	protected void preprocessIPadControl(Dictionary<string, string> element)
	{
		// {"controlID" = "46", "controlTypeID" = "Component", "x" = "30", "y" = "340", "w" = "1239",
		// "h" = "974", "measuredW" = "1239", "measuredH" = "974", "zOrder" = "16", "locked" = "false",
		// "isInGroup" = "-1", "src" = "$ACCOUNT/assets/iPads.bmml#iPad Landscape"}

		Container result = new Container();
		
		result.Order = element.GetInt("zOrder");
		
		bool landscape = element["src"].Contains("Landscape");
		
		Vector2 origin = element.GetVector2("x", "y");
		Vector2 framesize = element.GetVector2 ("w", "h");
		
		// TODO CORRECT FOR DISCREPANCY BETWEEN framesize AND actualsize
		Vector2 actualsize = element.GetVector2 ("measuredW", "measuredH");
		if (actualsize != framesize)
			result.Disproportional = true;
		
		result.Aspect = landscape ? MagicUIManager.AspectRatio.Landscape4x3 : MagicUIManager.AspectRatio.Portrait4x3;
		if (!landscape)
		{
			result.ClientSize.x = framesize.x * kIPadClientWidthScale;
			result.ClientSize.y = framesize.y * kIPadClientHeightScale;
		}
		else
		{
			result.ClientSize.x = framesize.x * kIPadClientHeightScale;
			result.ClientSize.y = framesize.y * kIPadClientWidthScale;
		}
		result.ClientCenter = origin + (framesize * 0.5f);
		
		_containers.Add (result.Aspect, result);
	}

	protected List<string> findContainers()
	{
		List<string> noncontainers = new List<string>();

		_containers = new Dictionary<MagicUIManager.AspectRatio, Container>();

		foreach (string id in _elements.Keys)
		{
			Dictionary<string, string> element = _elements[id];
			
			if (!element.ContainsKey(kControlTypeIDKey))
				continue;
			
			string type = element[kControlTypeIDKey];
			if (type.Length > kControlPrefix.Length)
			{
				type = type.Substring(kControlPrefix.Length);
				element[kControlTypeIDKey] = type;
			}
			
			if (type == kControlTypeIPhone)
				preprocessIPhoneControl(element);
			else if (type == kControlTypeComponent)
			{
				string src = "";
				if (element.ContainsKey ("src"))
					src = element["src"];

				if (src.Contains("iPads.bmml"))
					preprocessIPadControl(element);
				else
					noncontainers.Add (id);
			}
			else
				noncontainers.Add (id);
		}

		return noncontainers;
	}

	protected MagicUIManager.AspectRatio assignToContainer(Vector2 pos, int order)
	{
		float mindist = float.MaxValue;
		MagicUIManager.AspectRatio closest = MagicUIManager.AspectRatio.Unknown;

		foreach (Container container in _containers.Values)
		{
			if (container.Order > order)
				continue;

			float dist = (pos - container.ClientCenter).magnitude;
			if (dist < mindist)
			{
				mindist = dist;
				closest = container.Aspect;
			}
		}

		return closest;
	}

	protected void distributeControl(Control control)
	{
		control.RelativeCenter = control.UpperLeft + (control.RelativeSize * 0.5f);

		MagicUIManager.AspectRatio closest = assignToContainer(control.RelativeCenter, control.Order);
		if (closest != MagicUIManager.AspectRatio.Unknown)
		{
			List<Control> dest = null;
			if (_controls.ContainsKey (closest))
				dest = _controls[closest];
			else
			{
				dest = new List<Control>();
				_controls.Add (closest, dest);
			}
			
			dest.Add (control);
		}

		control.RelativeCenter -= _containers[closest].ClientCenter;
		control.RelativeCenter.x /= _containers[closest].ClientSize.x;
		control.RelativeCenter.y /= _containers[closest].ClientSize.y;
		control.RelativeSize.x /= _containers[closest].ClientSize.x;
		control.RelativeSize.y /= _containers[closest].ClientSize.y;
		
		control.RelativeCenter.y = -control.RelativeCenter.y;
	}

	protected void fixupControlGroup(Control group, List<Control> children)
	{
		foreach (Control child in children)
		{
			Vector2 pos = child.UpperLeft;
			pos += group.UpperLeft;
			child.UpperLeft = pos;
			child.Attributes.SetVector2("x", "y", pos);
			
			int z = child.Order;
			z += group.Order;
			child.Order = z;
			child.Attributes.SetInt("zOrder", z);
			
			if (group.Attributes.ContainsKey("controlName"))
				child.Attributes["controlName"] = group.Attributes["controlName"];
		}
	}

	protected void distributeControls(List<string> candidates)
	{
		_controls = new Dictionary<MagicUIManager.AspectRatio, List<Control>>();

		List<Control> temp = new List<Control>();

		Dictionary<string, Control> groups = new Dictionary<string, Control>();
		Dictionary<string, List<Control>> groupChildren = new Dictionary<string, List<Control>>();

		foreach (string id in candidates)
		{
			Dictionary<string, string> element = _elements[id];

			Vector2 origin = element.GetVector2("x", "y");

			Vector2 measured = element.GetVector2("measuredW", "measuredH");
			Vector2 size = element.GetVector2("w", "h");
			if (size.x < measured.x)
				size.x = measured.x;
			if (size.y < measured.y)
				size.y = measured.y;

			Control control = new Control();
			control.Attributes = element;
			control.UpperLeft = origin;
			control.RelativeSize = size;
			control.Order = element.GetInt("zOrder");

			temp.Add(control);

			if (control.Attributes["controlID"].Contains("."))
			{
				string[] parts = control.Attributes["controlID"].Split(".".ToCharArray());
				List<Control> list = null;
				if (groupChildren.ContainsKey (parts[0]))
					list = groupChildren[parts[0]];
				else
				{
					list = new List<Control>();
					groupChildren.Add (parts[0], list);
				}
				list.Add (control);
			}
			
			if (control.Attributes["controlTypeID"] == "__group__")
			{
				groups.Add(control.Attributes["controlID"], control);
			}
		}

		if (groups.Count > 0)
		{
			foreach (Control control in groups.Values)
			{
				if (groupChildren.ContainsKey(control.Attributes["controlID"]))
					fixupControlGroup (control, groupChildren[control.Attributes["controlID"]]);
				else
					Debug.Log ("Group " + control.Attributes["controlID"] + " has no children");
			}
		}

		foreach (Control control in temp)
			distributeControl(control);
	}

	protected void preprocessMarkup()
	{
		distributeControls(findContainers());
		//dumpAllElements();
	}



	public string Key { get; protected set; }

	public bool IsDialog { get; protected set; }

	public MagicUIMarkup(string key, string xml)
	{
		Key = key;

		string[] parts = key.Split(".".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 2 && parts[1].ToLower ().StartsWith("dialog"))
			IsDialog = true;

		_elements = new Dictionary<string, Dictionary<string, string>>();

		_hash = xml.GetHashCode();
		_valid = parseXml(xml);

		if (_valid)
			preprocessMarkup();
	}
}
#endif
