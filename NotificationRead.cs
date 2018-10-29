
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
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
    #region POST
    public class PostBodyNotificationRead
    {
        public string id { get; set; }
        public string userId { get; set; }
    }
    #endregion

    public static class NotificationRead
    {
        [FunctionName("NotificationRead")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notificationread")]
            HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "NotificationLanding",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
            TraceWriter log)
        {

            PostBodyNotificationRead data = await req.Content.ReadAsAsync<PostBodyNotificationRead>();

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
            //if (dId.ToLower() != data.userId.ToLower())
            //{
            //    return new UnauthorizedResult();
            //}

            log.Info($"Processed request for {data.id} in NotificationLanding, input: {data.userId},{data.id}");
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "NotificationLanding");

            IDocumentQuery<NotificationElement> query = client.CreateDocumentQuery<NotificationElement>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                              .Where(d => d.notificationid.ToUpper() == data.id.ToUpper())
                                                              .Where(d => d.logonid.ToUpper() == data.userId.ToUpper())
                                                              .AsDocumentQuery();
            bool isRead = false;
            while (query.HasMoreResults)
            {
                foreach (Document result in await query.ExecuteNextAsync())
                {
                    result.SetPropertyValue("isRead", "true");
                    await client.ReplaceDocumentAsync(result.SelfLink, result);
                    isRead = true;
                    break;
                }
            }

            if (isRead)
            {
                return new OkResult();
            }
            else
            {
                return new NoContentResult();
            }
        }
    }
}
