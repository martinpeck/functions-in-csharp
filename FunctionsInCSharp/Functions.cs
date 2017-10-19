using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

namespace FunctionsInCSharp
{
    /// <summary>
    /// Class providing the implementation to a couple of Azure Functions
    /// </summary>
    public class Functions
    {
        /// <summary>
        /// Azure Function that uses attributes to define its behaviour
        /// </summary>
        [FunctionName("Add")]
        public static int Add([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
                               TraceWriter log)
        {
            int x = int.Parse(req.GetQueryNameValuePairs()
                              .FirstOrDefault(q => string.Compare(q.Key, "x", true) == 0)
                              .Value);

            int y = int.Parse(req.GetQueryNameValuePairs()
                               .FirstOrDefault(q => string.Compare(q.Key, "y", true) == 0)
                               .Value);

            return x + y;
        }
        /// <summary>
        /// Azure Function that uses function.json to define its behaviour
        /// </summary>
        public static int Add2(HttpRequestMessage req,
                               int x,
                               int y,
                               TraceWriter log) => x + y;

        [FunctionName("Process")]
        [return: Table("Results")]
        public static TableRow Process([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Process/{x:int}/{y:int}")] HttpRequestMessage req,
                                       int x,
                                       int y,
                                       [Table("Results","sums","{x}_{y}")] TableRow tableRow,
                                       TraceWriter log)
        {

            if (tableRow != null)
            {
                log.Info($"{x} + {y} already exists");
                return null;
            }

            log.Info($"Processing {x} + {y}");

            return new TableRow()
            {
                PartitionKey = "sums",
                RowKey = $"{x}_{y}",
                X = x,
                Y = y,
                Sum = x + y
            };
        }

        [FunctionName("List")]
        public static HttpResponseMessage List([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
                                               [Table("Results","sums")] IQueryable<TableRow> table,
                                               TraceWriter log)
        {
            log.Info("doing stuff!");
            return req.CreateResponse(HttpStatusCode.OK, table, "application/json");
        }

    }

    public class TableRow: TableEntity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Sum { get; set; }
    }
}
