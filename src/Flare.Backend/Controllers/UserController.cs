using Flare.Backend.Models;
using Flare.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using PhoneNumbers;
using Microsoft.EntityFrameworkCore;

namespace Flare.Backend.Controllers {
    public class User {
        public int? id { get; set; }

        public string name { get; set; }

        public string email { get; set; }

		public bool? is_password_present { get; set; }

		public string password { get; set; }

		public DateTimeOffset? password_updated_at { get; set; }

        public string phone { get; set; }

        public int? type { get; set; }

        public DateTimeOffset? created_at { get; set; }
        
        public virtual File avatar { get; set; }
    }

    public class UserController : Controller {
        private readonly ApplicationDbContext db;

        public static readonly Expression<Func<user, User>> user = a => new User {
            id = a.id,
            name = a.name,
            email = a.email,
            phone = a.phone,
            type = a.type,
            created_at = a.created_at,
			is_password_present = a.password_hash != null,
			password_updated_at = a.password_set_at,
			avatar = a.avatar_id == null ? null : FileController.file.Invoke(a.avatar)
        };

        static UserController() {
	        user = user.Expand();
        }

        public UserController(ApplicationDbContext db) {
            this.db = db;
        }

        [HttpGet("user")]
        public async Task<List<User>> all_users([FromServices] Token token) {
            bool isAdmin = token.type == 512;

            return await db.users
                .Where(a => a.id == token.user_id || isAdmin)
                .Select(user)
                .ToListAsync();
        }

        [HttpGet("user/me")]
        [HttpGet("user/{id}")]
        public async Task<User> single_user([FromServices] Token token, int? id) {
	        if (id == null) {
		        id = token.user_id;
	        }
	        
            if(token.type != 512 && token.user_id != id) {
                throw new Exception("404");
            }

            return await db.users
                .Where(a => a.id == id)
                .Select(user)
                .SingleAsync();
        }
	    
	    [HttpDelete("user/{id}")]
	    public async Task delete_user([FromServices] Token token, int? id) {
		    if (token.type != 512) {
			    throw new Exception("you need to be an admin");
		    }

		    db.users
			   .RemoveRange(db.users.Where(a => a.id == id));

		    await db.SaveChangesAsync();
	    }

	    [HttpPut("user")]
	    [HttpPatch("user/{id}")]
	    public async Task<User> update_user([FromServices] Token token, int? id, [FromBody] User User) {
		    user user;

		    if (id == null) {
			    if (token.type != 512) {
				    throw new Exception("you need to be an admin");
			    }
			    
			    user = new user {
				    created_at = DateTimeOffset.Now,
				    type = User.type ?? token.type
			    };

			    db.users.Add(user);
		    } else {
			    if (token.type != 512 && id != token.user_id) {
				    throw new Exception("you need to be an admin");
			    }
			    
			    user = await db.users.SingleAsync(a => a.id == id);
			    user.updated_at = DateTimeOffset.Now;
		    }
			
		    if (User.name != null && user.name != User.name) {
			    user.name = User.name;
		    }
		    
		    if (User.email != null && user.email != User.email) {
			    user.email = User.email;
		    }

		    if (User.phone != null) {
			    var phoneUtil = PhoneNumberUtil.GetInstance();

				var phoneNumber = phoneUtil.ParseAndKeepRawInput(User.phone, "GR");
			    var phone = phoneUtil.Format(phoneNumber, PhoneNumberFormat.INTERNATIONAL);
				
				if (phone != user.phone) {
				    user.phone = phone;
			    }
			}

		    if (User.password != null) {
			    user.password_salt = new byte[128 / 8];
			    using (var rng = RandomNumberGenerator.Create()) {
				    rng.GetBytes(user.password_salt);
			    }

			    user.password_hash = KeyDerivation.Pbkdf2(
				    password: User.password,
				    salt: user.password_salt,
				    prf: KeyDerivationPrf.HMACSHA1,
				    iterationCount: 10000,
				    numBytesRequested: 256 / 8
			    );

			    user.password_set_at = user.created_at;
			}

		    if (User.avatar?.id != null) {
			    user.avatar_id = User.avatar.id;
		    }

			await db.SaveChangesAsync();

		    return await db.users
			    .Where(a => a.id == token.user_id)
			    .Select(UserController.user)
			    .SingleAsync();
	    }
    }
}
