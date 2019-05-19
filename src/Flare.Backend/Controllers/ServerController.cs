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
    public class Server {
        public int? id { get; set; }
        
        public string name { get; set; }
        
        public bool? proxy_active { get; set; }
        
        public bool? proxy_block_requests { get; set; }
        
        public virtual List<ServerDomain> domains { get; set; }
        
        public string origin_ip { get; set; }
        
        public DateTimeOffset created_at { get; set; }
        
        public DateTimeOffset? updated_at { get; set; }
    }

    public class ServerDomain {
        public int? id { get; set; }
        
        public string domain { get; set; }
    }

    public class ServerController : Controller {
        ApplicationDbContext db;

        static Expression<Func<server, Server>> server = a => new Server {
            id = a.id,
            name = a.name,
            proxy_active = a.proxy_active,
            proxy_block_requests = a.proxy_block_requests,
            domains = a.domains
                       .Select(b => new ServerDomain() {
                           id = b.id,
                           domain = b.domain
                       })
                       .ToList(),
            origin_ip = a.origin_ip,
            created_at = a.created_at,
            updated_at = a.updated_at
        };

        public ServerController(ApplicationDbContext db) {
            this.db = db;
        }

        [HttpGet("server")]
        public async Task<List<Server>> all([FromServices] Token token) {
            return await db.servers
                .Select(server)
                .ToListAsync();
        }

        [HttpGet("server/{id}")]
        public async Task<Server> single([FromServices] Token token, int id) {
            return await db.servers
                .Where(a => a.id == id)
                .Select(server)
                .SingleAsync();
        }

        [HttpDelete("server/{id}")]
        public async Task delete([FromServices] Token token, int id) {
            db.servers.RemoveRange(
                db.servers
                    .Where(a => a.id == id)
            );

            if(await db.SaveChangesAsync() != 1) {
                throw new KeyNotFoundException();
            }
        }

        [HttpPut("server")]
        [HttpPatch("server/{id}")]
        public async Task<Server> CreateOrEditPersonalAccessToken([FromServices] Token token, int? id, [FromBody] Server Server) {
            server server;
            
            if (id == null) {
                server = new server {
                    created_at = DateTimeOffset.UtcNow,
                    domains = new List<server_domain>()
                };

                db.servers.Add(server);
            } else {
                server = await db.servers
                     .Include(a => a.domains)
                     .SingleAsync(a => a.id == id);
                
                server.updated_at = DateTimeOffset.UtcNow;
            }

            if(Server.name != null) {
                server.name = Server.name;
            }
            
            if(Server.proxy_active != null) {
                server.proxy_active = Server.proxy_active.Value;
            }
            
            if(server.proxy_active) {
                if(Server.proxy_block_requests != null) {
                    server.proxy_block_requests = Server.proxy_block_requests.Value;
                }
                
                if(Server.origin_ip != null) {
                    server.origin_ip = Server.origin_ip;
                }
            } else {
                server.proxy_block_requests = false;
                server.origin_ip = null;
            }

            if(Server.domains != null) {
                server.domains.RemoveAll(a => !Server.domains.Any(b => b.id != a.id));
                foreach (var (_domain, Domain, order) in Server.domains
                            .Select((b, i) => (b.id == null ? null : server.domains.FirstOrDefault(c => c.id == b.id), b, i))
                            .ToList()) {
                    server_domain domain = _domain;
                    
                    if (domain == null) {
                        domain = new server_domain() {};
                        server.domains.Add(domain);
                    }

                    domain.order = order;

                    if (Domain.domain != null) {
                        domain.domain = Domain.domain;
                    }
                }
            }
            
            await db.SaveChangesAsync();

            return await single(token, server.id);
        }
    }
}
