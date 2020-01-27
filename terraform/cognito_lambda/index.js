exports.handler = (event, context, callback) => {
    event.response.issueTokens = true;
    event.response.failAuthentication = false;
    context.done(null, event);
};