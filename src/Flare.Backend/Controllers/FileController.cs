using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Flare.Backend.Models;
using Flare.Backend.Services;
using Flare.Backend.Utils;

namespace Flare.Backend.Controllers {
    public class File {
        public int? id { get; set; }

        public string name { get; set; }

        public long size { get; set; }

        public string access_url => "https://api.flare.wtf/uploads/" + id + "/" + name;
    }
    
    public class FileController : Controller {
        private readonly ApplicationDbContext db;
        private readonly FileService fs;
        
        public static Expression<Func<file, File>> file = a => a == null ? null : new File {
            id = a.id,
            name = a.name,
            size = a.size
        };
	    
        public FileController(ApplicationDbContext db, FileService fs) {
            this.db = db;
            this.fs = fs;
        }

        [HttpPut("file/{name}")]
        public async Task<File> upload([FromServices] Token a, string name, CancellationToken ct) {
            if (Request.ContentLength == null) {
                throw new Exception("partial uploads are not supported");
            }

            var file = await fs.upload(Request.Body, name, Request.ContentLength, Request.Headers["Content-Type"], ct);

            return new File() {
                id = file.id,
                name = file.name,
                size = file.size
            };
        }
    }

}
