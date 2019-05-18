using System.Threading.Tasks;
using Flare.Backend.Models;
using Flare.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Flare.Backend.Controllers {
    public class IpAddressController : ControllerBase {
        private readonly GeoIpService geoIpService;

        public IpAddressController(GeoIpService geoIpService) {
            this.geoIpService = geoIpService;
        }

        [HttpGet("ip/{ip}")]
        public async Task<ip_address> ip(string ip) {
            return await geoIpService.Query(ip);
        }
    }
}