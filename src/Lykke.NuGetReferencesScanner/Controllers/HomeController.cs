﻿using System.Linq;
using Lykke.NuGetReferencesScanner.Domain;
using Lykke.NuGetReferencesScanner.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lykke.NuGetReferencesScanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReferencesScanner _scanner;

        public HomeController(IReferencesScanner scanner)
        {
            _scanner = scanner;
        }

        public IActionResult Index()
        {
            var result = _scanner.GetScanResult();
            var model = new RefsScanStatisticsModel { Statistics = result.Statistics, Status = result.Status };
            return View(model);
        }

        public IActionResult GetData()
        {
            var scanResult = _scanner.GetScanResult();
            var data = scanResult.Data
                .OrderBy(d => d.Item1.Name)
                .ThenBy(d => d.Item1.Version)
                .ThenBy(d => d.Item2.Name)
                .Select(sr => new RowItemModel
                {
                    NugetPacketName = sr.Item1.Name,
                    NugetVersion = sr.Item1.Version.ToString(),
                    RepoName = sr.Item2.Name,
                    RepoUrl = sr.Item2.Url.ToString()
                }).ToArray();

            var result = JsonConvert.SerializeObject(new
            {
                draw = data.Length,
                recordsTotal = data.Length,
                recordsFiltered = data.Length,
                data,
                error = ""
            });
            return Ok(result);
        }
    }
}
