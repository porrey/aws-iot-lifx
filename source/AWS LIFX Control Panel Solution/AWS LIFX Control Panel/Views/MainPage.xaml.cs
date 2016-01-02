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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AwsLifxControl.Common;
using AwsLifxControl.Interfaces;
using AwsLifxControl.Models;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.ServiceLocation;
using Porrey.Uwp.IoT.Devices.Lifx;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AwsLifxControl.Views
{
	public partial class MainPage : BindablePage
	{
		private bool _initialized = false;
		private readonly ObservableCollection<ComboBoxItem> _lightSourceItems = new ObservableCollection<ComboBoxItem>();
		private readonly ObservableCollection<ComboBoxItem> _clickSceneItems = new ObservableCollection<ComboBoxItem>();
		private readonly ObservableCollection<ComboBoxItem> _doubleClickSceneItems = new ObservableCollection<ComboBoxItem>();
		private readonly ObservableCollection<ComboBoxItem> _longClickSceneItems = new ObservableCollection<ComboBoxItem>();
		private SubscriptionToken _exceptionSubscriptionToken = null;
		private SubscriptionToken _applicationSettingsSubscriptionToken = null;

		private readonly DispatcherTimer _singleClickTimer = new DispatcherTimer();
		private readonly DispatcherTimer _doubleClickTimer = new DispatcherTimer();
		private readonly DispatcherTimer _longClickTimer = new DispatcherTimer();

		public MainPage()
		{
			this.InitializeComponent();
		}

		protected ILifxApiWrapper LifxApiWrapper => ServiceLocator.Current.GetInstance<ILifxApiWrapper>();
		protected ILifxConfigurationRepository LifxConfiguration => ServiceLocator.Current.GetInstance<ILifxConfigurationRepository>();

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (!_initialized)
			{
				// ***
				// *** Subscribe to the Exception event
				// ***
				if (_exceptionSubscriptionToken == null)
				{
					_exceptionSubscriptionToken = this.EventAggregator.GetEvent<Events.ExceptionEvent>().Subscribe(this.OnExceptionEvent);
				}

				// ***
				// *** Subscribe to ApplicationSettingChangedEvent
				// ***
				if (_applicationSettingsSubscriptionToken == null)
				{
					_applicationSettingsSubscriptionToken = this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Subscribe(this.OnApplicationSettingChangedEvent);
				}

				// ***
				// *** The Tag value is used as the property name
				// *** in the SelectionChanged event
				// ***
				this.SingleClickSceneList.Tag = MagicValue.Lifx.Configuration.Property.SingleClickScene;
				this.DoubleClickSceneList.Tag = MagicValue.Lifx.Configuration.Property.DoubleClickScene;
				this.LongClickSceneList.Tag = MagicValue.Lifx.Configuration.Property.LongClickScene;

				// ***
				// *** Load all of the data
				// ***
				await this.LoadData();

				_initialized = true;
			}

			base.OnNavigatedTo(e);
		}

		private async Task<bool> LoadSceneItems(ObservableCollection<ComboBoxItem> items, string selectedItem = null)
		{
			bool returnValue = false;

			try
			{
				// ***
				// *** Lists the lights
				// ***
				IEnumerable<Scene> scenes = await this.LifxApiWrapper.Api.ListScenes();

				// ***
				// *** Load the individual lights
				// ***
				foreach (var scene in scenes)
				{
					// ***
					// *** The UUID is needed for the APi nd will be
					// *** stored in the database
					// ***
					ComboBoxItem comboBoxItem = new ComboBoxItem() { Content = scene.Name, Tag = scene.Uuid };

					if (selectedItem != null && scene.Uuid.ToString() == selectedItem)
					{
						comboBoxItem.IsSelected = true;
					}

					items.Add(comboBoxItem);
				}

				returnValue = true;
			}
			catch (Exception ex)
			{
				this.PulishException(ex);
				returnValue = false;
			}

			return returnValue;
		}

		private async Task LoadData()
		{
			try
			{
				this.IsBusy = true;

				if (!string.IsNullOrWhiteSpace(this.ApplicationSettings.AwsAccessKeyId) &&
					!string.IsNullOrWhiteSpace(this.ApplicationSettings.AwsSecretAccessKey) &&
					!string.IsNullOrWhiteSpace(this.ApplicationSettings.LifxApiKey))
				{
					// ***
					// *** Clear the timers
					// ***
					this.UnsetTimer(_singleClickTimer, SingleClickTimer_Tick);
					this.UnsetTimer(_doubleClickTimer, DoubleClickTimer_Tick);
					this.UnsetTimer(_longClickTimer, LongClickTimer_Tick);

					// ***
					// *** Clear any exception from the screen
					// ***
					await this.ClearPageException();

					// ***
					// *** Clear the current lists
					// ***
					this.SingleClickSceneItems.Clear();
					this.DoubleClickSceneItems.Clear();
					this.LongClickSceneItems.Clear();

					// ***
					// *** Ensure that the configuration is initialized
					// ***
					bool configurationInitializationSuccessful = false;
					try
					{
						configurationInitializationSuccessful = await this.LifxConfiguration.Initialize();
					}
					catch
					{
						this.EventAggregator.GetEvent<Events.ExceptionEvent>().Publish(
							new ExceptionEventArgs(
								this.ResourceLoader.GetString(MagicValue.ResourceItem.AwsConfigurationExceptionTitle),
								this.ResourceLoader.GetString(MagicValue.ResourceItem.AwsConfigurationExceptionMessage)));
					}

					if (configurationInitializationSuccessful)
					{
						// ***
						// *** Restore he duration values
						// ***
						this.SingleClickDuration = await this.LifxConfiguration.GetItem<double>(MagicValue.Lifx.Configuration.Property.SingleClickDuration, 1.0);
						this.DoubleClickDuration = await this.LifxConfiguration.GetItem<double>(MagicValue.Lifx.Configuration.Property.DoubleClickDuration, 1.0);
						this.LongClickDuration = await this.LifxConfiguration.GetItem<double>(MagicValue.Lifx.Configuration.Property.LongClickDuration, 1.0);

						// ***
						// *** Get the current configuration items
						// ***
						string singleClickScene = await this.LifxConfiguration.GetItem<string>(MagicValue.Lifx.Configuration.Property.SingleClickScene);
						string dobleClickScene = await this.LifxConfiguration.GetItem<string>(MagicValue.Lifx.Configuration.Property.DoubleClickScene);
						string longClickScene = await this.LifxConfiguration.GetItem<string>(MagicValue.Lifx.Configuration.Property.LongClickScene);

						// ***
						// *** Load the ComboBox items
						// ***
						if (await this.LoadSceneItems(this.SingleClickSceneItems, singleClickScene))
						{
							if (await this.LoadSceneItems(this.DoubleClickSceneItems, dobleClickScene))
							{
								await this.LoadSceneItems(this.LongClickSceneItems, longClickScene);
							}
						}
					}
				}
				else
				{
					this.EventAggregator.GetEvent<Events.ExceptionEvent>().Publish(
						new ExceptionEventArgs(
							this.ResourceLoader.GetString(MagicValue.ResourceItem.MissingConfigurationMessage),
							this.ResourceLoader.GetString(MagicValue.ResourceItem.MissingConfigurationTitle)));
				}
			}
			finally
			{
				// ***
				// *** Set the timers
				// ***
				this.SetTimer(_singleClickTimer, SingleClickTimer_Tick);
				this.SetTimer(_doubleClickTimer, DoubleClickTimer_Tick);
				this.SetTimer(_longClickTimer, LongClickTimer_Tick);

				this.IsBusy = false;
			}
		}

		private void SetTimer(DispatcherTimer timer, EventHandler<object> tick)
		{
			timer.Tick += tick;
			timer.Interval = TimeSpan.FromMilliseconds(750);
		}

		private void UnsetTimer(DispatcherTimer timer, EventHandler<object> tick)
		{
			timer.Stop();
			timer.Tick -= tick;
			timer.Interval = TimeSpan.Zero;
		}

		#region Bindings
		private bool _isBusy = false;
		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			set
			{
				this.SetProperty(ref _isBusy, value);
			}
		}

		public ObservableCollection<ComboBoxItem> SingleClickSceneItems
		{
			get
			{
				return _clickSceneItems;
			}
		}

		public ObservableCollection<ComboBoxItem> DoubleClickSceneItems
		{
			get
			{
				return _doubleClickSceneItems;
			}
		}

		public ObservableCollection<ComboBoxItem> LongClickSceneItems
		{
			get
			{
				return _longClickSceneItems;
			}
		}

		private double _singleClickDuration = 1;
		public double SingleClickDuration
		{
			get
			{
				return _singleClickDuration;
			}
			set
			{
				this.SetProperty(ref _singleClickDuration, value);

				// ***
				// *** Reset the timer so that the user can
				// *** make multiple changes without updating
				// *** the database everytime
				// ***
				_singleClickTimer.Stop();
				_singleClickTimer.Start();
			}
		}

		private double _doubleClickDuration = 1;
		public double DoubleClickDuration
		{
			get
			{
				return _doubleClickDuration;
			}
			set
			{
				this.SetProperty(ref _doubleClickDuration, value);

				// ***
				// *** Reset the timer so that the user can
				// *** make multiple changes without updating
				// *** the database everytime
				// ***
				_doubleClickTimer.Stop();
				_doubleClickTimer.Start();
			}
		}

		private double _longClickDuration = 1;
		public double LongClickDuration
		{
			get
			{
				return _longClickDuration;
			}
			set
			{
				this.SetProperty(ref _longClickDuration, value);

				// ***
				// *** Reset the timer so that the user can
				// *** make multiple changes without updating
				// *** the database everytime
				// ***
				_longClickTimer.Stop();
				_longClickTimer.Start();
			}
		}

		private bool _isPaneOpen = false;
		public bool IsPaneOpen
		{
			get
			{
				return _isPaneOpen;
			}
			set
			{
				this.SetProperty(ref _isPaneOpen, value);
			}
		}
		#endregion

		#region Event Handlers
		private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox combo = sender as ComboBox;

			if (combo != null)
			{
				if (combo.Tag != null)
				{
					// ***
					// *** Get the selected item
					// ***
					ComboBoxItem selectedValue = combo.SelectedValue as ComboBoxItem;

					if (selectedValue != null)
					{
						// ***
						// *** This will hold value of the property being saved to the DynamoDB
						// ***
						string value = string.Empty;

						// ***
						// *** The value is in the Tag property of
						// *** of the ComBoxItem. The ComboBox.Tag 
						// *** property contains the property name.
						// ***
						await this.LifxConfiguration.SetItem(combo.Tag.ToString(), selectedValue.Tag.ToString());
					}
				}
			}
		}

		private void AwsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			this.IsPaneOpen = false;
			this.Frame.Navigate(typeof(AwsSetupPage), null);
		}

		private void LifxMenuItem_Click(object sender, RoutedEventArgs e)
		{
			this.IsPaneOpen = false;
			this.Frame.Navigate(typeof(LifxSetupPage), null);
		}

		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			this.IsPaneOpen = !this.IsPaneOpen;
		}

		private async void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsBusy = true;

				this.IsPaneOpen = !this.IsPaneOpen;
				await this.LoadData();
			}
			finally
			{
				this.IsBusy = false;
			}
		}

		private void ResetSettingsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsBusy = true;

				this.IsPaneOpen = !this.IsPaneOpen;
				this.ApplicationSettings.ResetToDefaults();
			}
			finally
			{
				this.IsBusy = false;
			}
		}

		private void OnExceptionEvent(ExceptionEventArgs e)
		{
			this.PageExceptionTitle = e.Title;
			this.PageExceptionDescription = e.Description;
			this.PageHasException = true;
		}

		protected async void OnApplicationSettingChangedEvent(ApplicationSettingChangedEventArgs e)
		{
			// ***
			// *** Check for when the keys change to load/reload the data
			// ***
			if (e.PropertyName == MagicValue.Property.Name.LifxApiKey ||
				e.PropertyName == MagicValue.Property.Name.AwsAccessKeyId ||
				e.PropertyName == MagicValue.Property.Name.AwsSecretAccessKey)
			{
				await this.LoadData();
			}
		}

		private async void SingleClickTimer_Tick(object sender, object e)
		{
			await this.LifxConfiguration.SetItem(MagicValue.Lifx.Configuration.Property.SingleClickDuration, this.SingleClickDuration.ToString());
		}

		private async void DoubleClickTimer_Tick(object sender, object e)
		{
			await this.LifxConfiguration.SetItem(MagicValue.Lifx.Configuration.Property.DoubleClickDuration, this.DoubleClickDuration.ToString());
		}

		private async void LongClickTimer_Tick(object sender, object e)
		{
			await this.LifxConfiguration.SetItem(MagicValue.Lifx.Configuration.Property.LongClickDuration, this.LongClickDuration.ToString());
		}
		#endregion
	}
}
