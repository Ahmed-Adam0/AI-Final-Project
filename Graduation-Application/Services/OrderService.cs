using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGenaricRepositories<Order> _orderRepository;
        private readonly IGenaricRepositories<VendorOrder> _vendorOrderRepository;
        private readonly IGenaricRepositories<Cart> _cartRepository;
        private readonly ICartService _cartService;
        private readonly INotificationService _notificationService;
        private readonly IInternalNotificationService _internalNotificationService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenaricRepositories<VendorMaterialOption> _vendorMaterialOptionRepository;
        private readonly IEmailService _emailService;
        private readonly IGenaricRepositories<Address> _addressRepository;
        private readonly IPaymentService _paymentService;

        public OrderService(
            IGenaricRepositories<Order> orderRepository,
            IGenaricRepositories<VendorOrder> vendorOrderRepository,
            IGenaricRepositories<Cart> cartRepository,
            ICartService cartService,
            INotificationService notificationService,
            IInternalNotificationService internalNotificationService,
            IPaymentGateway paymentGateway,
            IPaymentTransactionRepository paymentTransactionRepository,
            UserManager<ApplicationUser> userManager,
            IGenaricRepositories<VendorMaterialOption> vendorMaterialOptionRepository,
            IEmailService emailService,
            IGenaricRepositories<Address> addressRepository,
            IPaymentService paymentService
        )
        {
            _orderRepository = orderRepository;
            _vendorOrderRepository = vendorOrderRepository;
            _cartRepository = cartRepository;
            _cartService = cartService;
            _notificationService = notificationService;
            _internalNotificationService = internalNotificationService;
            _paymentGateway = paymentGateway;
            _paymentTransactionRepository = paymentTransactionRepository;
            _userManager = userManager;
            _vendorMaterialOptionRepository = vendorMaterialOptionRepository;
            _emailService = emailService;
            _addressRepository = addressRepository;
            _paymentService = paymentService;
        }

        public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository
                .Where(o => o.Status != "Cancelled")
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Items)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.StatusHistory)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var mappedOrders = new List<OrderResponseDto>();
            foreach (var order in orders)
            {
                mappedOrders.Add(await MapToDtoAsync(order));
            }
            return mappedOrders;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(
            string userId,
            CreateOrderDto request,
            ClaimsPrincipal user
        )
        {
            // Read cart WITHOUT tracking to avoid EF tracking collisions
            // Include User for workshops so we can retrieve vendor names/emails/phones
            var cart = await _cartRepository
                .WhereAsNoTracking(c => c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Workshop)
                            .ThenInclude(w => w.User)
                .FirstOrDefaultAsync();

            if (cart == null || !cart.Items.Any())
            {
                throw new Exception("Cart is empty");
            }

            decimal totalOrderPrice = cart.Items.Sum(ci => ci.CachedPrice * ci.Quantity);

            var phoneNumber = request.PhoneNumber;

            var allOptionIds = cart.Items
                .Where(ci => !string.IsNullOrEmpty(ci.SelectedOptionsJson) && ci.SelectedOptionsJson != "null")
                .SelectMany(ci => System.Text.Json.JsonSerializer.Deserialize<List<int>>(ci.SelectedOptionsJson) ?? new List<int>())
                .Distinct()
                .ToList();

            var optionsData = await _vendorMaterialOptionRepository
                .Where(o => allOptionIds.Contains(o.Id))
                .Include(o => o.Group)
                .ToListAsync();

            // Group items in cart by vendor (WorkshopId)
            var itemsByVendor = cart.Items
                .GroupBy(ci => ci.Product?.WorkshopId ?? 0)
                .ToList();

            var vendorOrders = new List<VendorOrder>();

            foreach (var vendorGroup in itemsByVendor)
            {
                var workshopId = vendorGroup.Key;
                if (workshopId == 0)
                {
                    throw new Exception("One of the products in the cart does not belong to a valid workshop.");
                }

                var sampleItem = vendorGroup.First();
                var workshop = sampleItem.Product?.Workshop;

                var vendorOrderItems = new List<OrderItem>();
                foreach (var ci in vendorGroup)
                {
                    var product = ci.Product;

                    var itemOptionIds = string.IsNullOrEmpty(ci.SelectedOptionsJson) || ci.SelectedOptionsJson == "null"
                        ? new List<int>()
                        : System.Text.Json.JsonSerializer.Deserialize<List<int>>(ci.SelectedOptionsJson) ?? new List<int>();

                    var selectedAttributes = optionsData
                        .Where(o => itemOptionIds.Contains(o.Id))
                        .Select(o => new SnapshotAttributeDto
                        {
                            NameAr = o.Group?.NameAr ?? "",
                            NameEn = o.Group?.NameEn ?? "",
                            ValueAr = o.ValueAr,
                            ValueEn = o.ValueEn
                        }).ToList();

                    string attrsJson = System.Text.Json.JsonSerializer.Serialize(selectedAttributes);

                    vendorOrderItems.Add(new OrderItem
                    {
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        SnapshotUnitPrice = ci.CachedPrice,
                        SnapshotProductNameEn = product?.NameEn ?? string.Empty,
                        SnapshotProductNameAr = product?.NameAr ?? string.Empty,
                        SnapshotVendorName = workshop?.WorkshopNameEn ?? string.Empty,
                        SnapshotAttributesJson = attrsJson,
                    });
                }

                decimal vendorTotalPrice = vendorGroup.Sum(ci => ci.CachedPrice * ci.Quantity);

                var vendorOrder = new VendorOrder
                {
                    WorkshopId = workshopId,
                    TotalPrice = vendorTotalPrice,
                    Status = VendorOrderStatus.Pending,
                    Items = vendorOrderItems,
                    StatusHistory = new List<VendorOrderStatusHistory>
                    {
                        new VendorOrderStatusHistory
                        {
                            OldStatus = "",
                            NewStatus = VendorOrderStatus.Pending.ToString()
                        }
                    }
                };

                vendorOrders.Add(vendorOrder);
            }

            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalOrderPrice,
                Status = "Pending",
                Address = request.Address,
                PhoneNumber = phoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Notes = request.Notes,
                VendorOrders = vendorOrders
            };

            // Check for matching primary address
            Address? matchedPrimary = null;
            if (request.AddressId.HasValue)
            {
                matchedPrimary = await _addressRepository.FirstOrDefaultAsync(a => a.Id == request.AddressId.Value && a.UserId == userId);
            }

            if (matchedPrimary == null)
            {
                var candidatePrimary = new Address
                {
                    UserId = userId,
                    City = request.Address,
                    Street = request.Address,
                    Notes = request.Notes ?? string.Empty
                };

                matchedPrimary = await _addressRepository.FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.City == candidatePrimary.City &&
                    a.Street == candidatePrimary.Street &&
                    (a.Area == candidatePrimary.Area || (string.IsNullOrEmpty(a.Area) && string.IsNullOrEmpty(candidatePrimary.Area))) &&
                    (a.BuildingNumber == candidatePrimary.BuildingNumber || (string.IsNullOrEmpty(a.BuildingNumber) && string.IsNullOrEmpty(candidatePrimary.BuildingNumber))) &&
                    (a.Notes == candidatePrimary.Notes || (string.IsNullOrEmpty(a.Notes) && string.IsNullOrEmpty(candidatePrimary.Notes)))
                );

                if (matchedPrimary == null)
                {
                    await _addressRepository.AddAsync(candidatePrimary);
                }
            }

            // Check for matching secondary address if provided
            if (!string.IsNullOrWhiteSpace(request.SecondaryAddress))
            {
                Address? matchedSecondary = null;
                if (request.SecondaryAddressId.HasValue)
                {
                    matchedSecondary = await _addressRepository.FirstOrDefaultAsync(a => a.Id == request.SecondaryAddressId.Value && a.UserId == userId);
                }

                if (matchedSecondary == null)
                {
                    var candidateSecondary = new Address
                    {
                        UserId = userId,
                        City = request.SecondaryAddress,
                        Street = request.SecondaryAddress,
                        Notes = "Secondary Address from Order"
                    };

                    matchedSecondary = await _addressRepository.FirstOrDefaultAsync(a =>
                        a.UserId == userId &&
                        a.City == candidateSecondary.City &&
                        a.Street == candidateSecondary.Street &&
                        (a.Area == candidateSecondary.Area || (string.IsNullOrEmpty(a.Area) && string.IsNullOrEmpty(candidateSecondary.Area))) &&
                        (a.BuildingNumber == candidateSecondary.BuildingNumber || (string.IsNullOrEmpty(a.BuildingNumber) && string.IsNullOrEmpty(candidateSecondary.BuildingNumber))) &&
                        (a.Notes == candidateSecondary.Notes || (string.IsNullOrEmpty(a.Notes) && string.IsNullOrEmpty(candidateSecondary.Notes)))
                    );

                    if (matchedSecondary == null)
                    {
                        await _addressRepository.AddAsync(candidateSecondary);
                    }
                }
            }

            // Add the master order (EF Core will cascade add vendor orders & items)
            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Clear the cart
            await _cartService.ClearCartAsync(userId);

            // Fetch user info for payment gateway & notifications
            var appUser = await _userManager.FindByIdAsync(userId);

            // Send customer confirmation notifications
            await _internalNotificationService.CreateAsync(
                userId,
                NotificationType.OrderPending,
                order.Id.ToString()
            );

            await _notificationService.SendOrderConfirmationAsync(
                userId,
                order.Id,
                order.TotalPrice
            );

            // Notify each vendor individually
            foreach (var vendorOrder in order.VendorOrders)
            {
                // Reload workshop details including user if not fully tracked
                var workshop = vendorOrder.Workshop;
                if (workshop == null)
                {
                    // Fallback load workshop
                    var cartGroup = itemsByVendor.FirstOrDefault(g => g.Key == vendorOrder.WorkshopId);
                    workshop = cartGroup?.First().Product?.Workshop;
                }

                if (workshop != null)
                {
                    var vendorUserId = workshop.UserId;

                    // Send internal notification to the vendor
                    await _internalNotificationService.CreateAsync(
                        vendorUserId,
                        NotificationType.NewOrder,
                        vendorOrder.Id.ToString()
                    );

                    // Send email notification to the vendor
                    if (workshop.User != null && !string.IsNullOrWhiteSpace(workshop.User.Email))
                    {
                        await _emailService.SendNewOrderVendorEmailAsync(workshop.User.Email, vendorOrder.Id);
                    }
                }
            }

            var responseDto = await GetOrderByIdAsync(order.Id);
            responseDto.PaymentUrl = null;

            return responseDto;
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Items)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.StatusHistory)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            return await MapToDtoAsync(order);
        }

        public async Task<List<OrderResponseDto>> GetMyOrdersAsync(string userId)
        {
            var orders = await _orderRepository
                .Where(o => o.UserId == userId)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Items)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.StatusHistory)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var mappedOrders = new List<OrderResponseDto>();
            foreach (var order in orders)
            {
                mappedOrders.Add(await MapToDtoAsync(order));
            }
            return mappedOrders;
        }

        public async Task CancelOrderAsync(int orderId, string userId)
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.StatusHistory)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Items)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found or you don't have permission to cancel it.");
            }

            if (order.Status == "Delivered" || order.Status == "Cancelled")
            {
                throw new Exception($"Cannot cancel order in '{order.Status}' status.");
            }

            // Check cancelable rules: block if any vendor order has shipped or delivered
            if (order.VendorOrders.Any(vo => vo.Status == VendorOrderStatus.Shipped || vo.Status == VendorOrderStatus.Delivered))
            {
                throw new Exception("Cannot cancel order because some items have already been shipped or delivered. Please contact support.");
            }

            foreach (var vendorOrder in order.VendorOrders)
            {
                if (vendorOrder.Status != VendorOrderStatus.Cancelled)
                {
                    var oldVoStatus = vendorOrder.Status.ToString();
                    vendorOrder.Status = VendorOrderStatus.Cancelled;
                    vendorOrder.UpdatedAt = DateTime.UtcNow;
                    vendorOrder.StatusHistory.Add(new VendorOrderStatusHistory
                    {
                        OldStatus = oldVoStatus,
                        NewStatus = VendorOrderStatus.Cancelled.ToString()
                    });
                }
            }

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.SaveChangesAsync();

            // Send internal notifications for cancellation
            await _internalNotificationService.CreateAsync(
                userId,
                NotificationType.OrderCancelled,
                orderId.ToString()
            );

            // Notify vendors of cancellation
            foreach (var vo in order.VendorOrders)
            {
                // Load workshop
                var workshop = await _vendorOrderRepository
                    .Where(v => v.Id == vo.Id)
                    .Select(v => v.Workshop)
                    .FirstOrDefaultAsync();

                if (workshop != null)
                {
                    await _internalNotificationService.CreateAsync(
                        workshop.UserId,
                        NotificationType.VendorOrderCancelled,
                        vo.Id.ToString()
                    );
                }
            }

            await _notificationService.SendOrderCancellationAsync(userId, order.Id);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var validStatuses = new[]
            {
                "Pending",
                "Confirmed",
                "In Progress",
                "Ready for Pickup",
                "Delivered",
                "Cancelled",
            };
            if (!validStatuses.Contains(status))
            {
                throw new Exception("Invalid status");
            }

            var order = await _orderRepository
                .Where(o => o.Id == orderId)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.StatusHistory)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            // Propagate status change to VendorOrders if applicable
            if (status == "Confirmed")
            {
                foreach (var vo in order.VendorOrders)
                {
                    if (vo.Status == VendorOrderStatus.Pending || vo.Status == VendorOrderStatus.AwaitingCustomerApproval)
                    {
                        var oldVoStatus = vo.Status.ToString();
                        vo.Status = VendorOrderStatus.Confirmed;
                        vo.UpdatedAt = DateTime.UtcNow;
                        vo.StatusHistory.Add(new VendorOrderStatusHistory
                        {
                            OldStatus = oldVoStatus,
                            NewStatus = VendorOrderStatus.Confirmed.ToString()
                        });
                    }
                }
            }
            else if (status == "In Progress")
            {
                foreach (var vo in order.VendorOrders)
                {
                    if (vo.Status == VendorOrderStatus.Confirmed || vo.Status == VendorOrderStatus.Pending)
                    {
                        var oldVoStatus = vo.Status.ToString();
                        vo.Status = VendorOrderStatus.InProgress;
                        vo.UpdatedAt = DateTime.UtcNow;
                        vo.StatusHistory.Add(new VendorOrderStatusHistory
                        {
                            OldStatus = oldVoStatus,
                            NewStatus = VendorOrderStatus.InProgress.ToString()
                        });
                    }
                }
            }
            else if (status == "Cancelled")
            {
                foreach (var vo in order.VendorOrders)
                {
                    if (vo.Status != VendorOrderStatus.Delivered && vo.Status != VendorOrderStatus.Cancelled)
                    {
                        var oldVoStatus = vo.Status.ToString();
                        vo.Status = VendorOrderStatus.Cancelled;
                        vo.UpdatedAt = DateTime.UtcNow;
                        vo.StatusHistory.Add(new VendorOrderStatusHistory
                        {
                            OldStatus = oldVoStatus,
                            NewStatus = VendorOrderStatus.Cancelled.ToString()
                        });
                    }
                }
            }

            await _orderRepository.SaveChangesAsync();

            // Send internal notifications based on status
            switch (status)
            {
                case "Confirmed":
                    await _internalNotificationService.CreateAsync(
                        order.UserId,
                        NotificationType.OrderConfirmed,
                        orderId.ToString()
                    );
                    break;
                case "In Progress":
                    await _internalNotificationService.CreateAsync(
                        order.UserId,
                        NotificationType.OrderInProgress,
                        orderId.ToString()
                    );
                    break;
                case "Shipped":
                    await _internalNotificationService.CreateAsync(
                        order.UserId,
                        NotificationType.OrderShipped,
                        orderId.ToString()
                    );
                    break;
                case "Delivered":
                    await _internalNotificationService.CreateAsync(
                        order.UserId,
                        NotificationType.OrderDelivered,
                        orderId.ToString()
                    );
                    break;
            }

            await _notificationService.SendOrderStatusUpdateAsync(order.UserId, order.Id, status);
        }

        public async Task<OrderResponseDto> UpdateOrderItemsAsync(
            int orderId,
            string userId,
            UpdateOrderItemsDto dto
        )
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Items)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found or you don't have permission to update it.");
            }

            if (order.Status != "Pending")
            {
                throw new Exception(
                    $"Cannot update order items unless the order is Pending. Current status is '{order.Status}'."
                );
            }

            if (dto.Items == null || dto.Items.Count == 0)
            {
                throw new Exception("No items provided for update.");
            }

            foreach (var itemDto in dto.Items)
            {
                var orderItem = order.VendorOrders
                    .SelectMany(vo => vo.Items ?? new List<OrderItem>())
                    .FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);

                if (orderItem == null)
                {
                    throw new Exception(
                        $"Product variant with ID {itemDto.ProductId} is not part of this order."
                    );
                }

                orderItem.Quantity = itemDto.Quantity;
                if (itemDto.UnitPrice.HasValue)
                {
                    orderItem.SnapshotUnitPrice = itemDto.UnitPrice.Value;
                }
            }

            // Recalculate vendor orders total prices
            foreach (var vo in order.VendorOrders)
            {
                vo.TotalPrice = vo.Items.Sum(oi => oi.SnapshotUnitPrice * oi.Quantity);
            }

            // Recalculate Master Order total price
            order.TotalPrice = order.VendorOrders.Sum(vo => vo.TotalPrice);
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.SaveChangesAsync();

            return await GetOrderByIdAsync(order.Id);
        }

        private async Task<OrderResponseDto> MapToDtoAsync(Order order)
        {

            return new OrderResponseDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                Address = order.Address,
                PhoneNumber = order.PhoneNumber,
                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email,
                Notes = order.Notes,
                VendorOrders = order.VendorOrders != null
                    ? order.VendorOrders
                        .Select(vo => new CustomerVendorOrderDto
                        {
                            Id = vo.Id,
                            Status = vo.Status.ToString(),
                            EstimatedDeliveryDate = vo.EstimatedDeliveryDate,
                            CanApprove = vo.Status == VendorOrderStatus.AwaitingCustomerApproval,
                            TotalPrice = vo.TotalPrice,
                            Items = vo.Items != null
                                ? vo.Items.Select(oi => new OrderItemResponseDto
                                {
                                    Id = oi.Id,
                                    ProductId = oi.ProductId,
                                    ProductNameEn = oi.SnapshotProductNameEn,
                                    ProductNameAr = oi.SnapshotProductNameAr,
                                    Status = vo.Status.ToString(),
                                    UnitPrice = oi.SnapshotUnitPrice,
                                    Quantity = oi.Quantity,
                                    Attributes = string.IsNullOrEmpty(oi.SnapshotAttributesJson)
                                        ? new List<SnapshotAttributeDto>()
                                        : System.Text.Json.JsonSerializer.Deserialize<List<SnapshotAttributeDto>>(oi.SnapshotAttributesJson)
                                            ?? new List<SnapshotAttributeDto>(),
                                }).ToList()
                                : new List<OrderItemResponseDto>()
                        }).ToList()
                    : new List<CustomerVendorOrderDto>(),
                StatusHistory = order.VendorOrders != null && order.VendorOrders.Any()
                    ? order.VendorOrders
                        .SelectMany(vo => vo.StatusHistory ?? new List<VendorOrderStatusHistory>())
                        .OrderByDescending(sh => sh.CreatedAt)
                        .Select(sh => new OrderStatusHistoryResponseDto
                        {
                            Id = sh.Id,
                            OldStatus = sh.OldStatus,
                            NewStatus = sh.NewStatus,
                            CreatedAt = sh.CreatedAt,
                        })
                        .FirstOrDefault()
                    : null,
                PaymentStatus = order.PaymentStatus,
            };
        }

        public async Task ApproveVendorOrderScheduleAsync(int vendorOrderId, string userId)
        {
            var vendorOrder = await _vendorOrderRepository
                .Where(vo => vo.Id == vendorOrderId)
                .Include(vo => vo.StatusHistory)
                .Include(vo => vo.MasterOrder)
                    .ThenInclude(mo => mo.VendorOrders)
                .Include(vo => vo.Workshop)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync();

            if (vendorOrder == null)
            {
                throw new Exception("Vendor order not found");
            }

            if (vendorOrder.MasterOrder.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to approve this schedule");
            }

            if (vendorOrder.Status != VendorOrderStatus.AwaitingCustomerApproval)
            {
                throw new Exception($"Cannot approve schedule when status is {vendorOrder.Status}");
            }

            var oldStatus = vendorOrder.Status.ToString();
            vendorOrder.Status = VendorOrderStatus.PendingPayment;
            vendorOrder.UpdatedAt = DateTime.UtcNow;

            vendorOrder.StatusHistory.Add(new VendorOrderStatusHistory
            {
                VendorOrderId = vendorOrderId,
                OldStatus = oldStatus,
                NewStatus = VendorOrderStatus.PendingPayment.ToString()
            });

            // Trigger milestone creation
            await _paymentService.CreateMilestoneIfNotExistAsync(vendorOrderId, VendorOrderStatus.PendingPayment, vendorOrder.TotalPrice);

            // Derive MasterOrder status
            var allVendorStatuses = vendorOrder.MasterOrder.VendorOrders
                .Select(v => v.Id == vendorOrderId ? VendorOrderStatus.PendingPayment : v.Status)
                .ToList();
            var derivedStatus = CalculateMasterOrderStatus(allVendorStatuses);
            vendorOrder.MasterOrder.Status = derivedStatus;
            vendorOrder.MasterOrder.UpdatedAt = DateTime.UtcNow;

            await _vendorOrderRepository.SaveChangesAsync();

            // Notify vendor
            var vendorUserId = vendorOrder.Workshop.UserId;
            await _internalNotificationService.CreateAsync(
                vendorUserId,
                NotificationType.DeliveryDateApproved,
                vendorOrderId.ToString()
            );

            if (vendorOrder.Workshop.User != null && !string.IsNullOrWhiteSpace(vendorOrder.Workshop.User.Email))
            {
                await _emailService.SendDeliveryDateApprovedEmailAsync(vendorOrder.Workshop.User.Email, vendorOrderId);
            }
        }

        public async Task RejectVendorOrderScheduleAsync(int vendorOrderId, string userId)
        {
            var vendorOrder = await _vendorOrderRepository
                .Where(vo => vo.Id == vendorOrderId)
                .Include(vo => vo.StatusHistory)
                .Include(vo => vo.MasterOrder)
                    .ThenInclude(mo => mo.VendorOrders)
                .Include(vo => vo.Workshop)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync();

            if (vendorOrder == null)
            {
                throw new Exception("Vendor order not found");
            }

            if (vendorOrder.MasterOrder.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to reject this schedule");
            }

            if (vendorOrder.Status != VendorOrderStatus.AwaitingCustomerApproval)
            {
                throw new Exception($"Cannot reject schedule when status is {vendorOrder.Status}");
            }

            var oldStatus = vendorOrder.Status.ToString();
            vendorOrder.Status = VendorOrderStatus.Cancelled;
            vendorOrder.UpdatedAt = DateTime.UtcNow;

            vendorOrder.StatusHistory.Add(new VendorOrderStatusHistory
            {
                VendorOrderId = vendorOrderId,
                OldStatus = oldStatus,
                NewStatus = VendorOrderStatus.Cancelled.ToString()
            });

            // Derive MasterOrder status
            var allVendorStatuses = vendorOrder.MasterOrder.VendorOrders
                .Select(v => v.Id == vendorOrderId ? VendorOrderStatus.Cancelled : v.Status)
                .ToList();
            var derivedStatus = CalculateMasterOrderStatus(allVendorStatuses);
            vendorOrder.MasterOrder.Status = derivedStatus;
            vendorOrder.MasterOrder.UpdatedAt = DateTime.UtcNow;

            await _vendorOrderRepository.SaveChangesAsync();

            // Notify vendor
            var vendorUserId = vendorOrder.Workshop.UserId;
            await _internalNotificationService.CreateAsync(
                vendorUserId,
                NotificationType.DeliveryDateRejected,
                vendorOrderId.ToString()
            );

            if (vendorOrder.Workshop.User != null && !string.IsNullOrWhiteSpace(vendorOrder.Workshop.User.Email))
            {
                await _emailService.SendDeliveryDateRejectedEmailAsync(vendorOrder.Workshop.User.Email, vendorOrderId);
            }
        }

        private string CalculateMasterOrderStatus(List<VendorOrderStatus> statuses)
        {
            if (!statuses.Any()) return "Pending";

            if (statuses.All(s => s == VendorOrderStatus.Cancelled))
                return "Cancelled";

            var nonCancelled = statuses.Where(s => s != VendorOrderStatus.Cancelled).ToList();
            if (nonCancelled.All(s => s == VendorOrderStatus.Delivered))
                return "Completed";

            if (statuses.Any(s => s == VendorOrderStatus.Delivered))
                return "PartiallyDelivered";

            if (statuses.Any(s => s == VendorOrderStatus.AwaitingCustomerApproval || 
                                s == VendorOrderStatus.PendingPayment ||
                                s == VendorOrderStatus.Confirmed || 
                                s == VendorOrderStatus.InProgress || 
                                s == VendorOrderStatus.Shipped))
                return "Processing";

            return "Pending";
        }
    }
}
