// Copyright © 2016 Daniel Porrey
//
// This file is part of the AWS LIFX Control.
// 
// AWS LIFX Control is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AWS LIFX Control is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AWS LIFX Control. If not, see http://www.gnu.org/licenses/.
//
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.Storage;
using System;
using AwsLifxControl.Interfaces;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.PubSubEvents;
using AwsLifxControl.Common;
using AwsLifxControl.Models;

namespace AwsLifxControl.Repositories
{
	public class ApplicationSettingsRepository : BindableModel, IApplicationSettingsRepository
	{
		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		#region Settings
		public string LifxApiKey
		{
			get
			{
				return this.GetSetting<string>(MagicValue.Property.Name.LifxApiKey, MagicValue.Property.Default.LifxApiKey);
			}
			set
			{
				if (value != this.LifxApiKey)
				{
					this.SaveSetting<string>(MagicValue.Property.Name.LifxApiKey, value);
				}
			}
		}

		public string AwsAccessKeyId
		{
			get
			{
				return this.GetSetting<string>(MagicValue.Property.Name.AwsAccessKeyId, MagicValue.Property.Default.AwsAccessKeyId);
			}
			set
			{
				if (value != this.AwsAccessKeyId)
				{
					this.SaveSetting<string>(MagicValue.Property.Name.AwsAccessKeyId, value);
				}
			}
		}

		public string AwsSecretAccessKey
		{
			get
			{
				return this.GetSetting<string>(MagicValue.Property.Name.AwsSecretAccessKey, MagicValue.Property.Default.AwsSecretAccessKey);
			}
			set
			{
				if (value != this.AwsSecretAccessKey)
				{
					this.SaveSetting<string>(MagicValue.Property.Name.AwsSecretAccessKey, value);
				}
			}
		}

		public bool ShowLifxApiKey
		{
			get
			{
				return this.GetSetting<bool>(MagicValue.Property.Name.ShowLifxApiKey, MagicValue.Property.Default.ShowLifxApiKey);
			}
			set
			{
				if (value != this.ShowLifxApiKey)
				{
					this.SaveSetting<bool>(MagicValue.Property.Name.ShowLifxApiKey, value);
				}
			}
		}

		public bool ShowAwsAccessKeyId
		{
			get
			{
				return this.GetSetting<bool>(MagicValue.Property.Name.ShowAwsAccessKeyId, MagicValue.Property.Default.ShowAwsAccessKeyId);
			}
			set
			{
				if (value != this.ShowAwsAccessKeyId)
				{
					this.SaveSetting<bool>(MagicValue.Property.Name.ShowAwsAccessKeyId, value);
				}
			}
		}

		public bool ShowAwsSecretAccessKey
		{
			get
			{
				return this.GetSetting<bool>(MagicValue.Property.Name.ShowAwsSecretAccessKey, MagicValue.Property.Default.ShowAwsSecretAccessKey);
			}
			set
			{
				if (value != this.ShowAwsSecretAccessKey)
				{
					this.SaveSetting<bool>(MagicValue.Property.Name.ShowAwsSecretAccessKey, value);
				}
			}
		}	
		#endregion

		public T GetSetting<T>(string name, T defaultValue)
		{
			T returnValue = default(T);

			try
			{
				if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(name))
				{
					// ***
					// *** WintRT will not serialize all objects, so use Newtonsoft.Json
					// ***
					string json = (string)ApplicationData.Current.RoamingSettings.Values[name];
					returnValue = JsonConvert.DeserializeObject<T>(json);
				}
				else
				{
					returnValue = defaultValue;
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.ExceptionEvent>().Publish(new ExceptionEventArgs(ex));
			}

			return returnValue;
		}

		public void SaveSetting<T>(string name, T value)
		{
			try
			{
				// ***
				// *** Not all objects will serialize so use Newtonsoft.Json for everything
				// ***
				string json = JsonConvert.SerializeObject(value);
				ApplicationData.Current.RoamingSettings.Values[name] = json;

				this.OnPropertyChanged(name);
				this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(name, value));
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.ExceptionEvent>().Publish(new ExceptionEventArgs(ex));
			}
		}

		public Task ResetToDefaults()
		{
			try
			{
				//ApplicationData.Current.RoamingSettings.Values.Clear();

				// ***
				// *** This will fire the events for any poperty
				// *** that had a different value thn its default.
				// ***
				this.LifxApiKey = MagicValue.Property.Default.LifxApiKey;
				this.AwsAccessKeyId = MagicValue.Property.Default.AwsAccessKeyId;
				this.AwsSecretAccessKey = MagicValue.Property.Default.AwsSecretAccessKey;
				this.ShowLifxApiKey = MagicValue.Property.Default.ShowLifxApiKey;
				this.ShowAwsAccessKeyId = MagicValue.Property.Default.ShowAwsAccessKeyId;
				this.ShowAwsSecretAccessKey = MagicValue.Property.Default.ShowAwsSecretAccessKey;

				//this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.Name.LifxApiKey, this.LifxApiKey));
				//this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.Name.AwsAccessKeyId, this.AwsAccessKeyId));
				//this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.Name.AwsSecretAccessKey, this.AwsSecretAccessKey));
				//this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.Name.ShowLifxApiKey, this.ShowLifxApiKey));
				//this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.Name.ShowAwsAccessKeyId, this.ShowAwsAccessKeyId));
				//this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.Name.ShowAwsSecretAccessKey, this.ShowAwsSecretAccessKey));

			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.ExceptionEvent>().Publish(new ExceptionEventArgs(ex));
			}

			return Task.FromResult(0);
		}
	}
}
