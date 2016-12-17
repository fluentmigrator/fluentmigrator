To use Log4NetAnnouncer with XML configuration of log4net,
remember to add an assembly attribute below the using statements 
in your main assembly:
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

Sample configuration of log4net to configure a console appender:
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
  </configSections>
  ...
  <log4net>
    <root>
      <level value="DEBUG"/>
      <appender-ref ref="ConsoleAppender" />
    </root>
    <appender name="ConsoleAppender" type="log4net.Appender.ColoredConsoleAppender">
      <mapping>
        <level value="INFO" />
        <foreColor value="Green, HighIntensity" />
      </mapping>
      <mapping>
        <level value="DEBUG" />
        <foreColor value="Cyan, HighIntensity" />
      </mapping>
      <mapping>
        <level value="WARN" />
        <foreColor value="Yellow, HighIntensity" />
      </mapping>
      <mapping>
        <level value="ERROR" />
        <foreColor value="Red, HighIntensity" />
      </mapping>
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="%message%newline" />
      </layout>
    </appender>
  </log4net>

Sample configuration of log4net to configure a file appender:
  <log4net>
    <root>
      <level value="DEBUG"/>
      <appender-ref ref="RollingFileAppender" />
    </root>
    <appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
      <file value="log.txt" />
      <appendToFile value="true" />
      <rollingStyle value="Size" />
      <maxSizeRollBackups value="10" />
      <maximumFileSize value="1MB" />
      <staticLogFileName value="true" />
      <layout type="log4net.Layout.PatternLayout">
          <conversionPattern value="%date [%thread] %-5level %logger [%property{NDC}] - %message%newline" />
      </layout>
    </appender>
  </log4net>