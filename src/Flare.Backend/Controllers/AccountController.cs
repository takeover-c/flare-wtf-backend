using Flare.Backend.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Flare.Backend.Controllers {
    public class OAuth2TokenResponse {
        public string access_token { get; set; }

        public string token_type { get; set; }

        public int expires_in { get; set; }

        public string refresh_token { get; set; }
    }

    public class OAuth2Exception : Exception {
        public string type { get; }

        public OAuth2Exception(string type) : base(type) {
            this.type = type;
        }
    }

    public class AccountController : Controller {
        private ApplicationDbContext db;
        private SigningCredentials sc;

        public AccountController(ApplicationDbContext db, SigningCredentials sc) {
            this.db = db;
            this.sc = sc;
        }

        private async Task<client> get_client(string query = null) {
            if(query != null)
                Request.Query = new QueryCollection(QueryHelpers.ParseQuery(query));

            if (!Guid.TryParse(Request.Query["client_id"], out Guid _client_id)) {
                throw new OAuth2Exception("invalid_client");
            }

            var client = await db.clients.SingleOrDefaultAsync(a => a.id == _client_id);
            if (client == null) {
                throw new OAuth2Exception("invalid_client");
            }

            return client;
        }

        public static int GetHexVal(char hex) {
            int val = hex;
            return val - (val < 58 ? 48 : 55);
        }

        public static byte[] StringToByteArrayFastest(string hex) {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < (hex.Length >> 1); ++i) {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

	    [HttpPost("otoken")]
        public async Task<OAuth2TokenResponse> token() {
            string grant_type = Request.Form["grant_type"];

            var now = DateTime.Now;

            Guid client_id = Guid.Parse(Request.Form["client_id"]);

            var client = await db.clients.SingleOrDefaultAsync(a => a.id == client_id);
            if (client == null) {
                throw new OAuth2Exception("invalid_client");
            }

            var client_secret = Request.Form["client_secret"];

            if (!KeyDerivation.Pbkdf2(
                    client_secret,
                    client.client_secret_salt,
                    KeyDerivationPrf.HMACSHA1,
                    10000,
                    256 / 8
            ).SequenceEqual(client.client_secret_hash)) {
                throw new OAuth2Exception("invalid_client");
            }

            if(grant_type == "authorization_code" || grant_type == "password") {
                user user;
                refresh_token refresh_token;

                if(grant_type == "password") {
                    if (!client.trusted)
                        throw new Exception("the client here is not trusted.");

                    string username = Request.Form["username"];
                    string password = Request.Form["password"];

                    user = await db.users
                        .AsNoTracking()
                        .Where(k => k.email == username)
                        .SingleOrDefaultAsync();

                    if (user == null) {
                        throw new OAuth2Exception("invalid_grant");
                    }

                    if (!KeyDerivation.Pbkdf2(
                        password,
                        user.password_salt,
                        KeyDerivationPrf.HMACSHA1,
                        10000,
                        256 / 8
                    ).SequenceEqual(user.password_hash)) {
                        throw new OAuth2Exception("invalid_grant");
                    }

                    refresh_token = new refresh_token() {
                        id = Guid.NewGuid(),
                        user_id = user.id,
                        created_at = DateTimeOffset.UtcNow,
                        client_id = client.id
                    };
                    
                    db.refresh_tokens.Add(refresh_token);
                } else if(grant_type == "authorization_code") {
                    string code = Request.Form["code"];
                    string redirect_url = Request.Form["redirect_uri"];

                    var _code = StringToByteArrayFastest(code);
                    var id = new Guid(_code.Take(16).ToArray());
                    
                    refresh_token = await db.refresh_tokens.Include(a => a.user).SingleOrDefaultAsync(a => a.id == id);
                    
                    if(refresh_token?.exchange_code_hash == null || !refresh_token.exchange_code_hash.SequenceEqual(
                        KeyDerivation.Pbkdf2(
                            redirect_url + string.Concat(_code.Skip(16).Select(b => b.ToString("X2")).ToArray()),
                            refresh_token.exchange_code_salt,
                            iterationCount: 10000,
                            numBytesRequested: 256 / 8,
                            prf: KeyDerivationPrf.HMACSHA1
                        )
                     )) {
                        throw new OAuth2Exception("invalid_grant" + id +"_"+ string.Concat(_code.Skip(16).Select(b => b.ToString("X2")).ToArray()));
                    }

                    user = refresh_token.user;
                    refresh_token.exchange_code_hash = null;
                    refresh_token.exchange_code_salt = null;
                } else {
                    throw new NotImplementedException();
                }
                
                byte[] key = new byte[256 / 8];
                refresh_token.refresh_token_salt = new byte[128 / 8];

                using (var rng = RandomNumberGenerator.Create()) {
                    rng.GetBytes(refresh_token.refresh_token_salt);

                    rng.GetBytes(key);
                }

                var str_refresh_token = string.Concat(key.Select(b => b.ToString("X2")).ToArray());
                var _str_refresh_token = string.Concat(refresh_token.id.ToByteArray().Select(b => b.ToString("X2")).ToArray()) + str_refresh_token;

                refresh_token.refresh_token_hash = KeyDerivation.Pbkdf2(
                    password: client_secret + str_refresh_token,
                    salt: refresh_token.refresh_token_salt,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8,
                    prf: KeyDerivationPrf.HMACSHA1
                );

                await db.SaveChangesAsync();

                var handler = new JwtSecurityTokenHandler();
                
                var ci = new ClaimsIdentity();

                ci.AddClaim(new Claim("user_id", user.id.ToString()));
                ci.AddClaim(new Claim("user_type", user.type.ToString()));
                ci.AddClaim(new Claim("name", user.name));

                var access_token = handler.CreateEncodedJwt(issuer: "Flare", audience: "Flare Users", subject: ci, issuedAt: now, notBefore: now, expires: now + TimeSpan.FromHours(1), signingCredentials: sc);

                return new OAuth2TokenResponse { access_token = access_token, expires_in = (3600 * 1) - 1, token_type = "Bearer", refresh_token = _str_refresh_token };
            } else if (grant_type == "refresh_token") {
                var str_refresh_token = Request.Form["refresh_token"];

                var _code = StringToByteArrayFastest(str_refresh_token);
                var id = new Guid(_code.Take(16).ToArray());

                var token = await db.refresh_tokens
                    .Where(a => a.id == id && a.client_id == client_id)
                    .Include(a => a.user)
                    .SingleOrDefaultAsync();

                if (token == null || !token.refresh_token_hash.SequenceEqual(
                        KeyDerivation.Pbkdf2(
                            password: client_secret + string.Concat(_code.Skip(16).Select(b => b.ToString("X2")).ToArray()),
                            salt: token.refresh_token_salt,
                            iterationCount: 10000,
                            numBytesRequested: 256 / 8,
                            prf: KeyDerivationPrf.HMACSHA1
                        )
                     )) {
                    throw new OAuth2Exception("invalid_grant");
                }

                var ci = new ClaimsIdentity();

                ci.AddClaim(new Claim("user_id", token.user.id.ToString()));
                ci.AddClaim(new Claim("user_type", token.user.type.ToString()));
                ci.AddClaim(new Claim("name", token.user.name));

                var handler = new JwtSecurityTokenHandler();

                var access_token = handler.CreateEncodedJwt(issuer: "Flare", audience: "Flare Users", subject: ci, notBefore: now, issuedAt: now, expires: now + TimeSpan.FromHours(1), signingCredentials: sc);

                return new OAuth2TokenResponse { access_token = access_token, expires_in = (3600 * 1) - 1, token_type = "Bearer" };
            } else {
                throw new OAuth2Exception("unsupported_grant_type");
            }
        }
    }
}
