
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
//using System.IdentityModel.Tokens.Jwt;
namespace DEVEBIAFUNCTIONS
{

    #region POST
    public class PostBodyNotificationLanding
    {
        public string id { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
    }
    #endregion

    #region POCO INPUT
    public class NotificationLandingResponse
    {
        public NotificationDetails notificationDetails { get; set; }
        public NotificationLandingResponse(NotificationDetails notificationDetails)
        {
            this.notificationDetails = notificationDetails;
        }
    }

    public class NotificationDetails
    {
        public string totalCount { get; set; }
        public List<NotificationElement> notificationList { get; set; }
        public NotificationDetails(string totalCount, List<NotificationElement> notificationList)
        {
            this.totalCount = totalCount;
            this.notificationList = notificationList;
        }
    }

    public class NotificationElement
    {
        public string type { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string dateAndTime { get; set; }
        public string isRead { get; set; }
        public string id { get; set; }
        public string notificationid { get; set; }
        public string logonid { get; set; }
        public string isDeleted { get; set; }
        public string isBellReset { get; set; }
    }

    #endregion
    public static class NotificationLanding
    {
        [FunctionName("NotificationLanding")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notificationlanding")]
            HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "NotificationLanding",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
            TraceWriter log)
        {

            PostBodyNotificationLanding data = await req.Content.ReadAsAsync<PostBodyNotificationLanding>();

            ///////2nd level Auth code
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
            //if (dId.ToLower() != data.id.ToLower())
            //{
            //    return new UnauthorizedResult();
            //}
            log.Info($"Processed request for {data.id} in NotificationLanding, settingslimit: {data.limit},{data.limit}");

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "NotificationLanding");

            IDocumentQuery<NotificationElement> query = client.CreateDocumentQuery<NotificationElement>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                              .Where(d => d.logonid.ToUpper() == data.id.ToUpper())
                                                              .Where(d => d.isDeleted.ToLower() == "false")
                                                              .AsDocumentQuery();


            //list result
            List<NotificationElement> notificationElementList = new List<NotificationElement>();
            while (query.HasMoreResults)
            {
                foreach (NotificationElement result in await query.ExecuteNextAsync())
                {
                    notificationElementList.Add(result);
                }
            }

            if (notificationElementList.Count == 0) return new NoContentResult();

            else
            {
                //filter for one year
                notificationElementList = notificationElementList.Where(x => (DateTime.Now - DateTime.ParseExact(x.dateAndTime, "M/d/yyyy h:mm:ss tt", new CultureInfo("en-US"))).TotalDays < 365).ToList();
                NotificationDetails notificationDetails = new NotificationDetails(notificationElementList.Count.ToString(), notificationElementList.Skip(((data.offset == 0 ? 1 : data.offset) - 1) * data.limit).Take(data.limit).ToList());

                //logic to copy notification id to id column
                foreach (NotificationElement item in notificationDetails.notificationList)
                {
                    item.id = item.notificationid;
                }
                NotificationLandingResponse notificationLandingResponse = new NotificationLandingResponse(notificationDetails);

                return new OkObjectResult(notificationLandingResponse);
            }
        }
    }

}
