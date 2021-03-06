﻿//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the AsyncInitializationMiddleware type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using IdentityProvider.Common.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityProvider.API.Middlewares
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    using Entities;
    using Helpers;
    using IdentityProvider.Common.Helpers;

    /// <summary>
    /// The Async Initialization Middleware.
    /// </summary>
    public class AsyncInitializationMiddleware
    {
        /// <summary>
        /// The next middleware.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The initialization task.
        /// </summary>
        private Task _initializationTask;

        /// <summary>
        /// The AWS Cognito Helper.
        /// </summary>
        private readonly AWSCognitoHelper _awsCognitoHelper;

        /// <summary>
        /// The AWS Cognito Client Secret Helper.
        /// </summary>
        private readonly AWSCognitoClientSecretHelper _awsCognitoClientSecretHelper;

        /// <summary>
        /// The JWT Token Helper.
        /// </summary>
        private readonly JwtTokenHelper _jwtTokenHelper;

        /// <summary>
        /// The Config Settings.
        /// </summary>
        private readonly ConfigSettings _configSettings;

        private ILogger<AsyncInitializationMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncInitializationMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        /// <param name="jwtTokenHelper">The JWT token helper.</param>
        /// <param name="awsCognitoClientSecretHelper">The AWS Cognito Client Secret Helper.</param>        
        /// <param name="lifetime">The application lifetime.</param>
        /// <param name="awsCognitoHelper">The AWS Cognito Helper.</param>
        /// <param name="configSettings">The Config Settings.</param>
        /// <param name="logger">Logger</param>
        public AsyncInitializationMiddleware(RequestDelegate next,
            IHostApplicationLifetime lifetime,
            AWSCognitoHelper awsCognitoHelper,
            JwtTokenHelper jwtTokenHelper,
            AWSCognitoClientSecretHelper awsCognitoClientSecretHelper,
            IOptions<ConfigSettings> configSettings,
            ILogger<AsyncInitializationMiddleware> logger
            )
        {
            this._next = next;
            this._awsCognitoHelper = awsCognitoHelper;
            this._awsCognitoClientSecretHelper = awsCognitoClientSecretHelper;
            this._configSettings = configSettings.Value;
            this._jwtTokenHelper = jwtTokenHelper;
            this._logger = logger;

            // Start initialization when the app starts
            var startRegistration = default(CancellationTokenRegistration);
            var registration = startRegistration;
            lifetime.ApplicationStarted.Register(() =>
            {
                _initializationTask = InitializeAsync(lifetime.ApplicationStopping);
                registration.Dispose();
            });
        }

        /// <summary>
        /// Calls Cognito endpoint for all clients - magic experiment
        /// (for boosting the performance of all further token requests)
        /// </summary>
        /// <returns>The Task object.</returns>
        private async Task CallCognitoEndpointForAllClientsAsync()
        {
            const int numberOfCalls = 1;
            var getTokenTasks = new List<Task<string>>();
            foreach (var client in _configSettings.Clients)
            {
                if (client.Cognito != null)
                {
                    client.Cognito.ClientApp.ClientSecret = _awsCognitoClientSecretHelper.GetClientSecretForCognitoClient(client);
                    if (string.IsNullOrEmpty(client.Cognito.ClientApp.ClientSecret))
                    {
                        continue;
                    }

                    for (int callNum = 0; callNum < numberOfCalls; callNum++)
                    {
                        getTokenTasks.Add(_awsCognitoHelper.GetClientCredentialsTokenAsync(client));
                    }
                }
            }

            await Task.WhenAll(getTokenTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads RSA Key Data to the In-Memory Cache.
        /// </summary>
        /// <returns></returns>
        private RsaKeyData LoadRsaKeyData()
        {
            return this._jwtTokenHelper.RsaKeyData;
        }

        /// <summary>
        /// Initializes the Web API application.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object.</returns>
        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Initialization starting");

                // Do async initialization here
                await CallCognitoEndpointForAllClientsAsync();
                //await Task.FromResult(1);
                LoadRsaKeyData();

                _logger.LogInformation("Initialization complete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initialization failed");
                throw;
            }
        }

        /// <summary>
        /// The middleware execution logic.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The Task object.</returns>
        public async Task Invoke(HttpContext context)
        {
            // Take a copy to avoid race conditions
            var initializationTask = _initializationTask;
            if (initializationTask != null)
            {
                // Wait until initialization is complete before passing the request to next middleware
                await initializationTask;

                // Clear the task so that we don't await it again later.
                _initializationTask = null;
            }

            // Pass the request to the next middleware
            await _next(context);
        }
    }
}
