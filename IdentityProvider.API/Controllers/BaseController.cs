namespace IdentityProvider.API.Controllers
{
    using Entities;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    using IdentityProvider.Common.Entities;

    /// <summary>
    /// The Base controller.
    /// </summary>    
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// The config settings
        /// </summary>
        public readonly ConfigSettings ConfigSettings;

        /// <summary>
        /// Gets or sets the current client.
        /// </summary>
        /// <value>The current client.</value>
        public Client CurrentClient { get; set; }

        /// <summary>
        /// Gets or sets the Identity Provider token issuer.
        /// </summary>
        /// <value>The Identity Provider token issuer.</value>
        public string TokenIssuer => $"{this.CurrentClient.ExtraClientData.ForwardedProto}://{this.Request.Host}";

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenController"/> class.
        /// </summary>
        /// <param name="configSettings">The Config Settings.</param>
        public BaseController(IOptions<ConfigSettings> configSettings)
        {
            this.ConfigSettings = configSettings.Value;
        }
    }
}
