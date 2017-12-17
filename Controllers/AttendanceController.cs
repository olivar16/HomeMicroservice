using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeMicroservice.Models;
using Microsoft.Extensions.Configuration;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace HomeMicroservice.Controllers
{
    public class AttendanceController : Controller
    {
        const string TABLE_NAME = "House";
        AmazonDynamoDBClient _dynamoClient;
        IConfiguration _config;
        public AttendanceController(IConfiguration configuration, AmazonDynamoDBClient dynamoClient){
            _config = configuration;
            _dynamoClient = dynamoClient;
        }
        
        [HttpGet]
        public IActionResult Index(){
            return Ok("Reached Attendance endpoint");
        }

        [HttpGet]
        public IActionResult Person([FromQuery]string Name)
        {
            GetItemRequest req = new GetItemRequest()
            {
                TableName = TABLE_NAME,
                Key = new Dictionary<string, AttributeValue>() { { "Name", new AttributeValue { S = Name } } },
            };
            var response = _dynamoClient.GetItemAsync(req);
            Dictionary<string, AttributeValue> responseItem = response.Result.Item;
            var person = new Person
            {
                Name = responseItem["Name"].S,
                IsHome = responseItem["IsHome"].BOOL,
                Timestamp = responseItem["Timestamp"].S
        };

            return Ok(person);
        }

        [HttpGet]
        public IActionResult People()
        {
            ArrayList responseList = new ArrayList();
            ScanRequest req = new ScanRequest
            {
                Limit=10,
                TableName=TABLE_NAME
            };
            var response = _dynamoClient.ScanAsync(req);
            response.Result.Items.ForEach( x => {
                responseList.Add(new Person
                {
                    Name = x["Name"].S,
                    IsHome = x["IsHome"].BOOL,
                    Timestamp = x["Timestamp"].S
                });

            });

            return Ok(responseList);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] Person p){
            //Define key
            Dictionary<string, AttributeValue> key = new Dictionary<string, AttributeValue>
            {
                {"Name", new AttributeValue{S=p.Name}}
            };

            // Define attribute updates
            Dictionary < string, AttributeValueUpdate > updates = new Dictionary<string, AttributeValueUpdate> ();
            updates["IsHome"] = new AttributeValueUpdate
            {
                Action = AttributeAction.PUT,
                Value = new AttributeValue{
                    BOOL= p.IsHome
                }
            };
            updates["Timestamp"] = new AttributeValueUpdate
            {
                Action = AttributeAction.PUT,
                Value = new AttributeValue
                {
                    S = DateTime.Now.ToString()
                }
            };

            UpdateItemRequest req = new UpdateItemRequest()
            {
                TableName=TABLE_NAME,
                AttributeUpdates = updates,
                Key=key
            };
            //await response from the dynamodb client
            UpdateItemResponse resp = await _dynamoClient.UpdateItemAsync(req);
            if(resp.HttpStatusCode == System.Net.HttpStatusCode.OK){
                return Ok();
            }
            else{
                return NotFound();
            }
        }

    }
}
