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
var https = require('https');

// ***
// *** Configure DynamoDB.
// ***
var AWS = require('aws-sdk');
var DDB = new AWS.DynamoDB();
AWS.config.update({region: "us-east-1"});

// ***
// *** LIFX API Uri.
// ***
var apiHost = 'api.lifx.com';

// ***
// *** Creates the headers requird for a LIFX API call.
// ***
function createHeaders(token, contentLength)
{
    console.log('Called createHeaders()');

    // ***
    // *** Create headers...
    // ***
    var headers =
    {
        'Accept':   '*/*',
        'Authorization': 'Bearer ' + token,
        'content-type': 'application/json',
        'content-length': contentLength
    };
    
    // ***
    // *** Log the values...
    // ***
    console.log("LIFX Headers: ", headers);
    
    // ***
    // *** Return the headers...
    // ***
    return headers;
}

// ***
// *** Gets the value of a configuration item
// *** from the database and returns it via
// *** the callback.
// ***
function getConfigurationItem(id, callback) 
{
    console.log('Called getConfigurationItem()');

    var q = DDB.getItem(
            {
                TableName: "Lifx",
                Key: 
                {
                    Id: { S: id } }
                }, 
                function(err, data) 
                {
                    if (err) 
                    {
                        // ***
                        // *** Log the result.
                        // ***
                        console.log('getConfigurationItem() Error = ', err);
                        
                        // ***
                        // *** Return an empty string.
                        // ***
                        callback('');
                    }
                    else 
                    {
                        // ***
                        // *** Log the result.
                        // ***
                        console.log('getConfigurationItem() Data = ', data);

                        // ***
                        // *** The configuration value is
                        // *** in an attribute named 'Value'.
                        // ***
                        callback(data.Item.Value.S);
                    }
            });
}

// ***
// *** Checks to see if any light assocaited with a
// *** scene is On. If at least one light is on then
// *** the scene is considered active.
// ***
function isSceneActive(token, sceneUuid, callback)
{
    console.log('Called isSceneActive()');

    // ***
    // *** Log the scene UUID...
    // ***
    console.log('Scene UUID = ', sceneUuid);

    // ***
    // *** Get the selector to return the list of lights
    // *** associated with the given scene.
    // ***
    getSceneLights(token, sceneUuid, function(selectors)
    {
        // ***
        // *** Log the selector...
        // ***
        console.log('selectors = ', selectors)
        
        // ***
        // *** This will hold the result of the check
        // *** which will be passed back to the calback function.
        // ***
        var isActive = false;

        // ***
        // *** Get the state of these lights
        // ***
        getState(token, selectors, function(state)
        {
            // ***
            // *** Enumerate the lights. If one or more is on
            // *** then the scene is considered active.
            // ***
            for(var i = 0; i < state.length; i++) 
            {
                var light = state[i];

                // ***
                // *** Check if the power is one for the first light/selection.
                // ***
                if (light.power == 'on')
                {
                    // ***
                    // *** The light is on, callback with true.
                    // ***
                    console.log('The light ' + light.label + ' is currently ON');
                    
                    // ***
                    // *** Set the isActive flag to true.
                    // ***
                    isActive = true;
                    break;
                }
                else
                {
                    // ***
                    // *** The light is off, callback with false.
                    // ***
                    console.log('The light ' + light.label + ' is currently OFF');
                }
            }
            
            // ***
            // *** Call the callback function with
            // *** the result;
            // ***
            callback(isActive);
        });
    });
}

