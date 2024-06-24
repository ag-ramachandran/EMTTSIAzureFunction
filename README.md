
# Developer documentation for running the setup

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

### Create the functions
```
.create-or-alter function with (docstring = "Get timeseries aggregates") GetTagAggregates(tags:string,startDate:datetime,endDate:datetime,timebucket:timespan) {
let tagsArray=split(tags,"|");
GoM
| where id in (tagsArray) and timestamp between (startDate..endDate)
| extend Value=todouble(series.numericValue)
| summarize Average = avg(Value), Count = count(), Min = min(Value), Max = max(Value) by id, bin(timestamp, timebucket)
| extend d = bag_pack("timestamp", timestamp, "value", Average)
| summarize values = make_list(d) by id
| project-rename tagName=id
}


.create-or-alter function with (docstring = "Get timeseries aggregates") GetAggregatesUni(tags:string,startDate:datetime,endDate:datetime) {
let tagsArray=split(tags,"|");
GoM
| where id in (tagsArray) and timestamp between (startDate..endDate)
| extend Value=todouble(series.numericValue)
| summarize value = avg(Value) by id
| project-rename tagName=id
}
```

### For InProcess Function runs

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

Once you get this token, you can set the following environment variable in your local environment. In a prod environment you can set this as an app setting. The format of the connection string is documented here : https://learn.microsoft.com/en-us/azure/data-explorer/kusto/api/connection-strings/kusto. Refer : Microsoft Entra ID Federated application authentication using ApplicationClientId and ApplicationKey set up which is the auth supported in Fabric today

```
export KustoConnectionString="Data Source=https://<>.fabric.microsoft.com;Fed=True;UserToken=<Token>"

```

For local runs you will need to have the function tools and then run the following command

```
func start --port 7104 --verbose
```	