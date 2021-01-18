﻿using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using RESTfulAPITemplate.Api.Controller.Extension;
#if (ENABLEJWTAUTHENTICATION)
using Microsoft.AspNetCore.Authorization;
#endif
#if (LOCALMEMORYCACHE)
using Microsoft.Extensions.Caching.Memory;
#elif (DISTRIBUTEDCACHE)
using Microsoft.Extensions.Caching.Distributed;
using MessagePack;
#endif
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using RESTfulAPITemplate.Core.DTO;
using RESTfulAPITemplate.Core.DomainModel;
using RESTfulAPITemplate.Core.Entity;
using RESTfulAPITemplate.Core.Interface;
using RESTfulAPITemplate.Core.Specification;
using RESTfulAPITemplate.Core.Specification.Filter;
#if (RESTFULAPIHELPER)
using Larsson.RESTfulAPIHelper.Interface;
using Larsson.RESTfulAPIHelper.Pagination;
using Larsson.RESTfulAPIHelper.Shaping;
using Larsson.RESTfulAPIHelper.Caching;
#endif

namespace RESTfulAPITemplate.Api.Controller
{
    /// <summary>
    /// Product
    /// </summary>
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("[controller]")]

#if (ENABLEJWTAUTHENTICATION)

    [Authorize]

#endif
    public class ProductController : ControllerBase
    {

#if (LOCALMEMORYCACHE)

        private readonly IMemoryCache _cache;

#elif (DISTRIBUTEDCACHE)

        private readonly IDistributedCache _cache;

#endif

#if (RESTFULAPIHELPER)

        private readonly LinkGenerator _generator;

#endif

        private readonly ILogger<ProductController> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _repository;

        public ProductController(ILogger<ProductController> logger, IMapper mapper, IUnitOfWork unitOfWork, IProductRepository repository

#if (LOCALMEMORYCACHE)

        , IMemoryCache cache

#elif (DISTRIBUTEDCACHE)

        , IDistributedCache cache

#endif

#if (RESTFULAPIHELPER)

        , LinkGenerator generator

#endif

        )
        {
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _repository = repository;

#if (LOCALMEMORYCACHE || DISTRIBUTEDCACHE)

            _cache = cache;
#endif

#if (RESTFULAPIHELPER)

            _generator = generator;

#endif

        }

        #region snippet_GetProductsAsync
        /// <summary>
        /// Get Products
        /// </summary>
        /// <param name="productFilterDTO">The params for filter and page products</param>
        /// <returns>products</returns>
        /// <response code="200">Returns the target products</response>
        /// <response code="304">Server-side data is not modified</response>
        /// <response code="401">Authorization verification is not passed</response>
        /// <response code="404">Did not get any product</response>
        /// <response code="406">Server does not support the media-type specified in the request</response>
        [HttpGet]

#if (ENABLEJWTAUTHENTICATION)

        [AllowAnonymous]

#endif

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ProductDTO>> GetProductsAsync([FromQuery] ProductFilterDTO productFilterDTO)
        {
            if (productFilterDTO == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            IEnumerable<Product> products;

            var projectFilter = _mapper.Map<ProductFilter>(productFilterDTO);

#if (LOCALMEMORYCACHE)

            var cacheKey = $"{nameof(ProductController)}_{nameof(GetProductsAsync)}_{Request.QueryString.Value}";
            products = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.Size = 2;
                entry.SetSlidingExpiration(TimeSpan.FromSeconds(15));
                return await _repository.ListWithOrderAsync<ProductDTO>(new ProductSpec(projectFilter),
                    projectFilter.OrderBy);
            });

#elif (DISTRIBUTEDCACHE)

            var cacheKey = $"{nameof(ProductController)}_{nameof(GetProductsAsync)}_{Request.QueryString.Value}";

#if (RESTFULAPIHELPER)

            products = await _cache.CreateOrGetCacheAsync(cacheKey,
                async () => await _repository.ListWithOrderAsync<ProductDTO>(new ProductSpec(projectFilter),
                    projectFilter.OrderBy),
                options => options.SetSlidingExpiration(TimeSpan.FromSeconds(15)));

#else
            var pagedProductsBytes = await _cache.GetAsync(cacheKey);
            if (pagedProductsBytes != null)
            {
                products = MessagePackSerializer.Deserialize<IEnumerable<Product>>(pagedProductsBytes);
            }
            else
            {
                products = await _repository.ListWithOrderAsync<ProductDTO>(new ProductSpec(projectFilter), 
                    projectFilter.OrderBy);
                var productsResourceBytes = MessagePackSerializer.Serialize(products);
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(15));
                await _cache.SetAsync(cacheKey, productsResourceBytes, options);
            }