// ***
// *** Get a list of lights that are part of a scene. This
// *** function will pass a selector that selects all of the
// *** lights in the given scene to the callback.
// ***
function getSceneLights(token, sceneUuid, callback)
{
    console.log('Called getSceneLights()');
    
    // ***
    // *** This array will contain the resulting
    // *** list of selectors.
    // ***
    var selectorsArray = [];
    
    // ***
    // *** Log the scene UUID...
    // ***
    console.log('Scene UUID = ', sceneUuid);
    
    // ***
    // *** Options for the HTTP request
    // ***
    var options = 
    {
        host:       apiHost,
        path:       '/v1/scenes',
        method:     'GET',
        headers:    createHeaders(token, 0)
    };
    
    // ***
    // *** Log the URL...
    // ***
    console.log('https://' + options.host + options.path);
    
    // ***
    // *** Create the HTTP request and 
    // *** get the state of the light(s).
    // ***
    var req = https.request(options, function(res) 
    {
        // ***
        // *** Log the status code...
        // ***
        console.log('getSceneLights(): Status Code = ', res.statusCode);

        // ***
        // *** Set the encoding...
        // ***
        res.setEncoding('utf-8');

        // ***
        // *** This variable holds the response JSON string.
        // ***
        var responseString = '';

        // ***
        // *** Add the data to responseString.
        // ***
        res.on('data', function(data)
        {
            responseString += data;
        });
        
        // ***
        // *** Process the response...
        // ***
        res.on('end', function()
        {
            // ***
            // *** Log the response...
            // ***
            console.log(responseString);
            
            // ***
            // *** Parse the JSON response into an oject.
            // ***
            var responseObject = JSON.parse(responseString);
            
            // ***
            // *** Enumerate the scenes
            // ***
            for(var i = 0; i < responseObject.length; i++) 
            {
                // ***
                // *** Get the scene object
                // ***
                var scene = responseObject[i];
                
                // ***
                // *** Log the scene name...
                // ***
                console.log("scene = ", scene.name);

                if (scene.uuid == sceneUuid)
                {
                    console.log("found scene");

                    // ***
                    // *** Enumerate the states in this scene
                    // ***
                    for(var j = 0; j < scene.states.length; j++) 
                    {
                        // ***
                        // *** Add the selector to the array
                        // ***
                        selectorsArray.push(scene.states[j].selector);
                        
                        // ***
                        // *** Log the selector
                        // ***
                        console.log("state.selector = ", scene.states[j].selector)
                    }
                    
                    break;
                }
            }
            
            // ***
            // *** Call the callback function a comma delimitted list
            // *** of selectors.
            // ***
            callback(selectorsArray.join(','));
        });
    }).on('error', function(e)
    {
        // ***
        // *** Assume the light is off, callback with false.
        // ***
        console.log('getSceneLights() failed: ', e);
        
        // ***
        // *** Callback with an empty value
        // ***
        callback('');
    });

    // ***
    // *** End the request...
    // ***
    req.end();
}

// ***
// *** Gets the state of the light(s) specified by 
// *** the given selector.
// ***
function getState(token, selector, callback)
{
    console.log('Called getState()');
    
    // ***
    // *** These are the options used 
    // *** for the HTTP request.
    // ***
    var options = 
    {
        host:       apiHost,
        path:       '/v1/lights/' + selector,
        method:     'GET',
        headers:    createHeaders(token, 0)
    };
    
    // ***
    // *** Log the URL...
    // ***
    console.log('https://' + options.host + options.path);
    
    // ***
    // *** Create the HTTP request and 
    // *** get the state of the light(s).
    // ***
    var req = https.request(options, function(res) 
    {
        // ***
        // *** Log the status code...
        // ***
        console.log('isPowered(): Status Code = ', res.statusCode);

        // ***
        // *** Set the encoding...
        // ***
        res.setEncoding('utf-8');

        // ***
        // *** This variable holds the response JSON string.
        // ***
        var responseString = '';

        // ***
        // *** Add the data to responseString.
        // ***
        res.on('data', function(data)
        {
            responseString += data;
        });
        
        // ***
        // *** Process the response...
        // ***
        res.on('end', function()
        {
            // ***
            // *** Log the response...
            // ***
            console.log(responseString);
            
            // ***
            // *** Parse the JSON response into an oject.
            // ***
            var responseObject = JSON.parse(responseString);
            callback(responseObject);
        });
    }).on('error', function(e)
    {
        // ***
        // *** Assume the light is off, callback with false.
        // ***
        console.log('isPowered() failed: ', e);
        callback(null);
    });

    // ***
    // *** End the request...
    // ***
    req.end();
}

