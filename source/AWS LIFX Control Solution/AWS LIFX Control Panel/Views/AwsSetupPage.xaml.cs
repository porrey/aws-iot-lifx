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
using System;
using AwsLifxControl.Common;
using AwsLifxControl.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Windows.UI.Xaml;

namespace AwsLifxControl.Views
{
	public partial class AwsSetupPage : BindablePage
	{
		public AwsSetupPage()
		{
			this.InitializeComponent();
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

		public string AwsAccessKeyId
		{
			get
			{
				return this.ApplicationSettings.AwsAccessKeyId;
			}
			set
			{
				this.ApplicationSettings.AwsAccessKeyId = value;
				this.OnPropertyChanged();
			}
		}

		public string AwsSecretAccessKey
		{
			get
			{
				return this.ApplicationSettings.AwsSecretAccessKey;
			}
			set
			{
				this.ApplicationSettings.AwsSecretAccessKey = value;
				this.OnPropertyChanged();
			}
		}

		public bool? ShowAwsAccessKeyId
		{
			get
			{
				return this.ApplicationSettings.ShowAwsAccessKeyId;
			}
			set
			{
				this.ApplicationSettings.ShowAwsAccessKeyId = value.HasValue ? value.Value : false;
				this.OnPropertyChanged();
			}
		}

		public bool? ShowAwsSecretAccessKey
		{
			get
			{
				return this.ApplicationSettings.ShowAwsSecretAccessKey;
			}
			set
			{
				this.ApplicationSettings.ShowAwsSecretAccessKey = value.HasValue ? value.Value : false;
				this.OnPropertyChanged();
			}
		}		
		#endregion

		#region Event Handlers
		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.Frame.Navigate(typeof(MainPage));
			}
			catch (Exception ex)
			{
				this.PulishException(ex);
			}
		}
		#endregion
	}
}
