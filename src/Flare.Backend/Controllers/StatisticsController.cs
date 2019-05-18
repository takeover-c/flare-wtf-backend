using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flare.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flare.Backend.Controllers {
    public class Statistics {
        public StatisticsGeneral general { get; set; }

        public StatisticsDataMining data_mining { get; set; }
        
        public StatisticsVisualisation visualisation { get; set; }
    }

    public class StatisticsGeneral {
        public int total_requests { get; set; }
        
        public int error_responses { get; set; }
        
        public int distinct_ips { get; set; }
    }
    
    public class StatisticsDataMining {
        public int flagged_requests { get; set; }
        
        public Dictionary<int, int> requests_by_vuln { get; set; }
        
        public List<(string, int)> top_pages { get; set; }
        
        public List<(string, int)> by_country { get; set; }
        
        public List<(string, int)> by_time { get; set; }
    }

    public class StatisticsVisualisation {
        public List<(DateTimeOffset, int)> requests_hr { get; set; }
        
        public List<(DateTimeOffset, int)> attacks_hr { get; set; }
    }
    
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
                    total_requests = a.requests.Count,
                    error_responses = a.requests
                            .Count(b => b.response_code != null && b.response_code >= 500 && b.response_code <= 599),
                    distinct_ips = a.requests
                            .Select(b => b.ip_id)
                            .Distinct()
                            .Count(),
                    flagged_requests = a.requests
                            .Count(b => b.flags != 0),
                    by_vuln = a.requests
                            .GroupBy(b => b.flags)
                            .Select(b => new { b.Key, Count = b.Count() })
                            .ToList(),
                    top_pages = a.requests
                            .GroupBy(b => b.request_path)
                            .OrderByDescending(b => b.Count())
                            .Take(5)
                            .Select(b => new { b.Key, Count = b.Count() })
                            .ToList(),
                    top_countries = a.requests
                             .GroupBy(b => b.ip.country_name)
                             .OrderByDescending(b => b.Count())
                             .Take(5)
                             .Select(b => new { b.Key, Count = b.Count() })
                             .ToList(),
                    by_hourly = a.requests
                             .GroupBy(b => b.request_date.Value.Hour)
                             .OrderByDescending(b => b.Count())
                             .Take(5)
                             .Select(b => new { b.Key, Count = b.Count() })
                             .ToList(),
                    by_countries = a.requests
                             .GroupBy(b => b.ip.country_name)
                             .OrderByDescending(b => b.Count())
                             .Select(b => new { b.Key, Count = b.Count(), Good = b.Count(c => c.flags == 0), Bad = b.Count(c => c.flags != 0) })
                             .ToList(),
                    by_country_hourly = a.requests
                             .GroupBy(b => b.ip.country_name)
                             .Select(b => new {
                                 b.Key,
                                 Data = b.GroupBy(c => c.request_date.Value.Hour)
                                     .Select(c => new { c.Key, Count = c.Count() })
                                     .ToList()
                             })
                             .ToList()
                })
                .SingleAsync();
            return stats;
        }
    }
}