// ***
// *** Sets the state of the lights specified by
// *** the gieven selector.
// ***
function setState(token, selector, requestData, callback)
{
    console.log('Called setState()');

    // ***
    // *** Log the requestd data being passed to the API.
    // ***
    console.log('Request Data = ', requestData);
    
    // ***
    // *** Options for the HTTP request.
    // ***
    var options = 
    {
        host:       apiHost,
        path:       '/v1/lights/' + selector + '/state',
        method:     'PUT',
        headers:    createHeaders(token, Buffer.byteLength(requestData))
    };
    
    // ***
    // *** Log the URL...
    // ***
    console.log('https://' + options.host + options.path);

    // ***
    // *** Create the HTTP request and 
    // *** make the API call.
    // ***
    var req = https.request(options, function(res) 
    {
        // ***
        // *** Log the status code...
        // ***
        console.log('setState(): Status Code = ', res.statusCode);

        // ***
        // *** Any result in 2xx is OK.
        // ***
        if (res.statusCode >= 200 && res.statusCode <= 299)
        {
            console.log('setState() was successful.');
            callback(true);
        } 
        else
        {
            console.log('setState() was successful.');
            callback(false);
        }

        // ***
        // *** Log the response...
        // ***
        res.on('end', function (responseData) 
        { 
            console.log(responseData);
        });
    }).on('error', function(e)
    {
        // ***
        // *** A failure occurred.
        // ***
        console.log('setState() failed: ', e);
        callback(false);
    });
    
    // ***
    // *** PUT/POST the data...
    // ***
    req.write(requestData);
    
    // ***
    // *** End the request...
    // ***
    req.end();
}

// ***
// *** Activate Scene. The calback returns true if
// *** successful and false otherwise.
// ***
function activateScene(token, sceneUuid, duration, callback)
{
    console.log('Called activateScene()');

    // ***
    // *** Log the scene UUID...
    // ***
    console.log('Scene UUID = ', sceneUuid);
    
    // ***
    // *** The data is the duration; the length of
    // *** time to transition the theme to on.
    // ***
    var requestData = '{ "duration": ' + duration + ' }';
    
    // ***
    // *** Log the requestd data being passed to the API.
    // ***
    console.log('Request Data = ', requestData);

    // ***
    // *** Options for the HTTP request
    // ***
    var options = 
    {
        host:       apiHost,
        path:       '/v1/scenes/scene_id:' + sceneUuid + '/activate',
        method:     'PUT',
        headers:    createHeaders(token, Buffer.byteLength(requestData))
    };
    
    // ***
    // *** Log the URL
    // ***
    console.log('https://' + options.host + options.path);

    // ***
    // *** Create the HTTP request and 
    // *** make the API call
    // ***
    var req = https.request(options, function(res) 
    {
        // ***
        // *** Log the status code
        // ***
        console.log('activateScene(): Status Code = ', res.statusCode);

        // ***
        // *** Any result in 2xx is OK
        // ***
        if (res.statusCode >= 200 && res.statusCode <= 299)
        {
            console.log('activateScene() was successful.');
            callback(true);
        } 
        else
        {
            console.log('activateScene() failed.');
            callback(false);
        }

        // ***
        // *** Log the response...
        // ***
        res.on('end', function (responseData) 
        { 
            console.log(responseData);
        });
    }).on('error', function(e)
    {
        // ***
        // *** A failure occurred.
        // ***
        console.log('activateScene() failed: ', e);
        callback(false);
    });
    
    // ***
    // *** PUT/POST the data
    // ***
    req.write(requestData);
    
    // ***
    // *** End the request
    // ***
    req.end();
}

