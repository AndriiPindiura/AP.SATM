﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AP.SATM.Brain.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public int sqlCommandTimeout {
            get {
                return ((int)(this["sqlCommandTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SELECT TOP 1 [date] FROM [satm].[dbo].[PROTOCOL] WHERE owner=@owner ORDER by [dat" +
            "e] DESC")]
        public string lastProtocolQuery {
            get {
                return ((string)(this["lastProtocolQuery"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SELECT TOP 1 [Date] FROM [satm].[dbo].[satm1C] WHERE owner=@owner ORDER by [Date]" +
            " DESC")]
        public string lastProtocol1cQuery {
            get {
                return ((string)(this["lastProtocol1cQuery"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=.\\SQLEXPRESS;Integrated Security=True;MultipleActiveResultSets=True;")]
        public string databaseConnectionString {
            get {
                return ((string)(this["databaseConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"INSERT [satm].[dbo].[PROTOCOL](objtype,objid,action,date,owner,pk,processed)
select objtype,objid,action,date,owner,pk,'false' from OPENROWSET
('SQLOLEDB','Server={@SQLServer};UID={@username};PWD={@password}','SELECT [objtype]
      ,[objid],[action],[date],[owner],[pk]
  FROM [{@Database}].[dbo].[ViewProtocol]
  order by date desc') rdb
  where NOT EXISTS (select pk FROM [satm].[dbo].[PROTOCOL] ldb where rdb.pk=ldb.pk)")]
        public string protocolQuery {
            get {
                return ((string)(this["protocolQuery"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"INSERT [satm].[dbo].[satm1C] (owner,[1cID],carID,Date,Entry,Culture,WHO,pk)
select * from OPENROWSET
('SQLOLEDB','Server={@SQLServer};UID={@username};PWD={@password}','SELECT *
  FROM [{@Database}].[dbo].[ViewProtocol1C]') rdb
  where NOT EXISTS (select owner,pk FROM [satm].[dbo].[satm1C] ldb where 
  (rdb.owner=ldb.owner) and  (rdb.pk=ldb.pk) ) ")]
        public string sapQuery {
            get {
                return ((string)(this["sapQuery"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("DELETE FROM [satm].[dbo].[PROTOCOL]\n      WHERE date<GETDATE()-91\nDELETE FROM [sa" +
            "tm].[dbo].[Events]\n\t  WHERE startDate<GETDATE()-91")]
        public string rotateDataQuery {
            get {
                return ((string)(this["rotateDataQuery"]));
            }
        }
    }
}