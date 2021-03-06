
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
    public static class NationalConvention
    {
        [FunctionName("NationalConvention")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "nationalconvention/{id}")]
                HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "NationalConvention",
                ConnectionStringSetting = "contesthub_DOCUMENTDB",
                PartitionKey ="/id",
                SqlQuery = "select c.title, c.weeksLeft, c.apProgress, c.apActual, c.apGoal, c.apRemaining, c.noPay, c.fameCount, c.weekAndYear, c.awards, c.isAwardsWon, c.apAchievedStatus, c.isBadgeActive, c.isActive from NationalConvention c where upper(c.id)=upper({id})"
                )]
            IEnumerable<object> items,
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
            log.Info($"Processed request for {id} in NationalConvention");

            if (items == null || !items.Any())
            {
                log.Info($"No data");
                return new NoContentResult();
            }
            else
            {

                return new OkObjectResult(items.First());

            }
        }
    }

}
