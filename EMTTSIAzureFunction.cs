using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.Azure.WebJobs.Kusto;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Fabric.TSI
{
    /**
        * This function is used to get the aggregated values of the PI Points from the TSI database.
        * The function takes in a list of tags, startDateTime, endDateTime and binInterval as input and returns the aggregated values of the PI Points.
        * The function uses the Kusto binding to query the Kusto database and get the aggregated values of the PI Points. This uses the declarative function call method to invoke the Kusto query
    */
    public class GetAggregates
    {
        [FunctionName("GetPIPointValuesTSI")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "GetPIPointValuesTSI")]
                HttpRequest req,ILogger logger, IBinder binder)
        {
            var content = new StreamReader(req.Body).ReadToEnd();
            TSIQueryRequest tsiQueryRequest = JsonConvert.DeserializeObject<TSIQueryRequest>(content);
            //string tags = JsonConvert.SerializeObject(tsiQueryRequest.tags);
            string tags = String.Join("|", tsiQueryRequest.tags);
            var startDateTime = tsiQueryRequest.startDateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
            var endDateTime = tsiQueryRequest.endDateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
            var binInterval = tsiQueryRequest.binInterval;

            /*
                Uses the Fabric KQL binding to query the Kusto database and get the aggregated values of the PI Points.
            */
            var kustoAttribute = new KustoAttribute("PI-TimeSeriesData-For-EMT") // Database name
            {
                KqlCommand = "declare query_parameters (tags:string,startDate:datetime,endDate:datetime,timebucket:timespan);GetTagAggregates(tags,startDate,endDate,timebucket)",
                KqlParameters = $"@tags={tags},@startDate={startDateTime},@endDate={endDateTime},@timebucket={binInterval}",
                Connection = "KustoConnectionString"
            };
            var exportedRecords = (await binder.BindAsync<IEnumerable<AggregateValues>>(kustoAttribute)).ToList();
            return new OkObjectResult(new AggregateValuesWrapper(data: exportedRecords));
        }
    }
    public record TSIQueryRequest(List<string> tags, DateTime startDateTime, DateTime endDateTime, string binInterval);
    public record GetPIPointValuesTSI(JsonArray data);
    public record AggregateValue(string timestamp, double value);
    public record AggregateValues(string tagName, List<AggregateValue> values);
    public record AggregateValuesWrapper(List<AggregateValues> data);


}

/*
 ,
            [Kusto(Database:"PI-TimeSeriesData-For-EMT" ,
                KqlCommand = "declare query_parameters (tags:string,startDate:datetime,endDate:datetime,timebucket:timespan);GetAggregates(tags,startDate,endDate,timebucket)",
                KqlParameters = "@tags={tags},@startDate={startDateTime},@endDate={endDateTime},@timebucket={binInterval}",Connection = "KustoConnectionString")] List<AggregateValues> aggregateValuesWrapper
 */