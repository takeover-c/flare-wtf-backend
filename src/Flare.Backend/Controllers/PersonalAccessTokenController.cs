using Flare.Backend.Models;
using Flare.Backend.Utils;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Flare.Backend.Controllers {
    public class PersonalAccessToken {
        public Guid? id { get; set; }

        public string name { get; set; }
        
        public string username { get; set; }

        public string password { get; set; }

        public DateTimeOffset? created_at { get; set; }

        public DateTimeOffset? accessed_at { get; set; }
    }

    public class PersonalAccessTokenController : Controller {
        ApplicationDbContext db;

        static Expression<Func<personal_access_token, PersonalAccessToken>> pac = a => new PersonalAccessToken {
            id = a.id,
            name = a.name,
            created_at = a.created_at
        };

        public PersonalAccessTokenController(ApplicationDbContext db) {
            this.db = db;
        }

        [HttpGet("token")]
        public async Task<List<PersonalAccessToken>> PersonalAccessTokens([FromServices] Token token) {
            return await db.pacs
                .Where(a => a.user_id == token.user_id)
                .Select(pac)
                .ToListAsync();
        }

        [HttpGet("token/{id}")]
        public async Task<PersonalAccessToken> PersonalAccessToken([FromServices] Token token, Guid id) {
            return await db.pacs
                .Where(a => a.user_id == token.user_id)
                .Where(a => a.id == id)
                .Select(pac)
                .SingleAsync();
        }

        [HttpDelete("token/{id}")]
        public async Task DeletePersonalAccessToken([FromServices] Token token, Guid id) {
            db.pacs.RemoveRange(
                db.pacs
                    .Where(a => a.user_id == token.user_id)
                    .Where(a => a.id == id)
            );

            if(await db.SaveChangesAsync() != 1) {
                throw new KeyNotFoundException();
            }
        }

        [HttpPut("token")]
        [HttpPatch("token/{id}")]
        public async Task<PersonalAccessToken> CreateOrEditPersonalAccessToken([FromServices] Token token, Guid? id, [FromBody] PersonalAccessToken Pac) {
            personal_access_token _pac;
            string username = null,
                   password = null;

            if (id == null) {
                _pac = new personal_access_token {
                    id = Guid.NewGuid(),
                    created_at = DateTimeOffset.UtcNow,
                    password_salt = new byte[128 / 8]
                };

                _pac.user_id = token.user_id;
                
                byte[] key = new byte[256 / 8];

                using (var rng = RandomNumberGenerator.Create()) {
                    rng.GetBytes(_pac.password_salt);

                    rng.GetBytes(key);
                }

                password = string.Concat(key.Select(b => b.ToString("X2")).ToArray());
                username = string.Concat(_pac.id.ToByteArray().Select(b => b.ToString("X2")).ToArray());

                _pac.password_hash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: _pac.password_salt,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8,
                    prf: KeyDerivationPrf.HMACSHA1
                );

                db.pacs.Add(_pac);
            } else {
                _pac = await db.pacs.SingleAsync(a => a.user_id == token.user_id && a.id == id);
                _pac.updated_at = DateTimeOffset.UtcNow;
            }

            if(Pac.name != null) {
                _pac.name = Pac.name;
            }

            await db.SaveChangesAsync();

            return new PersonalAccessToken {
                id = _pac.id,
                name = _pac.name,
                created_at = _pac.created_at,
                username = username,
                password = password
            };
        }
    }
}
