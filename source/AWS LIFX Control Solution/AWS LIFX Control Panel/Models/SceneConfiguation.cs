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

namespace AwsLifxControl.Models
{
	public class SceneConfiguation
	{
		public string SceneUuid { get; set; } = Guid.Empty.ToString();
		public double OnDuration { get; set; } = 1.0;
		public double OffDuration { get; set; } = 1.0;

		public static SceneConfiguation Create(string sceneUuid, double onDuration, double offDuration)
		{
			return new SceneConfiguation()
			{
				SceneUuid = sceneUuid,
				OnDuration = onDuration,
				OffDuration = offDuration
			};
		}
	}
}
