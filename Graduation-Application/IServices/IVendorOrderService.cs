using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;

namespace Graduation_Application.IServices
{
    public interface IVendorOrderService
    {
        Task<(List<VendorOrderDashboardDto> Orders, int TotalCount)> GetVendorOrdersAsync(
            int workshopId,
            VendorOrdersFilterDto filter
        );
        Task<VendorOrderDetailsDto> GetVendorOrderDetailsAsync(int orderId, int workshopId);
        Task UpdateVendorOrderStatusAsync(int orderId, int workshopId, string newStatus);
        Task<VendorRevenueStatisticsDto> GetVendorRevenueStatisticsAsync(
            int workshopId,
            DateTime? startDate,
            DateTime? endDate
        );
        Task<VendorOrderAnalyticsDto> GetVendorOrderAnalyticsAsync(
            int workshopId,
            DateTime? startDate,
            DateTime? endDate
        );
        Task<VendorDashboardMetricsDto> GetVendorDashboardMetricsAsync(int workshopId);
        Task<VendorActivityReportDto> GetVendorActivityReportAsync(
            int workshopId,
            string reportType,
            DateTime startDate,
            DateTime endDate
        );
        Task ProposeDeliveryDateAsync(int orderId, int workshopId, ProposeDeliveryDateRequestDto dto);
    }
}
