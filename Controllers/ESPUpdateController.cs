using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace ESPUpdater.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ESPUpdateController : ControllerBase
    {

        private const string LATEST_BIN = "Data/latest.bin";
        // GET api/ESPUpdate
        [HttpGet]
        public IActionResult Get()
        {
            //check header to ensure it's from an ESP updater
            if (Request.Headers.TryGetValue("USER-AGENT", out StringValues vals) &&
                vals.Any(s => string.Equals(s, "ESP8266-http-Update", StringComparison.OrdinalIgnoreCase)))
            {
                string currentVersion = string.Empty;
                if (Request.Headers.TryGetValue("x-ESP8266-VERSION", out StringValues vVals))
                {
                    currentVersion = vVals.FirstOrDefault();
                }

                string currentMD5 = string.Empty;
                if (Request.Headers.TryGetValue("x-ESP8266-SKETCH-MD5", out StringValues md5Vals))
                {
                    currentMD5 = md5Vals.FirstOrDefault();
                }

                //todo: compare versions
                string updateMD5 = string.Empty;
                if (System.IO.File.Exists(LATEST_BIN))
                {
                    updateMD5 = CalculateMD5(LATEST_BIN);
                    if (!string.Equals(currentMD5, updateMD5, StringComparison.OrdinalIgnoreCase))
                    {
                        //we have different hashes, therefore update
                        FileStream stream = System.IO.File.OpenRead(LATEST_BIN);
                        if (stream != null)
                        {
                            return File(stream, "application/octet-stream");
                        }
                    }
                }
            }

            return StatusCode(304); //no update required
        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
