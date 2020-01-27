const uuidv4 = require('uuid/v4')

exports.handler = (event, context, callback) => {    
    // Send pre token generation data to Cloudwatch logs
    console.log("Cognito Pre token generation data");
    console.log("event = ", JSON.stringify(event, null, 4));
    console.log("context = ", JSON.stringify(context, null, 4));
    //console.log("App client ID = ", event.callerContext.clientId);
    //console.log("User ID = ", event.userName);
    event.response = {
        "claimsOverrideDetails": {
            "claimsToAddOrOverride": {
                "SessionId": uuidv4()
            }
        }
    };

    // Return to Amazon Cognito
    callback(null, event);
};