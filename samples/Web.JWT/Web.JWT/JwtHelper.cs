using BeetleX.EventArgs;
using BeetleX.FastHttpApi;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Web.JWT
{
    public class JWTHelper
    {
        public const string TOKEN_KEY = "Token";

        private string mIssuer = null;

        private string mAudience = null;

        private SecurityKey mSecurityKey;

        private SigningCredentials mSigningCredentials;

        private TokenValidationParameters mTokenValidation = new TokenValidationParameters();

        private JwtSecurityTokenHandler mJwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        public JWTHelper(string issuer, string audience, byte[] key)
        {
            mIssuer = issuer;
            mAudience = audience;
            mSecurityKey = new SymmetricSecurityKey(key);
            if (string.IsNullOrEmpty(mIssuer))
            {
                mTokenValidation.ValidateIssuer = false;
            }
            else
            {
                mTokenValidation.ValidIssuer = mIssuer;
            }
            if (string.IsNullOrEmpty(mAudience))
            {
                mTokenValidation.ValidateAudience = false;
            }
            else
            {
                mTokenValidation.ValidAudience = mAudience;
            }
            mTokenValidation.IssuerSigningKey = mSecurityKey;
            mSigningCredentials = new SigningCredentials(mSecurityKey, SecurityAlgorithms.HmacSha256);
            Expires = 60 * 24;
        }

        public int Expires { get; set; }

        public void ClearToken(HttpResponse response)
        {
            response.SetCookie(TOKEN_KEY, "", "/", DateTime.Now);
        }

        public void CreateToken(HttpResponse response, string name, string role, int timeout = 120)
        {
            string token = CreateToken(name, role, timeout);
            response.SetCookie(TOKEN_KEY, token, "/", DateTime.Now.AddDays(100));
        }

        public string CreateToken(string name, string role, int timeout = 60)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim("Name", name));
            claimsIdentity.AddClaim(new Claim("Role", role));
            var item = mJwtSecurityTokenHandler.CreateEncodedJwt(mIssuer, mAudience, claimsIdentity, DateTime.Now.AddMinutes(-5),
                DateTime.Now.AddMinutes(timeout), DateTime.Now,
               mSigningCredentials);
            return item;
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            return mJwtSecurityTokenHandler.ValidateToken(token, mTokenValidation, out var securityToken);
        }

        public UserInfo GetUserInfo(HttpRequest request)
        {
            string token = request.Cookies[TOKEN_KEY];
            if (string.IsNullOrEmpty(token))
                return null;
            try
            {
                return GetUserInfo(token);
            }
            catch (Exception e_)
            {
                HttpApiServer server = request.Server;
                if (server.EnableLog(LogType.Warring))
                {
                    server.Log(LogType.Warring, $"{request.RemoteIPAddress} get token error {e_.Message}");
                }
                return null;
            }

        }

        public UserInfo GetUserInfo(string token)
        {
            UserInfo userInfo = new UserInfo();
            if (!string.IsNullOrEmpty(token))
            {
                var info = ValidateToken(token);
                ClaimsIdentity identity = info?.Identity as ClaimsIdentity;
                userInfo.Name = identity?.Claims?.FirstOrDefault(c => c.Type == "Name")?.Value;
                userInfo.Role = identity?.Claims?.FirstOrDefault(c => c.Type == "Role")?.Value;
            }
            return userInfo;
        }

        public class UserInfo
        {
            public string Name;

            public string Role;
        }

        public static JWTHelper Default
        {
            get;
            set;
        }

        public static void Init()
        {
            byte[] key = new byte[128];
            new Random().NextBytes(key);
            Default = new JWTHelper("beetlex", "beetlex", key);
        }
    }

    public class TokenFilter : BeetleX.FastHttpApi.FilterAttribute
    {
        public override bool Executing(ActionContext context)
        {
            var user = JWTHelper.Default.GetUserInfo(context.HttpContext.Request);
            if (user == null)
            {
                var result = new ActionResult(401, "Access to this resource on the server is denied!");
                context.Result = result;
                return false;
            }
            else
            {
                context.HttpContext.Data.SetValue("_userName", user.Name);
                context.HttpContext.Data.SetValue("_userRole", user.Role);
            }
            return true;
        }
    }

    public class AdminFilter : TokenFilter
    {
        public override bool Executing(ActionContext context)
        {
            if (base.Executing(context))
            {
                string name = context.HttpContext.Data["_userRole"];
                if (name != "admin")
                {
                    var result = new ActionResult(401, "Access to this resource on the server is denied!");
                    context.Result = result;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}
