/**************************************************************************/
/*  UidInspector.cs                                                       */
/**************************************************************************/
/*                         This file is part of:                          */
/*                          UID Explorer Plugin                           */
/**************************************************************************/
/* Copyright (c) 2024-present Justin Sasso                                */
/*                                                                        */
/* Permission is hereby granted, free of charge, to any person obtaining  */
/* a copy of this software and associated documentation files (the        */
/* "Software"), to deal in the Software without restriction, including    */
/* without limitation the rights to use, copy, modify, merge, publish,    */
/* distribute, sublicense, and/or sell copies of the Software, and to     */
/* permit persons to whom the Software is furnished to do so, subject to  */
/* the following conditions:                                              */
/*                                                                        */
/* The above copyright notice and this permission notice shall be         */
/* included in all copies or substantial portions of the Software.        */
/*                                                                        */
/* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        */
/* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     */
/* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. */
/* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   */
/* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   */
/* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE      */
/* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 */
/**************************************************************************/

#if TOOLS

using System;
using Godot;
using System.Collections.Generic;

namespace UidExplorerPluginProject;

public partial class UidInspector : EditorInspectorPlugin
{
	public string LastEditedPath 
	{
		get => lastEditedPath;
	}

	private const string UID_HINT_FILTER = "uid";

	// Project Settings
	private bool devModeEnabled;
	private PressOptionE pressOption;

	private readonly Dictionary<ulong, UidInspectorProperty> addedInspectorProperties = new();

	private string lastEditedPath = "";

	public UidInspector() {  }
	public UidInspector(bool devMode, PressOptionE pressOption)
	{
		UpdateSettings(devMode, pressOption);
	}

	public void UpdateSettings(bool devModeEnabled, PressOptionE pressOption)
	{
		this.devModeEnabled = devModeEnabled;
		this.pressOption = pressOption;

		foreach (KeyValuePair<ulong, UidInspectorProperty> item in addedInspectorProperties)
		{
			UidInspectorProperty foundProperty = item.Value;

			if (!IsInstanceValid(foundProperty))
			{
				if (devModeEnabled) GD.PrintErr("UidInspector>UpdateSettings>property was no longer valid.");
				addedInspectorProperties.Remove(item.Key);
				continue;
			}

			foundProperty.UpdateSettings(devModeEnabled, pressOption);
		}
	}
	private void OnUnpackCompleted(UidInspectorProperty uidInspectorProperty)
	{
		if (!IsInstanceValid(uidInspectorProperty))
		{
			return;
		}

		ManualUnpack(uidInspectorProperty);
	}
	public void ManualUnpack(UidInspectorProperty uidInspectorProperty)
	{
		addedInspectorProperties.Remove(uidInspectorProperty.GetInstanceId());
	}

	public override bool _CanHandle(GodotObject godotObject)
	{
		return true;
	}

	public override bool _ParseProperty(GodotObject godotObject, Variant.Type godotType, string name, 
		PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
	{
		if (godotType != Variant.Type.String || hintType != PropertyHint.File || hintString != UID_HINT_FILTER)
		{
			return false;
		}

		UidInspectorProperty inspectorProperty = new(this, devModeEnabled, pressOption);
		addedInspectorProperties.Add(inspectorProperty.GetInstanceId(), inspectorProperty);
		inspectorProperty.Connect(UidInspectorProperty.SignalName.UnpackCompleted, new(this, MethodName.OnUnpackCompleted));
		AddPropertyEditor(name, inspectorProperty);
		
		return true;
	}

	public void SetLastEditedPath(string lastEditedPath)
	{
		this.lastEditedPath = lastEditedPath;
	}
}

#endif