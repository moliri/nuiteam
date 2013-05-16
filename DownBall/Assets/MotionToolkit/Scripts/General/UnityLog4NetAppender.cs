using UnityEngine;
using System.Collections;
using log4net.Core;
using log4net.Appender;

/// <summary>
/// a Log4Net appender that right all debug messages to Unity log
/// </summary>
public class UnityLog4NetAppender : AppenderSkeleton 
{
    public bool UseFrameworkLog = false;
    /// <summary>
    /// Writes the logging event to a MessageBox
    /// </summary>
    override protected void Append(LoggingEvent loggingEvent)
    {
        if (loggingEvent.Level <= Level.Info)
        {            
            Debug.Log(RenderLoggingEvent(loggingEvent));
        }
        else if (loggingEvent.Level < Level.Error)
        {            
            Debug.LogWarning(RenderLoggingEvent(loggingEvent));
        }
        else
            Debug.LogError(RenderLoggingEvent(loggingEvent));
    }
    /// <summary>
    /// This appender requires a <see cref="Layout"/> to be set.
    /// </summary>
    override protected bool RequiresLayout
    {
        get { return true; }
    } 
}