// ***
// *** Deactivate Scene. The calback returns true if
// *** successful and false otherwise.
// ***
function deactivateScene(token, sceneUuid, duration, callback)
{
    console.log('Called deactivateScene()');

    // ***
    // *** Log the scene UUID...
    // ***
    console.log('Scene UUID = ', sceneUuid);
    
    // ***
    // *** Get the selector to return the list of lights
    // *** associated with the given scene.
    // ***
    getSceneLights(token, sceneUuid, function(selectors)
    {
        // ***
        // *** Log the selector...
        // ***
        console.log('selectors = ', selectors);
        
        // ***
        // *** Set the state of the lights to off
        // ***
        setState(token, selectors, '{"power": "off", "duration" : ' + duration + '}', function(result)
        {
            if (result)
            {
                callback(true);
            }
            else
            {
                callback(false);
            }
        });
    });
}

// ***
// *** Responds to an event with the given API Token,
// *** the Selector for the light source and the name
// *** of the scene to activate (if powering on). This
// *** function first checks if the power is on. If on,
// *** the power is toggled off otherwise the scene
// *** is activated.
// ***
function onRespondToEvent(token, sceneUuid, duration, callback)
{
    console.log('Called onRespondToEvent()');

    // ***
    // *** Get the current state of the selectd lights
    // *** true = on (powered)
    // *** false = off
    // ***
    isSceneActive(token, sceneUuid, function(isActive)
    {
        if (isActive)
        {
            console.log('The scene is currently active. Deactivating...');
            
            deactivateScene(token, sceneUuid, duration, function(result)
            {
                if (result)
                {
                    callback(true);
                }
                else
                {
                    callback(false);
                }
            });
        }
        else
        {
            console.log('The scene is currently inactive. Activating...');
            
            activateScene(token, sceneUuid, duration, function(result)
            {
                if (result)
                {
                    callback(true);
                }
                else
                {
                    callback(false);
                }
            });
        }
    });
}

// ***
// *** This is the exported handler for the button event
// ***
exports.handler = function(event, context)
{
    console.log('Called handler()');

    // ***
    // *** Log the type of event.
    // ***
    console.log(event.clickType);
    
    // ***
    // *** The API Token needed to access the LIFX API
    // *** is stored in a configuration value.
    // ***
    getConfigurationItem('LifxApiKey', function(token)
    {
        // ***
        // *** Check the click type of the .
        // ***
        var configurationItemName = '';
        var durationItemName = '';

        // ***
        // *** Two values are needed for each click. The name
        // *** of the LIFX scene to activate/deactivate and the
        // *** transition duration. These values are retrieved
        // *** from the DynamoDB table called Lifx.
        // ***
        switch(event.clickType)
        {
            case "SINGLE":
                // ***
                // *** Set the configuration item name to SingleClickScene
                // ***
                configurationItemName = 'SingleClickScene';
                durationItemName = 'SingleClickDuration';
                break;
            case "DOUBLE":
                // ***
                // *** Set the configuration item name to DoubleClickScene
                // ***
                configurationItemName = 'DoubleClickScene';
                durationItemName = 'DoubleClickDuration';
                break;
            case "LONG":
                // ***
                // *** Set the configuration item name to LongClickScene
                // ***
                configurationItemName = 'LongClickScene';
                durationItemName = 'LongClickDuration';
                break;
        }
        
        // ***
        // *** Get the name of the scene to activate for the
        // *** call to onRespondToEvent(). This is value is
        // *** set through the Windows 10 Control Panel
        // *** application.
        // ***
        getConfigurationItem(configurationItemName, function(sceneName)
        {
            // ***
            // *** The duration is the number of seconds the light(s)
            // *** will transition from off to on or on to off. This is
            // *** value is set through the Windows 10 Control Panel
            // *** application.
            // ***
            getConfigurationItem(durationItemName, function(duration)
            {
                // ***
                // *** Respond to the event...
                // ***
                onRespondToEvent(token, sceneName, duration, function(result)
                {
                    if (result)
                    {
                        // ***
                        // *** The process completed successfully.
                        // ***
                        console.log('The process completed successfully.');
                        context.succeed(event);
                    }
                    else
                    {
                        // ***
                        // *** The process failed; check the log for details.
                        // ***
                        console.log('The process completed with errors.');
                        context.fail(event);
                    }
                });
            });
        });
    });
};