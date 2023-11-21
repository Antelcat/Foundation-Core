﻿using Microsoft.Extensions.Logging;

namespace Antelcat.Core.Implements.Loggers;

public class LoggerConfig
{
    public enum LogFormat
    {
        Plain = 0,
        Json  = 1
    }


    #region Configs

    protected virtual bool OutputConsole { get; set; } = true;
    protected string Directory { get; set; } = Path.Combine(AppContext.BaseDirectory, "Logs");
    protected Func<DateTime, string> NamingFormat { get; set; } = time => $"{nameof(Antelcat)}[{time:yyyy-MM-dd}].log";
    protected string Prefix { get; set; } = "/**";
    protected string Suffix { get; set; } = "*/";
    protected LogLevel LogLevel { get; set; } = LogLevel.Trace;
    protected virtual LogFormat Format { get; set; } = LogFormat.Json;

    #endregion

    public LoggerConfig WithDirectory(string directory)
    {
        Directory = directory;
        return this;
    }

    public LoggerConfig WithPrefix(string prefix)
    {
        Prefix = prefix;
        return this;
    }

    public LoggerConfig WithSuffix(string suffix)
    {
        Suffix = suffix;
        return this;
    }

    public LoggerConfig WithFileNameFormat(Func<DateTime, string> namingFormat)
    {
        NamingFormat = namingFormat;
        return this;
    }

    public LoggerConfig OutputToConsole(bool output)
    {
        OutputConsole = output;
        return this;
    }

    public LoggerConfig WithLogLevel(LogLevel logLevel)
    {
        LogLevel = logLevel;
        return this;
    }

    public LoggerConfig WithLogFormat(LogFormat format)
    {
        Format = format;
        return this;
    }

    internal virtual void Initialize(AntelcatLogger logger)
    {
        logger.WithFileNameFormat(NamingFormat);
        logger.OutputToConsole(OutputConsole);
        logger.WithDirectory(Directory);
        logger.WithLogLevel(LogLevel);
        logger.WithPrefix(Prefix);
        logger.WithSuffix(Suffix);
        logger.WithLogFormat(Format);
    }
}