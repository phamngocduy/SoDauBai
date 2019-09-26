// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Helpers;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json.Linq;
using Owin.Security.Providers.Yahoo.Messages;

namespace Owin.Security.Providers.Yahoo
{
    internal class YahooAuthenticationHandler : AuthenticationHandler<YahooAuthenticationOptions>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const string StateCookie = "__YahooState";
        private const string AuthenticationEndpoint = "https://api.login.yahoo.com/oauth2/request_auth";
        private const string AccessTokenEndpoint = "https://api.login.yahoo.com/oauth2/get_token";
        private const string UserInformationEndpoint = "https://social.yahooapis.com/v1/user/me/profile";

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public YahooAuthenticationHandler(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public override async Task<bool> InvokeAsync()
        {
            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
            {
                return await InvokeReturnPathAsync();
            }
            return false;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            AuthenticationProperties properties = null;
            try
            {
                var query = Request.Query;
                var protectedRequestToken = Request.Cookies[StateCookie];

                var requestToken = Options.StateDataFormat.Unprotect(protectedRequestToken);

                if (requestToken == null)
                {
                    _logger.WriteWarning("Invalid state");
                    return null;
                }

                properties = requestToken.Properties;
                /*
                var returnedToken = query.Get("oauth_token");
                if (string.IsNullOrWhiteSpace(returnedToken))
                {
                    _logger.WriteWarning("Missing oauth_token");
                    return new AuthenticationTicket(null, properties);
                }

                if (returnedToken != requestToken.Token)
                {
                    _logger.WriteWarning("Unmatched token");
                    return new AuthenticationTicket(null, properties);
                }
                */
                var oauthVerifier = query.Get("code");
                if (string.IsNullOrWhiteSpace(oauthVerifier))
                {
                    _logger.WriteWarning("Missing or blank oauth_verifier");
                    return new AuthenticationTicket(null, properties);
                }

                var accessToken = await ObtainAccessTokenAsync(Options.ConsumerKey, Options.ConsumerSecret, requestToken, oauthVerifier);

                var userCard = await ObtainUserProfile(Options.ConsumerKey, Options.ConsumerSecret, accessToken, oauthVerifier);

                var context = new YahooAuthenticatedContext(Context, userCard, accessToken.UserId, accessToken.Token,
                    accessToken.TokenSecret)
                {
                    Identity = new ClaimsIdentity(
                        Options.AuthenticationType,
                        ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType)
                };

                if (!string.IsNullOrEmpty(context.UserId))
                {
                    context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.UserId,
                        "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                }
                if (!string.IsNullOrEmpty(context.NickName))
                {
                    context.Identity.AddClaim(new Claim(ClaimTypes.Name, context.NickName,
                        "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                }
                if (!string.IsNullOrEmpty(context.Email))
                {
                    context.Identity.AddClaim(new Claim(ClaimTypes.Email, context.Email,
                        "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                }
                if (!string.IsNullOrEmpty(context.UserId))
                {
                    context.Identity.AddClaim(new Claim("urn:yahoo:userid", context.UserId,
                        "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                }
                if (!string.IsNullOrEmpty(context.NickName))
                {
                    context.Identity.AddClaim(new Claim("urn:yahoo:nickname", context.NickName,
                        "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                }

                var idClaim = context.Identity.FindFirst(ClaimTypes.NameIdentifier);
                HttpContext.Current.Session["ExternalLoginInfo"] = new ExternalLoginInfo
                {
                    ExternalIdentity = context.Identity,
                    Login = new UserLoginInfo(idClaim.Issuer, idClaim.Value),
                    DefaultUserName = context.NickName,
                    Email = context.Email
                };

                context.Properties = requestToken.Properties;

                Response.Cookies.Delete(StateCookie);

                await Options.Provider.Authenticated(context);

                return new AuthenticationTicket(context.Identity, context.Properties);
            }
            catch (Exception ex)
            {
                _logger.WriteError("Authentication failed", ex);
                return new AuthenticationTicket(null, properties);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream.Dispose is idempotent")]
        protected override async Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode != 401)
            {
                return;
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                var requestPrefix = Request.Scheme + "://" + Request.Host;
                var callBackUrl = requestPrefix + RequestPathBase + Options.CallbackPath;

                var extra = challenge.Properties;
                if (string.IsNullOrEmpty(extra.RedirectUri))
                {
                    extra.RedirectUri = requestPrefix + Request.PathBase + Request.Path + Request.QueryString;
                }

                //var requestToken = await ObtainRequestTokenAsync(Options.ConsumerKey, Options.ConsumerSecret, callBackUrl, extra);
                var requestToken = new RequestToken
                {
                    Token = Options.ConsumerKey,
                    TokenSecret = Options.ConsumerSecret,
                    CallbackConfirmed = true,
                    Properties = extra
                };

                if (true/*requestToken.CallbackConfirmed*/)
                {
                    var yahooAuthenticationEndpoint = AuthenticationEndpoint +
                        "?client_id=" + Options.ConsumerKey +
                        "&response_type=code&redirect_uri=" + callBackUrl +
                        "&scope=openid&nonce=" + Guid.NewGuid().ToString("N");

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsSecure
                    };

                    Response.StatusCode = 302;
                    Response.Cookies.Append(StateCookie, Options.StateDataFormat.Protect(requestToken), cookieOptions);
                    Response.Headers.Set("Location", yahooAuthenticationEndpoint);
                }
                else
                {
                    //_logger.WriteError("requestToken CallbackConfirmed!=true");
                }
            }
            await Task.Run(() => { });
        }

        public async Task<bool> InvokeReturnPathAsync()
        {
            var model = await AuthenticateAsync();
            if (model == null)
            {
                _logger.WriteWarning("Invalid return state, unable to redirect.");
                Response.StatusCode = 500;
                return true;
            }

            var context = new YahooReturnEndpointContext(Context, model)
            {
                SignInAsAuthenticationType = Options.SignInAsAuthenticationType,
                RedirectUri = model.Properties.RedirectUri
            };
            model.Properties.RedirectUri = null;

            await Options.Provider.ReturnEndpoint(context);

            if (context.SignInAsAuthenticationType != null && context.Identity != null)
            {
                var signInIdentity = context.Identity;
                if (!string.Equals(signInIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                {
                    signInIdentity = new ClaimsIdentity(signInIdentity.Claims, context.SignInAsAuthenticationType, signInIdentity.NameClaimType, signInIdentity.RoleClaimType);
                }
                Context.Authentication.SignIn(context.Properties, signInIdentity);
            }

            if (context.IsRequestCompleted || context.RedirectUri == null) return context.IsRequestCompleted;
            if (context.Identity == null)
            {
                // add a redirect hint that sign-in failed in some way
                context.RedirectUri = WebUtilities.AddQueryString(context.RedirectUri, "error", "access_denied");
            }
            Response.Redirect(context.RedirectUri);
            context.RequestCompleted();

            return context.IsRequestCompleted;
        }

        private async Task<AccessToken> ObtainAccessTokenAsync(string consumerKey, string consumerSecret, RequestToken token, string verifier)
        {
            // http://developer.yahoo.com/oauth/guide/oauth-accesstoken.html

            _logger.WriteVerbose("ObtainAccessToken");

            //var nonce = Guid.NewGuid().ToString("N");
            var requestPrefix = Request.Scheme + "://" + Request.Host;
            var callBackUrl = requestPrefix + RequestPathBase + Options.CallbackPath;

            var authorizationParts = new SortedDictionary<string, string>
            {
                { "client_id", consumerKey },
                { "client_secret", consumerSecret },
                { "redirect_uri", callBackUrl },
                { "grant_type", "authorization_code" },
                { "code", verifier },
            };
            /*
            var parameterBuilder = new StringBuilder();
            foreach (var authorizationKey in authorizationParts)
            {
                parameterBuilder.AppendFormat("{0}={1}&", Uri.EscapeDataString(authorizationKey.Key), Uri.EscapeDataString(authorizationKey.Value));
            }
            parameterBuilder.Length--;
            var parameterString = parameterBuilder.ToString();

            var canonicalRequestBuilder = new StringBuilder();
            canonicalRequestBuilder.Append(HttpMethod.Post.Method);
            canonicalRequestBuilder.Append("&");
            canonicalRequestBuilder.Append(Uri.EscapeDataString(AccessTokenEndpoint));
            canonicalRequestBuilder.Append("&");
            canonicalRequestBuilder.Append(Uri.EscapeDataString(parameterString));

            var signature = ComputeSignature(consumerSecret, token.TokenSecret, canonicalRequestBuilder.ToString());
            authorizationParts.Add("oauth_signature", signature);
            
            var authorizationHeaderBuilder = new StringBuilder();
            authorizationHeaderBuilder.Append("Basic ");
            
            foreach (var authorizationPart in authorizationParts)
            {
                authorizationHeaderBuilder.AppendFormat(
                    "{0}=\"{1}\", ", authorizationPart.Key, Uri.EscapeDataString(authorizationPart.Value));
            }
            authorizationHeaderBuilder.Length = authorizationHeaderBuilder.Length - 2;
            */
            var request = new HttpRequestMessage(HttpMethod.Post, AccessTokenEndpoint);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(consumerKey + ":" + consumerSecret)));

            var formPairs = new List<KeyValuePair<string, string>>();
            foreach (var authorizationKey in authorizationParts)
            {
                formPairs.Add(new KeyValuePair<string, string>(authorizationKey.Key, authorizationKey.Value));
            }

            request.Content = new FormUrlEncodedContent(formPairs);

            var response = await _httpClient.SendAsync(request, Request.CallCancelled);

            if (!response.IsSuccessStatusCode)
            {
                _logger.WriteError("AccessToken request failed with a status code of " + response.StatusCode);
                response.EnsureSuccessStatusCode(); // throw
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseText);
            //var responseParameters = WebHelpers.ParseForm(responseText);

            return new AccessToken
            {
                Token = Uri.UnescapeDataString((string)responseObject.SelectToken("access_token")),
                //TokenSecret = Uri.UnescapeDataString(responseParameters["oauth_token_secret"]),
                // ReSharper disable once StringLiteralTypo
                //UserId = Uri.UnescapeDataString(responseParameters["xoauth_yahoo_guid"])
            };
        }

        private async Task<JObject> ObtainUserProfile(string consumerKey, string consumerSecret, AccessToken token, string verifier)
        {
            _logger.WriteVerbose("ObtainUserProfile");
            /*
            var nonce = Guid.NewGuid().ToString("N");
            var requestUrl = "https://query.yahooapis.com/v1/yql";

            var queryParts = new Dictionary<string, string>
            {
                { "q", "select * from social.profile where guid=me" },
                { "format", "json" }
            };
            var authorizationParts = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_token", token.Token },
                { "oauth_timestamp", GenerateTimeStamp() },
                { "oauth_verifier", verifier },
                { "oauth_version", "1.0" },
            };

            var parameterBuilder = new StringBuilder();
            foreach (var authorizationKey in authorizationParts.Union(queryParts).OrderBy(x => x.Key))
            {
                parameterBuilder.AppendFormat("{0}={1}&", Uri.EscapeDataString(authorizationKey.Key), Uri.EscapeDataString(authorizationKey.Value));
            }
            parameterBuilder.Length--;
            var parameterString = parameterBuilder.ToString();

            var canonicalRequestBuilder = new StringBuilder();
            canonicalRequestBuilder.Append(HttpMethod.Get.Method);
            canonicalRequestBuilder.Append("&");
            canonicalRequestBuilder.Append(Uri.EscapeDataString(requestUrl));
            canonicalRequestBuilder.Append("&");
            canonicalRequestBuilder.Append(Uri.EscapeDataString(parameterString));

            var signature = ComputeSignature(consumerSecret, token.TokenSecret, canonicalRequestBuilder.ToString());
            authorizationParts.Add("oauth_signature", signature);

            var authorizationHeaderBuilder = new StringBuilder();
            authorizationHeaderBuilder.Append("OAuth ");
            foreach (var authorizationPart in authorizationParts)
            {
                authorizationHeaderBuilder.AppendFormat(
                    "{0}=\"{1}\", ", authorizationPart.Key, Uri.EscapeDataString(authorizationPart.Value));
            }
            authorizationHeaderBuilder.Length = authorizationHeaderBuilder.Length - 2;
            */
            //requestUrl = WebUtilities.AddQueryString(requestUrl, queryParts);
            var request = new HttpRequestMessage(HttpMethod.Get, UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            var response = await _httpClient.SendAsync(request, Request.CallCancelled);

            if (!response.IsSuccessStatusCode)
            {
                _logger.WriteError("AccessToken request failed with a status code of " + response.StatusCode);
                response.EnsureSuccessStatusCode(); // throw
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseText);
            /*
            var queryObject = responseObject.SelectToken("query");
            if (queryObject == null) return null;
            var count = (int) queryObject.SelectToken("count");
            if (count <= 0) return null;
            var userCard = (JObject)queryObject.SelectToken("results.profile");
            */
            return (JObject)responseObject.SelectToken("profile");
        }
    }
}
