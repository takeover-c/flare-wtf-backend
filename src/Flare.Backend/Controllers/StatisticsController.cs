using System.Linq;
using System.Threading.Tasks;
using Flare.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flare.Backend.Controllers {
    public class StatisticsController : ControllerBase {
        private readonly ApplicationDbContext db;

        public StatisticsController(ApplicationDbContext db) {
            this.db = db;
        }

        [HttpGet("server/{server_id}/statistic")]
        public async Task<object> statistics(int server_id) {
            var stats = await db.servers
                .Where(a => a.id == server_id)
                .Select(a => new {
                    start_date = a.requests.Select(b => b.request_date).OrderBy(b => b).FirstOrDefault(),
                    end_date = a.requests.Select(b => b.request_date).OrderByDescending(b => b).FirstOrDefault(),
                    total_requests = a.requests.Count,
                    total_response_bytes = a.requests.Sum(b => (long)b.response_length),
                    error_responses = a.requests
                            .Count(b => b.response_code != null && b.response_code >= 500 && b.response_code <= 599),
                    distinct_ips = a.requests
                            .Select(b => b.ip_id)
                            .Distinct()
                            .Count(),
                    flagged_requests = a.requests
                            .Count(b => b.flags != 0),
                    by_vuln = a.requests
                            .Where(b => b.flags != 0)
                            .GroupBy(b => b.flags)
                            .Select(b => new { b.Key, Count = b.Count() })
                            .ToList(),
                    top_pages = a.requests
                            .Where(b => b.flags != 0)
                            .GroupBy(b => b.request_path)
                            .OrderByDescending(b => b.Count())
                            .Take(5)
                            .Select(b => new { b.Key, Count = b.Count() })
                            .ToList(),
                    top_countries = a.requests
                            .Where(b => b.flags != 0)
                            .GroupBy(b => b.ip.country_name)
                            .OrderByDescending(b => b.Count())
                            .Take(5)
                            .Select(b => new { b.Key, Count = b.Count() })
                            .ToList(),
                    by_hourly = a.requests
                            .Where(b => b.flags != 0)
                            .GroupBy(b => b.request_date.Value.LocalDateTime.Hour)
                            .OrderByDescending(b => b.Count())
                            .Take(5)
                            .Select(b => new { b.Key, Count = b.Count() })
                            .ToList(),
                    by_hourly_all = a.requests
                         .OrderBy(b => b.request_date)
                         .GroupBy(b => b.request_date.Value.LocalDateTime.Hour + ".00 - " + (b.request_date.Value.LocalDateTime.Hour + 1) + ".00")
                        // .GroupBy(b => b.request_date.Value.LocalDateTime.Day + "." + b.request_date.Value.LocalDateTime.Month + "." + b.request_date.Value.LocalDateTime.Year
                        //               + " " + b.request_date.Value.LocalDateTime.Hour + ".00 - " + (b.request_date.Value.LocalDateTime.Hour + 1) + ".00")
                         .Select(b => new { b.Key, Requests = b.Count(), Attacks = b.Count(c => c.flags != 0) })
                         .ToList(),
                    by_really_hourly = a.requests
                                 .OrderBy(b => b.request_date.Value.LocalDateTime.Hour)
                                 .GroupBy(b => b.request_date.Value.LocalDateTime.Hour)
                                 .Select(b => new { b.Key, Count = b.Count() })
                                 .ToList(),
                    by_countries = a.requests
                            .GroupBy(b => b.ip.country_name)
                            .OrderByDescending(b => b.Count())
                            .Select(b => new { b.Key, Count = b.Count(), Good = b.Count(c => c.flags == 0), Bad = b.Count(c => c.flags != 0) })
                            .ToList(),
                    by_countries2 = a.requests
                            .Where(b => b.flags != 0)
                            .GroupBy(b => b.ip.country_name)
                            .OrderByDescending(b => b.Count())
                            .Select(b => new { b.Key, Count = b.Count() })
                            .ToList(),
                    by_country_hourly = a.requests
                            .Where(b => b.flags != 0)
                            .OrderBy(b => b.request_date.Value.LocalDateTime.Hour)
                            .GroupBy(b => b.ip.country_name)
                            .Select(b => new {
                                b.Key,
                                Total = b.Count(),
                                Data = b.GroupBy(c => c.request_date.Value.LocalDateTime.Hour)
                                        .Select(c => new { c.Key, Count = c.Count() })
                                        .ToList()
                            })
                            .ToList(),
                    dangerous_ip = a.requests
                            .GroupBy(b => b.ip)
                            .OrderByDescending(b => b.Count(c => c.flags != 0))
                            .Select(b => new {
                                rival = 
                                    a.requests
                                     .Where(c => c.ip.country_code != b.Key.country_code)
                                     .GroupBy(c => c.ip)
                                     .OrderByDescending(c => c.Count(d => d.flags != 0))
                                     .Select(c => new {
                                         bad_count = c.Count(d => d.flags != 0),
                                         ip = c.Key
                                     })
                                     .FirstOrDefault(),
                                next_with_city = b.Key.city_name != null ? null : a.requests
                                         .Where(c => c.ip.city_name != null)
                                         .GroupBy(c => c.ip)
                                         .OrderByDescending(c => c.Count(d => d.flags != 0))
                                         .Select(c => new {
                                             bad_count = c.Count(d => d.flags != 0),
                                             ip = c.Key
                                         })
                                         .FirstOrDefault(),
                                bad_count = b.Count(c => c.flags != 0),
                                by_vuln = b
                                    .Where(c => c.flags != 0)
                                    .GroupBy(c => c.flags)
                                    .Select(c => new { c.Key, Count = c.Count() })
                                    .ToList(),
                                ip = b.Key
                            })
                            .FirstOrDefault()
                })
                .SingleAsync();
            return stats;
        }
    }
}