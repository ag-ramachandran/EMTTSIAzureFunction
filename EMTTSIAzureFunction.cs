// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json.Nodes;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;
using Microsoft.Azure.Functions.Worker.Extensions.Kusto;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Mvc;


namespace Microsoft.Azure.WebJobs.Extensions.Fabric.TSI
{
    public class GetAggregates
    {
        [Function("GetPIPointValuesTSI")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "GetPIPointValuesTSI")] HttpRequestData req,[FromBody] TSIQueryRequest tsiQueryRequest,
            [KustoInput(Database: "PI-TimeSeriesData-For-EMT",
            KqlCommand = "declare query_parameters (tags:string,startDate:datetime,endDate:datetime,timebucket:timespan);GetAggregates(tags,startDate,endDate,timebucket)",
            KqlParameters = "@tags={tags},@startDate={startDateTime},@endDate={endDateTime},@timebucket={binInterval}",Connection = "KustoConnectionString")] List<AggregateValues> aggregateValuesWrapper)
        {
            var tags = tsiQueryRequest.tags;
            var startDateTime = tsiQueryRequest.startDateTime;
            var endDateTime = tsiQueryRequest.endDateTime;
            var binInterval = tsiQueryRequest.binInterval;
            return new OkObjectResult(new AggregateValuesWrapper(data:aggregateValuesWrapper));
        }
    }
    public record TSIQueryRequest(string tags, DateTime startDateTime, DateTime endDateTime, string binInterval);
    public record GetPIPointValuesTSI(JsonArray data);

    public record AggregateValue(string timestamp, double value);

    public record AggregateValues(string tagName, List<AggregateValue> values);

    public record AggregateValuesWrapper(List<AggregateValues> data);


}

/*
*/
