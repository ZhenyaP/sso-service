namespace IdentityProvider.API.Attributes
{
    using Common;
    using IdentityProvider.Common.Entities;
    using System;
    using System.Linq;
    using Controllers;
    using Entities;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Serilog;


    /// <summary>
    /// The SetCurrentClient Attribute.
    /// </summary>
    public class SetCurrentClientAttribute : ActionFilterAttribute
    {
        private readonly string[] _validClientNames;

        /// <summary>
        /// The log
        /// </summary>
        private static readonly ILogger Log = Serilog.Log.ForContext<SetCurrentClientAttribute>();

        public SetCurrentClientAttribute(params string[] validClientNames)
        {
            _validClientNames = validClientNames;
        }

        #region Private methods

        /// <summary>
        /// Gets the SSL Certificate's Common Name (CN) attribute value.
        /// </summary>
        /// <param name="certDistinguishedName">The SSL Certificate's Distinguished Name (DN) attribute value.</param>
        /// <returns></returns>
        private string GetCertCommonName(string certDistinguishedName)
        {
            var certCommonName = certDistinguishedName.Split(',').Select(x => x.Split('='))
                .FirstOrDefault(x => x.Length == 2 && x[0] == "CN")?[1];

            return certCommonName;
        }

        /// <summary>
        /// Gets the current client
        /// </summary>
        /// <returns>The current client</returns>
        private Client GetCurrentClient(HttpContext context, ConfigSettings configSettings)
        {
            string clientCertSubjectDistinguishedName,
                   clientCertIssuerDistinguishedName,
                   clientCertSerialNumber;

            if (!context.Request.Headers.TryGetValue(CommonConstants.HttpHeaders.SSLClientSDN, out var clientCertSubjectDistinguishedNameValues) ||
                string.IsNullOrEmpty(clientCertSubjectDistinguishedName = clientCertSubjectDistinguishedNameValues.ToString()) ||
                !context.Request.Headers.TryGetValue(CommonConstants.HttpHeaders.SSLClientIDN, out var clientCertIssuerDistinguishedNameValues) ||
                string.IsNullOrEmpty(clientCertIssuerDistinguishedName = clientCertIssuerDistinguishedNameValues.ToString()) ||
                !context.Request.Headers.TryGetValue(CommonConstants.HttpHeaders.SSLClientSerial, out var clientCertSerialNumberValues) ||
                string.IsNullOrEmpty(clientCertSerialNumber = clientCertSerialNumberValues.ToString()))
            {
                return null;
            }

            Log.Information($"In GetCurrentClient: clientCertSubjectDistinguishedName={clientCertSubjectDistinguishedName}");
            Log.Information($"In GetCurrentClient: clientCertIssuerDistinguishedName={clientCertIssuerDistinguishedName}");

            var clientCertSubjectCommonName = this.GetCertCommonName(clientCertSubjectDistinguishedName);
            var clientCertIssuerCommonName = this.GetCertCommonName(clientCertIssuerDistinguishedName);

            Log.Information($"In GetCurrentClient: clientCertSubjectCommonName={clientCertSubjectCommonName}");
            Log.Information($"In GetCurrentClient: clientCertIssuerCommonName={clientCertIssuerCommonName}");

            var configClientData = configSettings.Clients.FirstOrDefault(c =>
                c.ClientCert.SubjectCommonName == clientCertSubjectCommonName &&
                c.ClientCert.IssuerCommonName == clientCertIssuerCommonName &&
                c.ClientCert.SerialNumber.Replace(":", "") == clientCertSerialNumber.ToLowerInvariant());

            if (configClientData == null)
            {
                Log.Information("In GetCurrentClient: configClientData is null");
                return null;
            }
            var client = new Client
            {
                ConfigClientData = configClientData
            };

            var extraClientData = new ExtraClientData
            {
                SessionId = Guid.NewGuid().ToString(),
                ClientName = configClientData.ClientCert.SubjectCommonName
            };
            if (context.Request.Headers.TryGetValue(CommonConstants.HttpHeaders.ForwardedFor, out var clientIpAddress))
            {
                extraClientData.IpAddress = clientIpAddress;
            }
            if (context.Request.Headers.TryGetValue(CommonConstants.HttpHeaders.ForwardedProto, out var scheme))
            {
                extraClientData.ForwardedProto = scheme;
            }
            if (context.Request.Query.TryGetValue("username", out var userNameValues))
            {
                extraClientData.UserName = userNameValues.ToString();
            }

            client.ExtraClientData = extraClientData;

            return client;
        }

        #endregion

        /// <summary>
        /// Called before the action executes, after model binding is complete.
        /// </summary>
        /// <param name="filterContext">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext" />.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                if (!(filterContext.Controller is BaseController controller))
                {
                    return;
                }

                controller.CurrentClient = this.GetCurrentClient(
                    filterContext.HttpContext,
                    controller.ConfigSettings);

                if (controller.CurrentClient == null ||
                    this._validClientNames?.Length > 0 &&
                    !this._validClientNames.Contains(controller.
                        CurrentClient.ConfigClientData.ClientCert.SubjectCommonName))
                {
                    filterContext.Result = new UnauthorizedResult();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Exception in SetCurrentClientAttribute.OnActionExecuting: Message={e.Message}, StackTrace={e.StackTrace}");
                throw;
            }            
        }
    }
}
