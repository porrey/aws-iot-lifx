// Copyright © 2016 Daniel Porrey
//
// This file is part of the AWS LIFX Control Solution.
// 
// AWS LIFX Control Solution is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AWS LIFX Control Solution is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AWS LIFX Control Solution. If not, see http://www.gnu.org/licenses/.
//
using AwsLifxControl.Common;
using AwsLifxControl.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;
using Porrey.Uwp.IoT.Devices.Lifx;

namespace AwsLifxControl.Models
{
	public class LifxApiWrapper : ILifxApiWrapper
	{
		private readonly object _lock = new object();
		private LifxApi _api = null;
		private SubscriptionToken _subscriptionToken = null;

		[Dependency]
		protected IApplicationSettingsRepository ApplicationSettingsRepository { get; set; }

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		public LifxApi Api
		{
			get
			{
				// ***
				// *** Subscribe to ApplicationSettingChangedEvent
				// ***
				lock (_lock)
				{
					if (_subscriptionToken == null)
					{
						_subscriptionToken = this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Subscribe(this.OnApplicationSettingChangedEvent);
					}

					if (_api == null)
					{
						// ***
						// *** Initialzie the LIFX API
						// ***
						_api = new LifxApi(this.ApplicationSettingsRepository.LifxApiKey);
					}
				}

				return _api;
			}
		}

		protected void OnApplicationSettingChangedEvent(ApplicationSettingChangedEventArgs e)
		{
			// ***
			// *** Check for when the key changes
			// ***
			if (e.PropertyName == MagicValue.Property.Name.LifxApiKey)
			{
				_api = null;
			}
		}
	}
}
