using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

public class DebugLogger
{
    public static bool EnableDebugLog = true;

    private TMP_Text _output = null;

    public List<string> _logLines = new List<string>();

    private SynchronizationContext _ctx;

    public DebugLogger(TMP_Text output = null)
    {
        _ctx = SynchronizationContext.Current;
        _output = output;
    }

    public void Log(string log)
    {
        if (EnableDebugLog)
        {
            if (_output != null)
            {
                AddLog(log);
            }
            else
            {
                Debug.Log(log);
            }
        }
    }

    public void Clear() {
        _logLines.Clear();
        _output.text = "";
    }

    private void AddLog(string message)
    {
        _ctx.Post(_ =>
        {
            _logLines.Add(message);
            _output.text = string.Join("\n", _logLines.ToArray().Reverse());
        }, null);
    }
}