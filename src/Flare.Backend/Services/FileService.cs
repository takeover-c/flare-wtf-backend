using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flare.Backend.Models;

namespace Flare.Backend.Services {
    public class FileService {
        private ApplicationDbContext db;

        public FileService(ApplicationDbContext db) {
            this.db = db;
        }

        public async Task<file> upload(Stream data, string filename, long? size = null, string mime = null, CancellationToken ct = default(CancellationToken)) {
            if (size == null) {
                size = data.Length;
            }

            using(var tran = await db.Database.BeginTransactionAsync(ct)) {
                var file = new file() {
                    name = filename,
                    size = size.Value,
                    content_type = mime,
                    created_at = DateTimeOffset.UtcNow
                };
    
                db.files.Add(file);
    
                await db.SaveChangesAsync(ct);

                Directory.CreateDirectory("../uploads/" + file.id);

                using (var _file = File.Create(Path.Combine("../uploads/", file.id.ToString(), file.name))) {
                    await data.CopyToAsync(_file, ct);
                }

                tran.Commit();
                return file;
            }
        }
    }

}
