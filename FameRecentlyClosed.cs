
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
    public class FameResponseRC
    {
        public List<FameElementRC> closedFameDetails { get; set; }
        public FameResponseRC(List<FameElementRC> fameElementList)
        {
            this.closedFameDetails = fameElementList;
        }
    }
    public class FameElementRC
    {
        public string id { get; set; }
        public string logonid { get; set; }
        public string title { get; set; }
        public string weeksLeft { get; set; }
        public string apProgress { get; set; }
        public string isActive { get; set; }
        public string apActual { get; set; }
        public string apGoal { get; set; }
        public string apRemaining { get; set; }
        public string producingRecruitsActual { get; set; }
        public string ProducingRecruitsGoal { get; set; }
        public string eapActual { get; set; }
        public string eapGoal { get; set; }
        public string newAccountsActual { get; set; }
        public string newAccountsGoal { get; set; }
        public string fastStartActual { get; set; }
        public string fastStartGoal { get; set; }
        public string fireballStarSeriesActual { get; set; }
        public string fireballStarSeriesGoal { get; set; }
        public string noPay { get; set; }
        public string weekAndYear { get; set; }
        public string fameNumber { get; set; }
        public string fameBonus { get; set; }
        public string apachievedStatus { get; set; }
        public string eapachievedStatus { get; set; }
        public string newAccountsachievedStatus { get; set; }
        public string producingRecruitsachievedStatus { get; set; }
        public string fastStartachievedStatus { get; set; }
        public string fireballStarSeriesachievedStatus { get; set; }
        public string isBadgeActive { get; set; }
        public string eapProgress { get; set; }
        public string producingRecruits { get; set; }
        public string newAccountsProgress { get; set; }
        public string fastStartProgress { get; set; }
        public string fireballStarSeriesProgress { get; set; }
        public string isBonusWon { get; set; }
        public string endDate { get; set; }
    }
    public static class FameRecentlyClosed
    {

        [FunctionName("FameRecentlyClosed")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "famerecentlyclosed/{id}")]
                HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "Fame",
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
            log.Info($"Processed request for {id} in Fame");

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "Fame");

            Uri c2 = UriFactory.CreateDocumentCollectionUri("contesthub", "ProductionCalendar");

            IDocumentQuery<ProductionCalendar> dateQuery = client.CreateDocumentQuery<ProductionCalendar>(c2, new FeedOptions { EnableCrossPartitionQuery = true }).AsDocumentQuery();

            var dates = await dateQuery.ExecuteNextAsync<ProductionCalendar>();

            var year = Convert.ToInt32(dates.FirstOrDefault().closedProdYear) - 1;

            var query = client.CreateDocumentQuery<FameElementRC>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = Int32.MaxValue }).Where(d => d.logonid.ToUpper() == id.ToUpper());




            List<FameElementRC> fameElementList = query.ToList();

            fameElementList.ForEach(x => x.endDate = "Ended " + x.endDate);

            //var fameElements = fameElementList;

            fameElementList = fameElementList.Where(e => Convert.ToInt32(e.title.Substring(e.title.Length - 4, 4)) == year).ToList();

            if (fameElementList.Count == 0) return new NoContentResult();

            else
            {
                FameResponseRC fameResponse = new FameResponseRC(fameElementList);
                return new OkObjectResult(fameResponse);
            }

        }

    }

}
