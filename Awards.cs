

using System;
using System.Collections.Generic;
using System.Globalization;
//using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DEVEBIAFUNCTIONS
{
    #region POST
    public class PostBodyAwards
    {
        public string id { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
    }
    #endregion

    #region POCO INPUT
    public class AwardsResponse
    {
        public AwardsContainer awards { get; set; }
        public AwardsResponse(AwardsContainer awards)
        {
            this.awards = awards;
        }
    }

    public class AwardsContainer
    {
        public string totalCount { get; set; }
        public List<AwardsElement> awardsList { get; set; }
        public AwardsContainer(string totalCount, List<AwardsElement> awards)
        {
            this.totalCount = totalCount;
            this.awardsList = awards;
        }
    }

    public class AwardsElement
    {
        public string type { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string date { get; set; }
        public string awardWon { get; set; }
        public string id { get; set; }
        public string awardId { get; set; }
        public string logonid { get; set; }
        public string dateAndTime { get; set; }
    }
    #endregion

    public static class Awards
    {
        [FunctionName("Awards")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "awards")]
            HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "Awards",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
            TraceWriter log)
        {
            PostBodyAwards data = await req.Content.ReadAsAsync<PostBodyAwards>();

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

            log.Info($"Processed request for {data.id} in Awards, settingslimit: {data.limit},{data.limit}");

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "Awards");

            IDocumentQuery<AwardsElement> query = client.CreateDocumentQuery<AwardsElement>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                          .Where(d => d.logonid.ToUpper() == data.id.ToUpper())
                                                          .AsDocumentQuery();


            //list result
            List<AwardsElement> awardsElementList = new List<AwardsElement>();
            while (query.HasMoreResults)
            {
                foreach (AwardsElement result in await query.ExecuteNextAsync())
                {
                    awardsElementList.Add(result);
                }
            }

            if (awardsElementList.Count == 0) return new NoContentResult();

            else
            {
                //filter for one year
                awardsElementList = awardsElementList.Where(x => (DateTime.Now - DateTime.ParseExact(x.dateAndTime, "M/d/yyyy h:mm:ss tt", new CultureInfo("en-US"))).TotalDays < 365).ToList();
                // AwardsContainer awardsContainer = new AwardsContainer(awardsElementList.Count.ToString(), awardsElementList.Skip(((data.offset == 0 ? 1 : data.offset) - 1) * data.limit).Take(data.limit).ToList());
                AwardsContainer awardsContainer = new AwardsContainer("3500", awardsElementList.Skip(((data.offset == 0 ? 1 : data.offset) - 1) * data.limit).Take(data.limit).ToList());

                ////logic to copy award id to id
                foreach (AwardsElement item in awardsContainer.awardsList)
                {
                    item.id = item.awardId;
                }
                AwardsResponse awardsResponse = new AwardsResponse(awardsContainer);

                return new OkObjectResult(awardsResponse);
            }
        }
    }
}
