﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Dapper;

namespace JaegerNetCoreSecond.App_Data
{
    public class BService : IDisposable
    {
        private const string NextNodeName = "test-name-node-Timur";
        private const string NextServiceName = "Third Service";
        private readonly WebClient _webClient = new WebClient();
        private const string GetValuesQuery = @"SELECT name FROM tableTest where name = 'test2' ";

        public async Task<string[]> GetValues()
        {
            var url = ConsulSettings.Url ?? GetUrl();
            var connectionString = ConsulSettings.ConnectionString;
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var command = new CommandDefinition(GetValuesQuery);
                var dbRes = (await db.QueryAsync<string>(command)).ToList();
                var res = _webClient.DownloadString(url);
                dbRes.Add(res);
                return dbRes.ToArray();
            }
        }

        public string GetUrl()
        {
            using (var consulClient = new ConsulClient())
            {
                consulClient.Config.Address = ConsulSettings.ClientUrl;
                var services = consulClient.Catalog.Service(NextServiceName).GetAwaiter().GetResult().Response;
                var currentService = services.First(service => service.Node.Equals(NextNodeName));
                var address = currentService.ServiceAddress;
                var port = currentService.ServicePort;
                return ConsulSettings.Url = $"{address}:{port}/api/GetValues";
            }
        }

        public void Dispose()
        {
            _webClient?.Dispose();
        }
    }
}