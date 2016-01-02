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
using AwsLifxControl.Common;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace AwsLifxControl.Converters
{
	public class DurationValueToTimeStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
			string format = resourceLoader.GetString(MagicValue.ResourceItem.DurationFormat);

			string returnValue = string.Format(format, 0);

			if (value is double)
			{
				returnValue = string.Format(format, value);
			}

			return returnValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}
