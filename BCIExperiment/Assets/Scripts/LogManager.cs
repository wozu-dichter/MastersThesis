using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Interfaces;
using Model;
using UnityEngine;
using NextMind.Events;
using DefaultNamespace;

public class LogManager : MonoBehaviour, ILogWriter
{
    private StreamWriter streamWriter;

    [SerializeField]
    private bool _enableConfidenceLogging;
    public void OnTriggered(int index)
    {
        using (NeuroTagHitLogEntry neuroTagHitLogEntry = new NeuroTagHitLogEntry(index))
        {
            PostDataToLogfile(neuroTagHitLogEntry);           
        }
    }
    public void OnConfidenceChanged(float value)
    {
        using (NeuroTagConfidenceLogEntry msg = new NeuroTagConfidenceLogEntry(value))
        {
            if(_enableConfidenceLogging)
                PostDataToLogfile(msg);           
        }
    }
    void onExperimentStarted(string message)
    {
        using (ExperimentEventLogEntry msg = new ExperimentEventLogEntry(message))
        {
            PostDataToLogfile(msg);
        }
    }

    void onExperimentFinished(string message)
    {
        using (ExperimentEventLogEntry msg = new ExperimentEventLogEntry(message))
        {
            PostDataToLogfile(msg);
        }
        streamWriter.Flush();
        streamWriter.Close();
    }
    public void OnTargetSet(string name)
    {
        using (NeuroTagMarkedAsTargetLogEntry msg = new NeuroTagMarkedAsTargetLogEntry(name))
        {
            PostDataToLogfile(msg);
        }
    }
    private void OnEnable()
    {
        OpenLogForWriting();
        ExperimentManager.onExperimentStarted += onExperimentStarted;
        ExperimentManager.onExperimentFinished += onExperimentFinished;
        TargetManager.onTargetSet += OnTargetSet;
    }

    private void OnDisable()
    {
        ExperimentManager.onExperimentStarted -= onExperimentStarted;
        ExperimentManager.onExperimentFinished -= onExperimentFinished;
        TargetManager.onTargetSet -= OnTargetSet;
    }
    public void PostDataToLogfile(LogEntry logEntry)
    {
        Debug.Log(logEntry.getLogString());
        if (!streamWriter.Equals(default))
        {
            streamWriter.WriteLineAsync(logEntry.getLogString());           
        }
    }
    public void OpenLogForWriting()
    {
        string filepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string filename = "BCILog.csv";

        try
        {
            streamWriter = new StreamWriter(Path.Combine(filepath, filename), true);
        }
        catch (Exception e)
        {
            using (ErrorLogEntry msg = new ErrorLogEntry("streamwriter error: " + e.Message))
            {
                PostDataToLogfile(msg);
            }
        }
    }
}
