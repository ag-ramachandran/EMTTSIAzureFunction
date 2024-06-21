
# EMTTSIAzureFunction

### Create a table
```
.set-or-append  GoM  <|
let toUnixTime = (dt:datetime) 
{ 
    (dt - datetime(1970-01-01)) / 1s 
};
let YesterdayStart = startofday(ago(1d)); //Start of month
let StepBy = 1m; //Supported timespans
let nn = 64000; // Row Count parametrized
let MyTimeline = range MyMonthHour from YesterdayStart to now() step StepBy;
let numericValue=todouble(rand());
let isQuestionable=0;
MyTimeline | extend id="NAK-DHTT-30550_PV",timestamp=MyMonthHour,series=bag_pack("numericValue",numericValue,"isQuestionable",toint(0)),pointId=0, edgeTimestamp= MyMonthHour| order by MyMonthHour asc | project-away MyMonthHour

```

### Create the function
```
.create-or-alter function with (docstring = "Get timeseries aggregates") GetAggregates(tags:string,startDate:datetime,endDate:datetime,timebucket:timespan) {
GoM
| where id in (parse_json(tags)) and timestamp between (startDate..endDate)
| extend Value=todouble(series.numericValue)
| summarize Average = avg(Value), Count = count(), Min = min(Value), Max = max(Value) by id, bin(timestamp, timebucket)
| extend d = bag_pack("timestamp", timestamp, "value", Average)
| summarize values = make_list(d) by id
| project-rename tagName=id
}
```

For InProcess Function runs

```
{
	"IsEncrypted": false,
	"Values": {
		"FUNCTIONS_WORKER_RUNTIME": "dotnet",
		"AzureWebJobsStorage": "UseDevelopmentStorage=true",
		"KustoConnectionString": "https://<>.z4.kusto.fabric.microsoft.com; Fed=true; Accept=true;UserToken=<Get a user access token for test>",
		"AzureWebJobs.HttpExample.Disabled": "true"
	},
	"Host": {
		"LocalHttpPort": 7104,
		"CORS": "*",
		"CORSCredentials": false
	}
}
```
### Get the access token as follows
```
 az account get-access-token --resource=https://<>.z4.kusto.fabric.microsoft.com --query accessToken --output tsv
 ```
