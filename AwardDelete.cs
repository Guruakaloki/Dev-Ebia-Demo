
using System;
//using System.IdentityModel.Tokens.Jwt;
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

namespace DEVEBIAFUNCTIONS
{
    #region POST
    public class PostBodyAwardDelete
    {
        public string id { get; set; }
        public string userId { get; set; }
    }
    #endregion

    public static class AwardDelete
    {
        [FunctionName("AwardDelete")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "awarddelete")]
            HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "Awards",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
            TraceWriter log)
        {
            PostBodyAwardDelete data = await req.Content.ReadAsAsync<PostBodyAwardDelete>();
            //ClaimsPrincipal principal = await Security.ValidateTokenAsync(req.Headers.Authorization);
            //if (principal == null)
            //{
            //    return new UnauthorizedResult();
            //}
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



            log.Info($"Processed request for {data.id} in AwardDelete, input: {data.userId},{data.id}");
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "Awards_R2");

            IDocumentQuery<AwardsElement> query = client.CreateDocumentQuery<AwardsElement>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                .Where(d => d.awardId.ToUpper() == data.id.ToUpper())
                                                .Where(d => d.logonid.ToUpper() == data.userId.ToUpper())
                                                .AsDocumentQuery();

            bool isDeleted = false;
            while (query.HasMoreResults)
            {
                foreach (Document result in await query.ExecuteNextAsync())
                {
                    result.SetPropertyValue("isDelete", "true");
                    await client.ReplaceDocumentAsync(result.SelfLink, result);
                    isDeleted = true;
                    break;
                }
            }
            // Random comment
            if (isDeleted)
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


