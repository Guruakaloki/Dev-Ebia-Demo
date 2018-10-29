
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
    public class VIPResponseCurrent
    {
        public List<VIPElement> vipContestDetails { get; set; }
        public VIPResponseCurrent(List<VIPElement> vipElementList)
        {
            this.vipContestDetails = vipElementList;
        }
    }
    public class VIPElement
    {
        public string id { get; set; }
        public string logonid { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string weeksLeft { get; set; }
        public string apProgress { get; set; }
        public string isActive { get; set; }
        public string apActual { get; set; }
        public string apGoal { get; set; }
        public string apRemaining { get; set; }
        public string noPay { get; set; }
        public string annualBonus { get; set; }
        public string annualProjectedBonus { get; set; }
        public string quarterlyBonus { get; set; }
        public string quarterlyProjectedBonus { get; set; }
        public string isBonusWon { get; set; }
        public string weekAndYear { get; set; }
        public string apAchievedStatus { get; set; }
        public string isBadgeActive { get; set; }


    }
    public static class VIP
    {
        [FunctionName("VIP")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vip/{id}")]
                HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "VIP",
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

            log.Info($"Processed request for {id} in VIP");

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "VIP");

            Uri c2 = UriFactory.CreateDocumentCollectionUri("contesthub", "ProductionCalendar");

            IDocumentQuery<ProductionCalendar> dateQuery = client.CreateDocumentQuery<ProductionCalendar>(c2, new FeedOptions { EnableCrossPartitionQuery = true }).AsDocumentQuery();

            var dates = await dateQuery.ExecuteNextAsync<ProductionCalendar>();

            var year = dates.FirstOrDefault().closedProdYear;



            var query = client.CreateDocumentQuery<VIPElement>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = Int32.MaxValue }).
            Where(d => d.logonid.ToUpper() == id.ToUpper());


            List<VIPElement> vipElementList = query.ToList();
            vipElementList = vipElementList.Where(e => e.weekAndYear.Substring(e.weekAndYear.Length - 4, 4) == year).ToList();


            if (vipElementList.Count == 0) return new NoContentResult();

            else
            {
                VIPResponseCurrent vipResponse = new VIPResponseCurrent(vipElementList);
                return new OkObjectResult(vipResponse);
            }


        }
    }

}