#endif

#else

            pagedProducts = await _repository.GetProducts(projectFilter);

#endif

#if (RESTFULAPIHELPER)

            var filterProps = new Dictionary<string, object>();
            filterProps.Add("name", projectFilter.Name);
            filterProps.Add("description", projectFilter.Description);

            var pagedProducts = new PagedListBase<Product>(productFilterDTO.PageIndex, productFilterDTO.PageSize, products.Count(), products);

            Response.SetPaginationHead(pagedProducts, projectFilter, filterProps,
                values => _generator.GetUriByAction(HttpContext, controller: "Product", action: "GetProducts", values: values),
                meta => JsonSerializer.Serialize(meta, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            );

#endif

            var mappedProducts = _mapper.Map<IEnumerable<ProductDTO>>(products);

#if (RESTFULAPIHELPER)

            return Ok(mappedProducts.ToDynamicIEnumerable(projectFilter.Fields));
#else

            return Ok(mappedProducts);

#endif

        }

        #endregion

        #region snippet_GetProductAsync   
        /// <summary>
        /// Get product by productId
        /// </summary>
        /// <param name="id">productId</param>
        /// <param name="fields">fields to keep</param>
        /// <returns>product</returns>
        /// <response code="200">Returns the target product</response>
        /// <response code="304">Client-side data is up to date</response>
        /// <response code="401">Authorization verification is not passed</response>
        /// <response code="404">Did not get any product</response>
        /// <response code="406">Server does not support the media-type specified in the request</response>
        [HttpGet("{id}", Name = nameof(GetProductAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        public async Task<ActionResult<ProductDTO>> GetProductAsync(Guid id, string fields)
        {
            var claims = HttpContext.User?.Claims;
            var username = claims.SingleOrDefault(x => x.Type == "RESTfulAPITemplateUserName")?.Value;
            Console.WriteLine($"username from jwt is \"{username}\"");

            var result = await _repository.TryGetByIdAsNoTrackingAsync(id);
            if (!result.has)
            {
                return NotFound();
            }

#if (RESTFULAPIHELPER)

            return Ok(_mapper.Map<ProductDTO>(result.entity).ToDynamic(fields));

#else

            return Ok(_mapper.Map<ProductDTO>(result.entity));

#endif
        }
        #endregion

        #region snippet_GetProductsByIdsAsync   
        /// <summary>
        /// Get products by productIds
        /// </summary>
        /// <param name="ids">productIds</param>
        /// <returns>products</returns>
        /// <response code="200">Returns the target products</response>
        /// <response code="304">Client-side data is up to date</response>
        /// <response code="400">The product to be created is null</response>
        /// <response code="401">Authorization verification is not passed</response>
        /// <response code="404">Did not get any product</response>
        /// <response code="406">Server does not support the media-type specified in the request</response>
        /// <response code="422">DTO productCreateDTO failed to pass the model validation</response>
        [HttpGet("s/{ids}", Name = nameof(GetProductsByIds))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByIds([ModelBinder(BinderType = typeof(MyArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var cacheKey = $"{nameof(ProductController)}_{nameof(GetProductsByIds)}_{string.Join(';', ids.OrderBy(x => x))}";

            (bool has, IEnumerable<Product> entities) result;

#if (DISTRIBUTEDCACHE)

    
    #if (RESTFULAPIHELPER)

            result = await _cache.CreateOrGetCacheAsync(cacheKey,
                async () => await _repository.TryListAsNoTrackingAsync(new ProductIdsSpec(ids)),
                options => options.SetAbsoluteExpiration(TimeSpan.FromSeconds(5)));

    #else

            var resultBytes = await _cache.GetAsync(cacheKey);
            if (resultBytes != null)
            {
                result = MessagePackSerializer.Deserialize<(bool has, IEnumerable<Product> entities)>(resultBytes);
            }
            else
            {
                result = await _repository.TryListAsNoTrackingAsync(new ProductIdsSpec(ids));
                var resultResourceBytes = MessagePackSerializer.Serialize(result);
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(5));
                await _cache.SetAsync(cacheKey, resultResourceBytes, options);
            }
    #endif

#else

            result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.Size = 2;
                entry.SetSlidingExpiration(TimeSpan.FromSeconds(5));
                return await _repository.TryListAsNoTrackingAsync(new ProductIdsSpec(ids));
            });

#endif

            if (!result.has)
            {
                return NotFound();
            }

            var mappedproducts = _mapper.Map<IEnumerable<ProductDTO>>(result.entities);

            return Ok(mappedproducts);
        }
        #endregion

        #region snippet_GetProductsEachAsync

#if (!OBSOLETESQLSERVER)

        /// <summary>
        /// Get products asynchronously by product id
        /// </summary>
        /// <returns>products</returns>
        /// <response code="200">Returns the target product</response>
        /// <response code="304">server-side cached data has not changed</response>
        /// <response code="401">Authorization verification is not passed</response>
        /// <response code="404">Did not get any product</response>
        /// <response code="406">Server does not support the media-type specified in the request</response>
        [HttpGet("async")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        public async IAsyncEnumerable<ProductDTO> GetProductsEachAsync() // larsson：IAsyncEnumerable 是 net core 3 中 c#8.0 的新特性
        {
            var products = _repository.GetProductsEachAsync();

            await foreach (var product in products)
            {
                if (product.IsOnSale)
                {
                    yield return _mapper.Map<ProductDTO>(product);
                }
            }
        }

#endif

        #endregion

        #region snippet_CreateProductAsync
        /// <summary>
        /// Create a product
        /// </summary>
        /// <param name="productCreateDTO">The product to be created</param>
        /// <returns>The created new product</returns>
        /// <response code="201">Create the product succeed and returns the newly created product</response>
        /// <response code="400">The product to be created is null</response>
        /// <response code="401">Authorization verification is not passed</response>
        /// <response code="406">Server does not support the media-type specified in the request</response>
        /// <response code="415">Server cannot accept the media-type of the incoming data.</response>
        /// <response code="422">DTO productCreateDTO failed to pass the model validation</response>
        /// <response code="500">Adding product service side failed</response>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)] // 格式数据请求使用[Consumes]，若没有该属性，则直接识别请求头中的Content-Type，也就是[Consumes]可以省略，只要Content-Type为你需要的就能进行数据的模型绑定 // larsson：实际上asp.net core 默认就是json
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDTO>> CreateProductAsync([FromBody] ProductCreateDTO productCreateDTO)
        {
            // larsson：这里必须startup中设置禁用自动400响应，SuppressModelStateInvalidFilter = true。否则Model验证失败后这里的ProductResource永远是null而无法返回422

            if (productCreateDTO == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState); // larsson：如果要自定义422之外的响应则需要新建一个类继承UnprocessableEntityObjectResult
            }

            var product = _mapper.Map<Product>(productCreateDTO);
            _repository.Add(product);
            {
                if (!await _unitOfWork.SaveAsync())
                {
                    return StatusCode(500, "Adding product failed.");
                }

                var productDTO = _mapper.Map<ProductDTO>(product);

                // larsson：CreatedAtRoute在response中加入一个locaton头，包含GetProduct的调用
                return CreatedAtRoute(nameof(GetProductAsync), new
                {
                    id = product.Id
                }, productDTO);
            }
        }
        #endregion

        #region snippet_DeleteProductAsync
        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="productId">The id of product whitch to be deleting</param>
        /// <returns>Results of the product deleting</returns>
        /// <response code="204">Product successfully deleted</response>
        /// <response code="401">Authorization verification is not passed</response>
        /// <response code="404">Did not get any product</response>
        /// <response code="406">Server does not support the media-type specified in the request</response>
        /// <response code="500">Deleting product service side failed</response>
        [HttpDelete("{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProductAsync(Guid productId)
        {
            var result = await _repository.TryGetByIdAsNoTrackingAsync(productId);
            if (!result.has)
            {
                return NotFound();
            }

            _repository.Delete(result.entity);
            if (!await _unitOfWork.SaveAsync())
            {
                return StatusCode(500, "Delere product failed.");
            }

            return NoContent();
        }

        #endregion

        #region snippet_UpdateProductAsync
        /// <summary>
        /// Update a product
        /// </summary>
        /// <param name="productUpdateDTO">The product to be updated</param>
        /// <param name="productId">The id of the product to be updated</param>
        /// <returns>The update a product</returns>
        /// <response code="204">Updating product succeed</response>
        /// <response code="400">The product to be updated is null</response>
        /// <response code="401">Authorization verification is not passed</response>
        /// <response code="406">Server does not support the media-type specified in the request</response>
        /// <response code="412">Source of the product has changed</response> 
        /// <response code="415">Server cannot accept the media-type of the incoming data.</response>
        /// <response code="422">DTO productUpdateDTO failed to pass the model validation</response>
        /// <response code="500">Updating product service side failed</response>
        [HttpPut("{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProductAsync(Guid productId, [FromBody] ProductUpdateDTO productUpdateDTO)
        {
            if (productUpdateDTO == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState); // larsson：如果要自定义422之外的响应则需要新建一个类继承UnprocessableEntityObjectResult
            }

            var result = await _repository.TryGetByIdAsNoTrackingAsync(productId);
            if (!result.has)
            {
                return NotFound();
            }

            _mapper.Map(productUpdateDTO, result.entity);
            _repository.Update(result.entity);

            if (!await _unitOfWork.SaveAsync())
            {
                return StatusCode(500, "Updating product field.");
            }

            return NoContent();
        }
        #endregion

        #region snippet_PartiallyUpdateProductAsync
        /// <summary>
        /// Partially Update a product
        /// </summary>
        /// <param name="patchDoc">The JsonPatchDocument for the product partially updating</param>
        /// <param name="productId">The id of the product to be partially updated</param>
        /// <returns>The partially update a product</returns>
        /// <response code="204">Partially updating product succeed</response>
        /// <response code="400">The product to be partially updated is null</response>
        /// <response code="401">Authorization verification is not passed</response>
        /// <response code="406">Server does not support the media-type specified in the request</response>
        /// <response code="412">Source of the product has changed</response> 
        /// <response code="415">Server cannot accept the media-type of the incoming data.</response>
        /// <response code="422">DTO productUpdateDTO failed to pass the model validation</response>
        /// <response code="500">Partially updating product service side failed</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch("{productId}")]
        public async Task<IActionResult> PartiallyUpdateProductAsync(Guid productId, [FromBody] Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<ProductUpdateDTO> patchDoc)
        {
            // body的格式详见JsonPatchDocument

            if (patchDoc == null)
            {
                return BadRequest();
            }

            var result = await _repository.TryGetByIdAsNoTrackingAsync(productId);
            if (!result.has)
            {
                return NotFound();
            }

            var productUpdateDTO = _mapper.Map<ProductUpdateDTO>(result.entity);
            patchDoc.ApplyTo(productUpdateDTO, ModelState);

            TryValidateModel(productUpdateDTO);
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            _mapper.Map(productUpdateDTO, result.entity);
            _repository.Update(result.entity);

            var saveResult = await _unitOfWork.SaveUnableNoEffectAsync();
            if (!saveResult.Succeed)
            {
                return StatusCode(500, "Patching product field.");
            }

            return NoContent();
        }
        #endregion
    }
}