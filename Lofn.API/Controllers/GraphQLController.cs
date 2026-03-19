using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lofn.API.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/graphql-docs")]
    public class GraphQLController : ControllerBase
    {
        /// <summary>
        /// Public GraphQL endpoint (POST /graphql).
        /// Access the interactive playground at /graphql to explore the schema.
        /// Supports queries: stores, products, categories, storeBySlug, featuredProducts.
        /// </summary>
        [HttpPost("public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GraphQLResponse), 200)]
        public IActionResult PublicGraphQL([FromBody] GraphQLRequest request)
        {
            return Ok(new { message = "Use POST /graphql directly. This endpoint exists for Swagger documentation only." });
        }

        /// <summary>
        /// Admin GraphQL endpoint (POST /graphql/admin).
        /// Requires authentication. Access the interactive playground at /graphql/admin.
        /// Supports queries: myStores, myProducts, myCategories, myOrders.
        /// </summary>
        [HttpPost("admin")]
        [Authorize]
        [ProducesResponseType(typeof(GraphQLResponse), 200)]
        public IActionResult AdminGraphQL([FromBody] GraphQLRequest request)
        {
            return Ok(new { message = "Use POST /graphql/admin directly. This endpoint exists for Swagger documentation only." });
        }
    }

    public class GraphQLRequest
    {
        /// <summary>GraphQL query string</summary>
        /// <example>{ stores { storeId name logoUrl products { name imageUrl } categories { name productCount } } }</example>
        public string Query { get; set; }

        /// <summary>Operation name (optional, for multi-operation documents)</summary>
        public string OperationName { get; set; }

        /// <summary>Variables as a JSON object (optional)</summary>
        public object Variables { get; set; }
    }

    public class GraphQLResponse
    {
        /// <summary>Query result data</summary>
        public object Data { get; set; }

        /// <summary>Errors, if any</summary>
        public object Errors { get; set; }
    }
}
