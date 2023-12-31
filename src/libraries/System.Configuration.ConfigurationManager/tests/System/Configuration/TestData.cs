// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.ConfigurationTests
{
    public static class TestData
    {
        public static string ImplicitMachineConfig =
@"<configuration>
    <configSections>
        <section name='appSettings' type='System.Configuration.AppSettingsSection, System.Configuration.ConfigurationManager' restartOnExternalChanges='false' requirePermission='false'/>
    </configSections>
</configuration>";

        public static string EmptyConfig =
@"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
</configuration>";

        public static string SimpleConfig =
@"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
  <appSettings>
    <add key='FooKey' value='FooValue' />
    <add key='BarKey' value='BarValue' />
  </appSettings>
</configuration>";

        public static string SystemRuntimeRemotingSectionConfig =
@"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
  <system.runtime.remoting>
    <application>
      <channels>
        <channel ref='tcp' port='1111' />
      </channels>
    </application>
  </system.runtime.remoting>
</configuration>";

        public static string WindowsSectionConfig =
@"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
  <windows />
</configuration>";
    }
}
