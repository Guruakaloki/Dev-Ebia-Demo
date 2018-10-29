
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
//using System.IdentityModel.Tokens.Jwt;


namespace DEVEBIAFUNCTIONS
{
    #region POCO OUTPUT
    public class NotificationSettingResponse
    {
        public List<NotificationSettingElement> notificationSettingList { get; set; }
        public NotificationSettingResponse(List<NotificationSettingElement> notificationSettingList)
        {
            this.notificationSettingList = notificationSettingList;
        }
    }

    public class NotificationSettingElement
    {
        public string type { get; set; }
        public string description { get; set; }
        public string isEnabled { get; set; }
        public string id { get; set; }
        public string logonid { get; set; }
    }
    #endregion


    public static class NotificationSetting
    {

        [FunctionName("NotificationSetting")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get","post", Route = "notificationsetting/{id}")]
                HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "NotificationSetting_NonPartition",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
            string id,
            TraceWriter log)
        {

            //var jwtHandler = new JwtSecurityTokenHandler();
            //var jwtInput = req.Headers.Authorization.ToString();

            //var jwt = "";

            //if (jwtInput.Contains("Bearer"))
            //    jwt = jwtInput.Substring(7);
            //else
            //    jwt = jwtInput;



            //var readableToken = jwtHandler.CanReadToken(jwt);


            //var dId = "";
            //if (readableToken != true)
            //{
            //    return new NotFoundResult();
            //}
            //if (readableToken == true)
            //{
            //    var token = jwtHandler.ReadJwtToken(jwt);


            //    var claims = token.Claims;


            //    var claim = claims.Where(c => c.Type == "upn").FirstOrDefault();
            //    dId = claim.Value.Substring(0, claim.Value.IndexOf('@'));




            //}
            //if(dId.ToLower()!=id.ToLower())
            //{
            //    return new UnauthorizedResult();
            //}



            log.Info($"Processed request for {id} in NotificationSetting");

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "NotificationSetting_NonPartition");

            IDocumentQuery<NotificationSettingElement> query = client.CreateDocumentQuery<NotificationSettingElement>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                          .Where(d => d.logonid.ToUpper() == id.ToUpper())
                                                          .AsDocumentQuery();


            //var results = await query.ExecuteNextAsync();
            //list result
            //List<NotificationSettingElement> notificationSettingList = new List<NotificationSettingElement>();

            //foreach (NotificationSettingElement result in results)
            //    {
            //        notificationSettingList.Add(result);
            //    }
            var results = await query.ExecuteNextAsync<NotificationSettingElement>();


            //list result
            List<NotificationSettingElement> notificationSettingList = results.ToList();

            if (notificationSettingList.Count == 0) return new NoContentResult();

            else
            {
                NotificationSettingResponse notificationSettingResponse = new NotificationSettingResponse(notificationSettingList);

                return new OkObjectResult(notificationSettingResponse);
            }
        }
    }
}

