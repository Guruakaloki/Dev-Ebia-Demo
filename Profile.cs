
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
    #region POCO INPUT
    public class ProfileInput
    {
        public string writingnumber { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string tenure { get; set; }
        public string email { get; set; }
        public string territory { get; set; }
        public string tvp { get; set; }
        public string marketOp { get; set; }
        public string mkd { get; set; }
        public string rsc { get; set; }
        public string dsc { get; set; }
        public string cit { get; set; }

    }
    #endregion
    public static class Profile
    {
        [FunctionName("Profile")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "profile/{id}")]
                HttpRequestMessage req,
            [CosmosDB(
                databaseName: "contesthub",
                collectionName: "Profile",
                ConnectionStringSetting = "contesthub_DOCUMENTDB",
                PartitionKey ="/id",
                SqlQuery = "select * from Profile c where upper(c.id)=upper({id})"
                )]
            IEnumerable<ProfileInput> items,
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
            log.Info($"Processed request for {id} in Profile");

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
