﻿using NCron;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Warehouse.Utils.Backup
{
    public class Job : CronJob
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string appPath;
        private string zipFile;
        private string token;
        private string uploadLink;

        public override void Execute()
        {
            try
            {
                var task = ExecuteAsync();
                task.Wait();
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        public async Task ExecuteAsync()
        {
            PreparePaths();
            Dump();
            Zip();
            LoadToken();
            await GetUploadLink();
            await UploadFile();
            Cleanup();

            logger.Trace("ok");
        }

        private void PreparePaths()
        {
            var codebase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codebase, UriKind.Absolute);
            appPath = Path.GetDirectoryName(uri.LocalPath);
            if (appPath == null)
            {
                throw new NullReferenceException("appPath");
            }
            var customerNow = DateTime.UtcNow.AddHours(3);
            zipFile = $"skill_{customerNow:yyyyMMdd_HHmm}.zip";
        }

        private void Dump()
        {
            var info = new ProcessStartInfo
            {
                FileName = "mongodump",
                Arguments = "--db skill --out " + Path.Combine(appPath, "dump"),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
            };
            var process = new Process { StartInfo = info };
            process.Start();
            process.WaitForExit();

            var error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
        }

        private void Zip()
        {
            var dumpPath = Path.Combine(appPath, "dump");
            var zipFullPath = Path.Combine(appPath, zipFile);
            ZipFile.CreateFromDirectory(dumpPath, zipFullPath);
        }

        private void LoadToken()
        {
            var fileName = Path.Combine(appPath, "token.txt");
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("file not found", fileName) ;
            }
            using (var reader = new StreamReader(fileName))
            {
                token = reader.ReadLine();
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("token is empty");
            }
        }

        private async Task GetUploadLink()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", token);
                client.BaseAddress = new Uri("https://cloud-api.yandex.net:443");

                var path = string.Concat("skill-backup/", zipFile);
                var linkUriString = $"/v1/disk/resources/upload?path={WebUtility.UrlEncode(path)}";

                var resp = await client.GetAsync(linkUriString);
                var content = await resp.Content.ReadAsStringAsync();
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var json = JObject.Parse(content);
                    uploadLink = json["href"].ToString();
                }
                if (string.IsNullOrEmpty(uploadLink))
                {
                    throw new Exception("empty upload link");
                }
            }
        }

        private async Task UploadFile()
        {
            var zipFullPath = Path.Combine(appPath, zipFile);
            using (var client = new HttpClient())
            using (var stream = File.OpenRead(zipFullPath))
            using (var content = new StreamContent(stream))
            {
                var resp = await client.PutAsync(uploadLink, content);
                if (resp.StatusCode != HttpStatusCode.Created)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new Exception(errorContent);
                }
            }
        }

        private void Cleanup()
        {
            var dumpPath = Path.Combine(appPath, "dump");
            if (Directory.Exists(dumpPath))
            {
                Directory.Delete(dumpPath, true);
            }

            var zipFullPath = Path.Combine(appPath, zipFile);
            if (File.Exists(zipFullPath))
            {
                File.Delete(zipFullPath);
            }
        }
    }
}
