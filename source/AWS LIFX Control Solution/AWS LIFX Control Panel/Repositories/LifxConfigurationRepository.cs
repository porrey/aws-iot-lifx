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
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using AwsLifxControl.Common;
using AwsLifxControl.Interfaces;
using AwsLifxControl.Models;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;

namespace AwsLifxControl.Repositories
{
	public class LifxConfigurationRepository : ILifxConfigurationRepository
	{
		private AmazonDynamoDBClient _client = null;
		private Table _table = null;
		private SubscriptionToken _subscriptionToken = null;

		[Dependency]
		protected IApplicationSettingsRepository ApplicationSettings { get; set; }

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		public async Task<bool> Initialize()
		{
			bool returnValue = false;

			if (!string.IsNullOrWhiteSpace(this.ApplicationSettings.AwsAccessKeyId) &&
				!string.IsNullOrWhiteSpace(this.ApplicationSettings.AwsSecretAccessKey))
			{
				// ***
				// *** Subscribe to ApplicationSettingChangedEvent
				// ***
				if (_subscriptionToken == null)
				{
					_subscriptionToken = this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Subscribe(this.OnApplicationSettingChangedEvent);
				}

				// ***
				// *** Get areference to the DynamoDB table in AWS
				// ***
				if (_table == null)
				{
					if (!await this.TableExists(this.ConfigTable))
					{
						await this.CreateTable(this.ConfigTable);
					}

					// ***
					// *** Get the table
					// ***
					_table = Table.LoadTable(this.Client, this.ConfigTable.TableName);
				}

				returnValue = true;
			}
			else
			{
				throw new Exception("AWS missing key information.");
			}

			return returnValue;
		}

		private AmazonDynamoDBClient Client
		{
			get
			{
				if (_client == null)
				{
					AWSCredentials credentials = new BasicAWSCredentials(this.ApplicationSettings.AwsAccessKeyId, this.ApplicationSettings.AwsSecretAccessKey);
					_client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
				}

				return _client;
			}
		}

		private Table Table
		{
			get
			{
				return _table;
			}
		}

		private async Task<bool> TableExists(CreateTableRequest table)
		{
			bool returnValue = false;

			// ***
			// *** Get a list of tables
			// ***
			ListTablesResponse response = await this.Client.ListTablesAsync();

			// ***
			// *** Check if the specified table exists
			// ***
			returnValue = response.TableNames.Contains(table.TableName);

			return returnValue;
		}

		private CreateTableRequest ConfigTable
		{
			get
			{
				return new CreateTableRequest
				{
					TableName = MagicValue.Lifx.Configuration.TableName,
					ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = MagicValue.Lifx.Configuration.ReadCapacityUnits, WriteCapacityUnits = MagicValue.Lifx.Configuration.WriteCapacityUnits },
					KeySchema = new List<KeySchemaElement>
					{
						new KeySchemaElement
						{
							AttributeName = MagicValue.Lifx.Configuration.Key,
							KeyType = KeyType.HASH
						}
					},
					AttributeDefinitions = new List<AttributeDefinition>
					{
						new AttributeDefinition { AttributeName = MagicValue.Lifx.Configuration.Key, AttributeType = ScalarAttributeType.S }
					}
				};
			}
		}

		private async Task<bool> CreateTable(CreateTableRequest table)
		{
			bool returnValue = false;

			if (!await this.TableExists(table))
			{
				CreateTableResponse response = await this.Client.CreateTableAsync(table);
				returnValue = (response.HttpStatusCode == System.Net.HttpStatusCode.OK);
			}

			return returnValue;
		}

		public async Task<T> GetItem<T>(string id, T defaultValue = default(T))
		{
			T returnValue = defaultValue;

			if (this.Table != null)
			{
				// ***
				// *** Set the key
				// ***
				Primitive partitionKey = new Primitive(id);

				// ***
				// ***
				// ***
				Document document = await this.Table.GetItemAsync(partitionKey);

				// ***
				// ***
				// ***
				if (document != null)
				{
					string value = document["Value"];
					returnValue = (T)Convert.ChangeType(value, typeof(T));
				}
			}

			return returnValue;
		}

		public async Task<bool> SetItem<T>(string id, T value)
		{
			bool returnValue = false;

			if (this.Table != null)
			{
				// ***
				// ***
				// ***
				Document configurationItem = new Document();

				configurationItem["Id"] = id;
				configurationItem["Value"] = (string)Convert.ChangeType(value, typeof(string));

				// ***
				// ***
				// ***
				await this.Table.PutItemAsync(configurationItem);
			}

			return returnValue;
		}

		public void Dispose()
		{
			if (_subscriptionToken != null)
			{
				this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Unsubscribe(_subscriptionToken);
				_subscriptionToken.Dispose();
				_subscriptionToken = null;
			}

			if (_client != null)
			{
				_client.Dispose();
				_client = null;
			}

			if (_table != null)
			{
				_table = null;
			}
		}

		protected void OnApplicationSettingChangedEvent(ApplicationSettingChangedEventArgs e)
		{
			// ***
			// *** Check for when the AWS keys change
			// ***
			if (e.PropertyName == MagicValue.Property.Name.AwsAccessKeyId || e.PropertyName == MagicValue.Property.Name.AwsSecretAccessKey)
			{
				// ***
				// *** Dispose this soit can be reinitialized
				// ***
				this.Dispose();
			}
		}
	}
}
