#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class DisableAIToolkitWarning
{
    static DisableAIToolkitWarning()
    {
        // ??ng ký callback ?? l?c log
        Application.logMessageReceived += OnLogMessageReceived;
    }

    private static void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        // Ch?n warning t? Unity AI Toolkit v? Account API
        if (type == LogType.Warning && 
            (logString.Contains("Account API did not become accessible") ||
             logString.Contains("network issues or editor focus")))
        {
            // Không làm gì c? - warning b? ch?n
            return;
        }
    }
}
#endif
