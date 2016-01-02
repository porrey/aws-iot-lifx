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
namespace AwsLifxControl.Common
{
	public static class MagicValue
	{
		public static class Lifx
		{
			public static class Configuration
			{
				public const string TableName = "Lifx";
				public const string Key = "Id";
				public const int ReadCapacityUnits = 5;
				public const int WriteCapacityUnits = 5;

				public static class Property
				{
					public const string LifxApiKey = "LifxApiKey";
					public const string SingleClickScene = "SingleClickScene";
					public const string SingleClickDuration = "SingleClickDuration";
					public const string DoubleClickScene = "DoubleClickScene";
					public const string DoubleClickDuration = "DoubleClickDuration";
					public const string LongClickScene = "LongClickScene";
					public const string LongClickDuration = "LongClickDuration";
				}
			}
		}

		public static class Property
		{
			public static class Default
			{
				public const string LifxApiKey = null;
				public const string AwsAccessKeyId = null;
				public const string AwsSecretAccessKey = null;
				public const bool ShowLifxApiKey = false;
				public const bool ShowAwsAccessKeyId = false;
				public const bool ShowAwsSecretAccessKey = false;
			}

			public static class Name
			{
				public const string LifxApiKey = "LifxApiKey";
				public const string AwsAccessKeyId = "AwsAccessKeyId";
				public const string AwsSecretAccessKey = "AwsSecretAccessKey";
				public const string ShowLifxApiKey = "ShowLifxApiKey";
				public const string ShowAwsAccessKeyId = "ShowAwsAccessKeyId";
				public const string ShowAwsSecretAccessKey = "ShowAwsSecretAccessKey";
			}
		}

		// ***
		// *** These items must match a key in the string resource file
		// ***
		public static class ResourceItem
		{
			public const string LifxApiExceptionTitle = "Exception_LifxApiExceptionTitle";
			public const string LifxApiExceptionMessage = "Exception_LifxApiExceptionMessage";
			public const string AwsConfigurationExceptionTitle = "Exception_AwsConfigurationExceptionTitle";
			public const string AwsConfigurationExceptionMessage = "Exception_AwsConfigurationExceptionMessage";
			public const string MissingConfigurationMessage = "Exception_MissingConfigurationMessage";
			public const string MissingConfigurationTitle = "Exception_MissingConfigurationTitle";
			public const string DurationFormat = "DurationFormat";
		}
	}
}
