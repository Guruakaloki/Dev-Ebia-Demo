using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.Documents.Client;
using System.Security.Claims;
using System.Net.Http;
//using System.IdentityModel.Tokens.Jwt;
namespace DEVEBIAFUNCTIONS
{
    public class TripleCrownContestDetails
    {
        public string title { get; set; }
        public string type { get; set; }
        public string weeksLeft { get; set; }
        public string apProgress { get; set; }
        public string isActive { get; set; }
        public string apActual { get; set; }
        public string apGoal { get; set; }
        public string apRemaining { get; set; }
        public string apWeeklyActual { get; set; }
        public string apWeeklyGoal { get; set; }
        public string apWeeklyRemaining { get; set; }
        public string newAccountsActual { get; set; }
        public string newAccountsGoal { get; set; }
        public string apBonus { get; set; }
        public string isBonusWon { get; set; }
        public string newAccountsBonus { get; set; }
        public string isAccountBonusWon { get; set; }
        public string noPay { get; set; }
        public string weekAndYear { get; set; }
        public string apAchievedStatus { get; set; }
        public string weeklyAPAchievedStatus { get; set; }
        public string newAccountsStatus { get; set; }
        public string isBadgeActive { get; set; }
        public string apWeeklyProgress { get; set; }
        public string newAccountsProgress { get; set; }
        public string isFirstYear { get; set; }
        public string endDate { get; set; }


    }
    public class TripleCrownContestResponse
    {
        public string id { get; set; }
        public List<TripleCrownContestDetails> TripleCrownContestDetails { get; set; }
        //public TripleCrownContestResponse(List<TripleCrownContestDetails> tripleCrownContestDetails)
        //{
        //    this.TripleCrownContestDetails = tripleCrownContestDetails;
        //}
    }

    public class TripleCrownContestResult
    {
        public List<TripleCrownContestDetails> closedTripleCrownDetails { get; set; }
        public TripleCrownContestResult(List<TripleCrownContestDetails> tripleCrownContestDetails)
        {
            this.closedTripleCrownDetails = tripleCrownContestDetails;
        }
    }

    public static class TripleCrownContestRC
    {
        const string fal = "FALSE";
        [FunctionName("TripleCrownContestRC")]
        public static IActionResult RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "triplecrowncontestrc/{id}")]
                HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "TripleCrownContest",
                ConnectionStringSetting = "contesthub_DOCUMENTDB",
                PartitionKey ="/id"

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
            log.Info($"Processed request for {id} in TripleCrownContest");

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "TripleCrownContest");

            var query = client.CreateDocumentQuery<TripleCrownContestResponse>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = Int32.MaxValue }).Where(d => d.id.ToUpper() == id.ToUpper()).ToList();

            var tripleCrownContestDetails = query[0].TripleCrownContestDetails.Where(a => a.isFirstYear == "false").ToList();

            tripleCrownContestDetails.ForEach(a => a.endDate = "Ended " + a.endDate);


            if (tripleCrownContestDetails == null || tripleCrownContestDetails.Count == 0)
            {
                log.Info($"No data");
                return new NoContentResult();
            }
            else
            {
                TripleCrownContestResult result = new TripleCrownContestResult(tripleCrownContestDetails);
                return new OkObjectResult(result);
            }



            //if (items == null || !items.Any())
            //{
            //    log.Info($"No data");
            //    return new NoContentResult();
            //}
            //else
            //{

            //    return new OkObjectResult(items.First());

            //}
        }
    }

}
