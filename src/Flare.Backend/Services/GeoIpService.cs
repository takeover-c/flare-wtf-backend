using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flare.Backend.Models;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.EntityFrameworkCore;

namespace Flare.Backend.Services {
    public class GeoIpService {
        private readonly ApplicationDbContext db;
        private readonly WebServiceClient geoIpClient;
        
        // ensure a single query per database connection (not static) 
        private readonly SemaphoreSlim saveSlim = new SemaphoreSlim(1);

        // ensure maximum 16 calls at a time made to the maxip database. 
        private static readonly SemaphoreSlim maxConcurrent = new SemaphoreSlim(16);
        
        // cache running tasks to not send same IP request 
        private static readonly Dictionary<string, Task<ip_address>> runningTasks
            = new Dictionary<string, Task<ip_address>>();
        
        public GeoIpService(ApplicationDbContext db, WebServiceClient geoIpClient) {
            this.db = db;
            this.geoIpClient = geoIpClient;
        }

        public async Task<ip_address> Query(string ip) {
            Task<ip_address> task;
            bool inserted = false;

            lock (runningTasks) {
                if (!runningTasks.TryGetValue(ip, out task)) {
                    runningTasks[ip] = task = _Query(ip);
                    inserted = true;
                }
            }

            try {
                return await task;
            }
            finally {
                if(inserted){
                    lock (runningTasks) {
                        runningTasks.Remove(ip);
                    }
                }
            }
        }
        
        private async Task<ip_address> _Query(string ip) {
            CityResponse cityResponse;
            
            await saveSlim.WaitAsync();
            try {
                var cached_ip = await db.ip_addresses.FirstOrDefaultAsync(a => a.ip == ip);
                if (cached_ip != null)
                    return cached_ip;
            }
            finally {
                saveSlim.Release();
            }

            await maxConcurrent.WaitAsync();
            try {
                cityResponse = await geoIpClient.CityAsync(ip);
            }
            finally {
                maxConcurrent.Release();
            }
            
            await saveSlim.WaitAsync();
            try {
                var ip_address = new ip_address() {
                    ip = cityResponse.Traits.IPAddress,
                    city_name = cityResponse.City?.Name,
                    connection_type = cityResponse.Traits?.ConnectionType,
                    isp = cityResponse.Traits?.Isp,
                    country_code = cityResponse.Country.IsoCode,
                    country_name = cityResponse.Country.Name,
                    organisation = cityResponse.Traits.Organization,
                    latitude = cityResponse.Location.Latitude,
                    longitude = cityResponse.Location.Longitude
                };
                
                db.ip_addresses.Add(ip_address);
                
                await db.SaveChangesAsync();

                return ip_address;
            }
            finally {
                saveSlim.Release();
            }
        }
    }
}
