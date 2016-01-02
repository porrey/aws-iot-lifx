﻿// Copyright © 2016 Daniel Porrey
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
using System.Threading.Tasks;

namespace AwsLifxControl.Interfaces
{
	public interface IApplicationSettingsRepository
	{
		string LifxApiKey { get; set; }
		string AwsAccessKeyId { get; set; }
		string AwsSecretAccessKey { get; set; }
		bool ShowLifxApiKey { get; set; }
		bool ShowAwsAccessKeyId { get; set; }
		bool ShowAwsSecretAccessKey { get; set; }
		Task ResetToDefaults();
	}
}