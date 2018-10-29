
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
    public class PostBodyNotificationSettingUpdate
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string isEnabled { get; set; }
    }
    #endregion

    public static class NotificationSettingUpdate
    {
        [FunctionName("NotificationSettingUpdate")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notificationsettingupdate")]
            HttpRequestMessage req,
             [CosmosDB(
                databaseName: "contesthub",
                collectionName: "NotificationSetting",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
             TraceWriter log)
        {

            PostBodyNotificationSettingUpdate data = await req.Content.ReadAsAsync<PostBodyNotificationSettingUpdate>();

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
            //if(dId.ToLower()!=data.userId.ToLower())
            //{
            //    return new UnauthorizedResult();
            //}

            log.Info($"Processed request for {data.id} in NotificationSettingUpdate, input: {data.userId},{data.id},{data.isEnabled}");
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "NotificationSetting");

            IDocumentQuery<NotificationSettingElement> query = client.CreateDocumentQuery<NotificationSettingElement>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                                    .Where(d => d.id.ToUpper() == data.id.ToUpper())
                                                                 .Where(d => d.logonid.ToUpper() == data.userId.ToUpper())
                                                              .AsDocumentQuery();
            bool isUpdated = false;
            var results = await query.ExecuteNextAsync();



            foreach (Document result in results)
            {
                result.SetPropertyValue("isEnabled", data.isEnabled);
                await client.ReplaceDocumentAsync(result.SelfLink, result);
                isUpdated = true;
                break;
            }

            if (isUpdated)
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
