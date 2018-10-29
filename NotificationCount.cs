using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
//using System.IdentityModel.Tokens.Jwt;
namespace DEVEBIAFUNCTIONS
{
    #region POCO OUTPUT
    public class UnreadNotifications
    {
        public string count { get; set; }
        public UnreadNotifications(string count)
        {
            this.count = count;
        }
    }
    public class UnreadNotificationsResponse
    {
        public UnreadNotifications unreadNotifications { get; set; }
        public UnreadNotificationsResponse(UnreadNotifications unreadNotifications)
        {
            this.unreadNotifications = unreadNotifications;
        }
    }
    #endregion

    public static class NotificationCount
    {
        [FunctionName("NotificationCount")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notificationcount/{id}")]
                HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "NotificationLanding",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
            string id,
            TraceWriter log)
        {

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
            //if (dId.ToLower() != id.ToLower())
            //{
            //    return new UnauthorizedResult();
            //}
            log.Info($"Processed request for {id} in NotificationCount");

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "NotificationLanding");

            var count = client.CreateDocumentQuery<NotificationElement>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                              .Where(d => d.logonid.ToUpper() == id.ToUpper())
                                                              .Where(d => d.isDeleted.ToLower() == "false")
                                                              .Where(d => d.isBellReset.ToLower() == "false")
                                                              .Count();


            //return expected result
            UnreadNotifications unreadNotifications = new UnreadNotifications(count.ToString());
            return new OkObjectResult(new UnreadNotificationsResponse(unreadNotifications));
        }
    }
}


