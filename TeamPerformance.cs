
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

    #region CONSTANTS
    public enum ORDER
    {
        Ascending, Descending
    }

    public enum SORTBY
    {
        AP, Firstname, Lastname, Accounts, NoPay
    }

    public enum PERIOD
    {
        YTD, QTD, WTD
    }

    public enum ROLE
    {
        ASC, DSC
    }
    #endregion

    #region POCO INPUT
    public class TeamPerformance
    {
        public string id { get; set; }
        public string period { get; set; }
        public string role { get; set; }
        public TeamPerformanceDetails teamPerformanceDetails { get; set; }

    }

    public class TeamPerformanceDetails
    {
        public string totalCount { get; set; }
        public List<TeamPerformanceElement> teamPerformanceList { get; set; }
    }

    public class TeamPerformanceElement
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string accounts { get; set; }
        public string ap { get; set; }
        public string noPay { get; set; }
    }

    #endregion

    #region POCO OUTPUT
    public class TeamPerformanceDetailsNoId
    {
        public string totalCount { get; set; }
        public List<TeamPerformanceElementNoRole> teamPerformanceList { get; set; }
    }

    public class TeamPerformanceElementNoRole
    {
        public TeamPerformanceElementNoRole(TeamPerformanceElement field)
        {
            this.firstName = field.firstName;
            this.lastName = field.lastName;
            this.accounts = field.accounts;
            this.ap = field.ap;
            this.noPay = field.noPay;
        }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string accounts { get; set; }
        public string ap { get; set; }
        public string noPay { get; set; }
    }

    public class TeamPerformanceNoId
    {
        public TeamPerformanceNoId(TeamPerformance teamPerformance, int totalCount)
        {
            TeamPerformanceDetails _teamPerformanceDetails = teamPerformance.teamPerformanceDetails;

            // Format names
            List<TeamPerformanceElementNoRole> newElem = new List<TeamPerformanceElementNoRole>();
            foreach (TeamPerformanceElement item in _teamPerformanceDetails.teamPerformanceList)
            {
                // Remove role
                newElem.Add(new TeamPerformanceElementNoRole(item));
            }

            this.teamPerformanceDetails = new TeamPerformanceDetailsNoId();
            this.teamPerformanceDetails.teamPerformanceList = newElem;
            this.teamPerformanceDetails.totalCount = totalCount.ToString();
        }
        public TeamPerformanceDetailsNoId teamPerformanceDetails { get; set; }
    }
    #endregion


    #region POST
    public class PostBody
    {
        public string id { get; set; }
        public ORDER sortOrder { get; set; }
        public SORTBY sortBy { get; set; }
        public int limit { get; set; }
        public ROLE role { get; set; }
        public PERIOD period { get; set; }
        public int offset { get; set; }
    }

    public static class TeamPerformanceFunc
    {
        [FunctionName("TeamPerformance")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TeamPerformance")]
            HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "TeamPerformance",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
            TraceWriter log)
        {

            PostBody data = await req.Content.ReadAsAsync<PostBody>();

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
            //if(dId.ToLower()!=data.id.ToLower())
            //{
            //    return new UnauthorizedResult();
            //}
            log.Info($"Processed request for {data.id} in Profile, settings: sortOrder: {data.sortOrder}, sortBy: {data.sortBy}, limit: {data.limit}," +
$"role: {data.role}, period: {data.period}");

            // Query data
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "TeamPerformance");

            // filter WTD,QTD,YTD
            string id = $"{data.id}_{data.role}_{data.period}";
            IDocumentQuery<TeamPerformance> query = client.CreateDocumentQuery<TeamPerformance>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                          .Where(d => d.id.ToUpper() == id.ToUpper()).AsDocumentQuery();
            //.Where(d => d.period == data.period.ToString()).AsDocumentQuery();

            // One result
            TeamPerformance item = null;
            while (query.HasMoreResults)
            {
                foreach (TeamPerformance result in await query.ExecuteNextAsync())
                {
                    item = result;
                    break;
                }
            }

            if (item == null) return new NoContentResult();

            // Sort
            List<TeamPerformanceElement> sorted = new List<TeamPerformanceElement>();

            if (data.sortBy == SORTBY.AP)
            {
                if (data.sortOrder == ORDER.Descending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderByDescending(elem => Convert.ToDouble(elem.ap.Replace("$", ""))).ToList();
                }
                else if (data.sortOrder == ORDER.Ascending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                                              .OrderBy(elem => Convert.ToDouble(elem.ap.Replace("$", ""))).ToList();
                }
                else
                {
                    log.Warning("Order is not specified correctly");
                    return new NoContentResult();
                }
            }
            else if (data.sortBy == SORTBY.Firstname)
            {
                if (data.sortOrder == ORDER.Descending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderByDescending(elem => (elem.firstName)).ToList();
                }
                else if (data.sortOrder == ORDER.Ascending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderBy(elem => (elem.firstName)).ToList();
                }
                else
                {
                    log.Warning("Order is not specified correctly");
                    return new NoContentResult();
                }
            }
            else if (data.sortBy == SORTBY.Lastname)
            {
                if (data.sortOrder == ORDER.Descending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderByDescending(elem => (elem.lastName)).ToList();
                }
                else if (data.sortOrder == ORDER.Ascending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderBy(elem => (elem.lastName)).ToList();
                }
                else
                {
                    log.Warning("Order is not specified correctly");
                    return new NoContentResult();
                }
            }
            else if (data.sortBy == SORTBY.Accounts)
            {
                if (data.sortOrder == ORDER.Descending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderByDescending(elem => Convert.ToDouble(Convert.ToDouble(elem.accounts))).ToList();
                }
                else if (data.sortOrder == ORDER.Ascending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderBy(elem => Convert.ToDouble(Convert.ToDouble(elem.accounts))).ToList();
                }
                else
                {
                    log.Warning("Order is not specified correctly");
                    return new NoContentResult();
                }
            }
            else if (data.sortBy == SORTBY.NoPay)
            {
                if (data.sortOrder == ORDER.Descending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderByDescending(elem => Convert.ToDouble(Convert.ToDouble(elem.noPay.Replace("%", "")))).ToList();
                }
                else if (data.sortOrder == ORDER.Ascending)
                {
                    sorted = item.teamPerformanceDetails.teamPerformanceList
                                 .OrderBy(elem => Convert.ToDouble(Convert.ToDouble(elem.noPay.Replace("%", "")))).ToList();
                }
                else
                {
                    log.Warning("Order is not specified correctly");
                    return new NoContentResult();
                }
            }
            else
            {
                // default only
                sorted = item.teamPerformanceDetails.teamPerformanceList;
            }


            //limit feature
            int totalCount = item.teamPerformanceDetails.teamPerformanceList.Count();
            item.teamPerformanceDetails.teamPerformanceList = sorted.Skip(((data.offset == 0 ? 1 : data.offset) - 1) * data.limit).Take(data.limit).ToList();


            // Build as per specifications         
            TeamPerformanceNoId finalRes = new TeamPerformanceNoId(item, totalCount);

            // Return
            return new OkObjectResult(finalRes);
        }
    }
    #endregion
}
