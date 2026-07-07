using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderTracking.Application.Commands.CreateOrder;
using OrderTracking.Application.Commands.UpdateOrderStatus;
using OrderTracking.Application.DTOs;
using OrderTracking.Application.Queries.GetOrderById;
using OrderTracking.Application.Queries.GetOrders;

namespace OrderTracking.API.Controllers
{
    /// <summary>
    /// Orders API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all orders
        /// </summary>
        /// <returns>List of orders</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
        public async Task<IActionResult> GetOrders()
        {
            _logger.LogInformation("GET /api/orders - Getting all orders");
            var orders = await _mediator.Send(new GetOrdersQuery());
            return Ok(orders);
        }

        /// <summary>
        /// Gets an order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            _logger.LogInformation("GET /api/orders/{OrderId} - Getting order by ID", id);
            
            var order = await _mediator.Send(new GetOrderByIdQuery(id));
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", id);
                return NotFound($"Order with ID {id} not found");
            }

            return Ok(order);
        }

        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="request">Order data</param>
        /// <returns>Created order</returns>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            _logger.LogInformation("POST /api/orders - Creating new order");
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new CreateOrderCommand(request.Description);
            var order = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        /// <summary>
        /// Updates order status
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="request">Status update data</param>
        /// <returns>Updated order</returns>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            _logger.LogInformation("PATCH /api/orders/{OrderId}/status - Updating order status", id);
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new UpdateOrderStatusCommand(id, request.Status);
            var order = await _mediator.Send(command);
            return Ok(order);
        }
    }
}