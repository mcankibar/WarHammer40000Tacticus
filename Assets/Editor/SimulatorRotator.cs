using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a shortcut to rotate the built-in Device Simulator view
/// (same action as the Rotate button in the Simulator toolbar).
/// Default:  Shift + R — remap in Edit → Shortcuts.
/// </summary>
public static class SimulatorRotator
{
    const string MenuPath = "Tools/Device Simulator/Rotate #r";
    const string WindowTypeName = "UnityEditor.DeviceSimulation.SimulatorWindow, UnityEditor.DeviceSimulatorModule";

    static readonly BindingFlags InstanceFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [MenuItem(MenuPath)]
    public static void RotateDevice()
    {
        if (!TryGetSimulatorWindow(out var window, out var windowType))
        {
            Debug.LogWarning("Device Simulator window not found. Open Game view and switch it to Simulator.");
            return;
        }

        var main = windowType.GetProperty("main", InstanceFlags)?.GetValue(window)
                   ?? windowType.GetField("m_Main", InstanceFlags)?.GetValue(window);
        if (main == null)
        {
            Debug.LogWarning("Device Simulator is not initialized yet.");
            return;
        }

        var ui = main.GetType().GetProperty("userInterface", InstanceFlags)?.GetValue(main)
                 ?? main.GetType().GetField("m_UserInterface", InstanceFlags)?.GetValue(main);
        if (ui == null)
        {
            Debug.LogWarning("Device Simulator UI controller not found.");
            return;
        }

        var rotationProp = ui.GetType().GetProperty("Rotation", InstanceFlags);
        if (rotationProp == null)
        {
            Debug.LogWarning("Could not access Device Simulator Rotation.");
            return;
        }

        var current = Convert.ToInt32(rotationProp.GetValue(ui));
        var next = (current + 90) % 360;
        rotationProp.SetValue(ui, next);
    }

    [MenuItem(MenuPath, true)]
    public static bool RotateDeviceValidate()
    {
        return Type.GetType(WindowTypeName) != null;
    }

    static bool TryGetSimulatorWindow(out object window, out Type windowType)
    {
        window = null;
        windowType = Type.GetType(WindowTypeName);
        if (windowType == null)
            return false;

        var instances = Resources.FindObjectsOfTypeAll(windowType);
        if (instances is { Length: > 0 })
        {
            window = instances[0];
            return true;
        }

        var show = windowType.GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        show?.Invoke(null, null);

        instances = Resources.FindObjectsOfTypeAll(windowType);
        if (instances is { Length: > 0 })
        {
            window = instances[0];
            return true;
        }

        return false;
    }
}
