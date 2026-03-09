using ISDN.Models;
using System.Threading.Tasks;

namespace ISDN.Services
{
    /// <summary>
    /// Service interface for inventory management operations
    /// Handles stock reservations and returns using existing tables only
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Reserves stock when an order is placed (increases quantity_reserved)
        /// </summary>
        Task<bool> ReserveStockAsync(int productId, int rdcId, int quantity);

        /// <summary>
        /// Deducts stock when order is packed (decreases both quantity_available and quantity_reserved)
        /// </summary>
        Task<bool> DeductStockOnPackingAsync(int orderId);

        /// <summary>
        /// Returns stock to quarantine location when return is approved
        /// </summary>
        Task<bool> ReturnStockToQuarantineAsync(int orderId, int productId, int quantity, int rdcId);

        /// <summary>
        /// Checks if sufficient stock is available for an order
        /// </summary>
        Task<bool> CheckStockAvailabilityAsync(int productId, int rdcId, int quantity);

        /// <summary>
        /// Gets inventory details for a specific product and RDC
        /// </summary>
        Task<Inventory?> GetInventoryAsync(int productId, int rdcId);
    }
}
