
using System;
using System.Collections.Generic;
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

    #region CONSTANTS

    #endregion

    #region POCO INPUT
    public class rawAssociate
    {
        public string id { get; set; }
        public long seqRoleTerritoryMarket { get; set; }
        public string week { get; set; }
        public string quarter { get; set; }
        public string year { get; set; }
        public string territoryMarketOp { get; set; }
        public string isTerritory { get; set; }
        public string isMarket { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string market { get; set; }
        public int currentRank { get; set; }
        public string rank { get { return currentRank.ToString(); } }
        public int maxRank { get; set; }
        public string ap { get; set; }
        public string noPay { get; set; }
        public string totalCount { get; set; }
    }
    #endregion

    #region POCO OUTPUT
    public class LeaderboardDetails
    {
        public LeaderboardDetails(rawAssociate asc, List<rawAssociate> associateList, int ascCount)
        {
            this.week = asc.week;
            this.quarter = asc.quarter;
            this.year = asc.year;
            this.territoryMarketOp = asc.territoryMarketOp;
            this.currentRank = asc.currentRank.ToString();
            this.maxRank = asc.maxRank.ToString();
            this.ap = asc.ap;
            this.noPay = asc.noPay;
            this.totalCount = ascCount.ToString();
            this.associateList = associateList;
        }

        public string week { get; set; }
        public string quarter { get; set; }
        public string year { get; set; }
        public string territoryMarketOp { get; set; }
        public string currentRank { get; set; }
        public string maxRank { get; set; }
        public string ap { get; set; }
        public string noPay { get; set; }
        public string totalCount { get; set; }
        public List<rawAssociate> associateList { get; set; }

    }

    public class associate
    {
        public associate(rawAssociate asc)
        {
            this.rank = asc.currentRank.ToString();
            this.name = asc.name;
            this.market = asc.market;
            this.noPay = asc.noPay;
            this.ap = asc.ap;
            //this.maxRank = asc.maxRank.ToString();
            // TODO: temporary
            //this.seqRoleTerritoryMarket = asc.seqRoleTerritoryMarket;
        }

        public string rank { get; set; }
        public string name { get; set; }
        public string market { get; set; }
        public string noPay { get; set; }
        public string ap { get; set; }

        //public string maxRank { get; set; }
        // TODO: temporary
        //public long seqRoleTerritoryMarket { get; set; }
    }
    #endregion


    #region POST
    public class LeaderBoardPostBody
    {
        public string id { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public string isTerritory { get; set; }
        public string isMarketOp { get; set; }
        public string searchText { get; set; }
    }

    public static class LeaderBoardFunc
    {
        [FunctionName("LeaderBoard")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "LeaderBoard")]
            HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "LeaderBoard",
                ConnectionStringSetting = "contesthub_DOCUMENTDB"
                )]
            DocumentClient client,
            TraceWriter log)
        {
            //try
            {

                LeaderBoardPostBody data = await req.Content.ReadAsAsync<LeaderBoardPostBody>();

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

                log.Info($"Processed request for {data.id} in LeaderBoard, settings: limit: {data.limit}, isTerritory: {data.isTerritory}, isMarketOp: {data.isMarketOp}," +
                     $"searchText: {data.searchText}");

                // Query data
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri("contesthub", "LeaderBoard");

                // Get top level associate
                // Prepare id filter
                string id = data.id.ToUpper();
                var territory = Convert.ToBoolean(data.isTerritory.ToLower());
                var isMarketOp = Convert.ToBoolean(data.isMarketOp.ToLower());
                if (!isMarketOp ^ territory) return new BadRequestResult();

                //check search string length if its a search query
                if (!string.IsNullOrEmpty(data.searchText) && data.searchText.Length < 3)
                {
                    return new BadRequestResult();
                }

                if (territory) id += "_T";
                else id += "_M";


                //rawAssociate item = client.CreateDocumentQuery<rawAssociate>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                //                                              .Where(d => d.id.ToUpper() == id.ToUpper())
                //                                              .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                //                                              .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                //                          .FirstOrDefault();

                IDocumentQuery<rawAssociate> query = client.CreateDocumentQuery<rawAssociate>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                                        .Where(d => d.id.ToUpper() == id).Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                                        .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                                     .AsDocumentQuery();
                var results = await query.ExecuteNextAsync<rawAssociate>();
                rawAssociate item = results.FirstOrDefault();

                // One result


                //foreach (rawAssociate result in results)
                //{
                //    item = result;
                //    break;
                //}
                if (item == null) return new NoContentResult();

                // Get offset
                int offset = (data.offset == 0) ? 0 : data.offset - 1;

                //// Get sublist 
                //IDocumentQuery<rawAssociate> subLevelQuery = null;

                List<rawAssociate> associateList = new List<rawAssociate>();

                int fullCount = 0;
                if (string.IsNullOrEmpty(data.searchText))
                {
                    //if market op is selected
                    if (isMarketOp)
                    {
                        associateList = client.CreateDocumentQuery<rawAssociate>(collectionUri, new FeedOptions { MaxItemCount = data.limit, EnableCrossPartitionQuery = true })
                                              .Where(d => d.role.ToLower() == item.role.ToLower())
                                              .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                              .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                              .Where(d => d.market.ToLower() == item.market.ToLower())
                                              .Where(d => d.seqRoleTerritoryMarket > offset * data.limit)
                                              .OrderBy(d => d.seqRoleTerritoryMarket)
                                              .Take(data.limit)
                                              .ToList();

                        fullCount = client.CreateDocumentQuery<rawAssociate>(collectionUri)
                                          .Where(d => d.role.ToLower() == item.role.ToLower())
                                          .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                          .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                          .Where(d => d.market.ToLower() == item.market.ToLower())
                                          .Count();
                    }
                    //if territory is selected
                    else
                    {

                        associateList = client.CreateDocumentQuery<rawAssociate>(collectionUri, new FeedOptions { MaxItemCount = data.limit, EnableCrossPartitionQuery = true })
                                              .Where(d => d.role.ToLower() == item.role.ToLower())
                                              .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                              .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                              .Where(d => d.territoryMarketOp.ToLower() == item.territoryMarketOp.ToLower())
                                              .Where(d => d.seqRoleTerritoryMarket > offset * data.limit)
                                              .OrderBy(d => d.seqRoleTerritoryMarket)
                                              .Take(data.limit).ToList();

                        fullCount = client.CreateDocumentQuery<rawAssociate>(collectionUri)
                                   .Where(d => d.role.ToLower() == item.role.ToLower())
                                   .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                   .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                   .Where(d => d.territoryMarketOp.ToLower() == item.territoryMarketOp.ToLower())
                                   .Count();
                    }


                }
                else
                {
                    //if market op is selected
                    if (isMarketOp)
                    {
                        associateList = client.CreateDocumentQuery<rawAssociate>(collectionUri, new FeedOptions { MaxItemCount = data.limit, EnableCrossPartitionQuery = true })
                                              .Where(d => d.role.ToLower() == item.role.ToLower())
                                              .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                              .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                              .Where(d => d.market.ToLower() == item.market.ToLower())
                                              .Where(d => d.name.ToLower().Contains(data.searchText.ToLower()))
                                              .OrderBy(d => d.currentRank)
                                              .ToList();

                        fullCount = client.CreateDocumentQuery<rawAssociate>(collectionUri)
                                          .Where(d => d.role.ToLower() == item.role.ToLower())
                                          .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                          .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                          .Where(d => d.market.ToLower() == item.market.ToLower())
                                          .Where(d => d.name.ToLower().Contains(data.searchText.ToLower()))
                                          .Count();
                    }
                    //if territory is selected
                    else
                    {
                        associateList = client.CreateDocumentQuery<rawAssociate>(collectionUri, new FeedOptions { MaxItemCount = data.limit, EnableCrossPartitionQuery = true })
                                              .Where(d => d.role.ToLower() == item.role.ToLower())
                                              .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                              .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                              .Where(d => d.territoryMarketOp.ToLower() == item.territoryMarketOp.ToLower())
                                              .Where(d => d.name.ToLower().Contains(data.searchText.ToLower()))
                                              .OrderBy(d => d.currentRank)
                                              .ToList();

                        fullCount = client.CreateDocumentQuery<rawAssociate>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                                          .Where(d => d.role.ToLower() == item.role.ToLower())
                                          .Where(d => d.isTerritory.ToLower() == data.isTerritory.ToLower())
                                          .Where(d => d.isMarket.ToLower() == data.isMarketOp.ToLower())
                                          .Where(d => d.territoryMarketOp.ToLower() == item.territoryMarketOp.ToLower())
                                          .Where(d => d.name.ToLower().Contains(data.searchText.ToLower()))
                                          .Count();
                    }

                }

                //Stopwatch st3 = new Stopwatch();
                //st3.Start();
                //while (subLevelQuery.HasMoreResults)
                //{
                //    foreach (rawAssociate result in await subLevelQuery.ExecuteNextAsync())
                //    {
                //        associateList.Add(new associate(result));
                //    }
                //}
                //st3.Stop();
                //Console.WriteLine("stop watch timer3:" + st3.ElapsedMilliseconds);


                // Offset in-memory for search queries

                if (!string.IsNullOrEmpty(data.searchText))
                {
                    var lower = offset * data.limit;
                    var upper = data.limit;
                    if (lower + upper > associateList.Count || lower >= associateList.Count)
                    {
                        lower = offset * data.limit;
                        upper = associateList.Count - lower;
                    }
                    // Limit
                    associateList = associateList.GetRange(lower, upper);
                }

                LeaderboardDetails leaderboardDetails = new LeaderboardDetails(item, associateList, fullCount);

                // Return
                return new OkObjectResult(leaderboardDetails);
            }
            //catch(Exception e)
            //{
            //    return new BadRequestResult();
            //}
            // Parse body


        }
    }
    #endregion
}
