using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;

namespace ISDN.Controllers
{
    /// <summary>
    /// Base controller providing RDC-based data partitioning support.
    /// All controllers that need RDC filtering should inherit from this class.
    /// </summary>
    public abstract class BaseRdcController : Controller
    {
        /// <summary>
        /// Get the current user's RDC ID from JWT claims.
        /// Returns null if user is Head Office (rdc_id = NULL or 0) or claim doesn't exist.
        /// 
        /// Important: This method reads from JWT token claims, NOT from session or database.
        /// Users must re-login to get updated RDC assignments in their JWT token.
        /// </summary>
        protected int? GetUserRdcId()
        {
            var rdcIdClaim = User.FindFirst("rdc_id");
            if (rdcIdClaim == null)
            {
                return null; // Head Office user (no RDC restriction)
            }

            if (int.TryParse(rdcIdClaim.Value, out int rdcId))
            {
                return rdcId > 0 ? rdcId : null; // Return null for 0 (Head Office)
            }

            return null;
        }

        /// <summary>
        /// Check if current user is Head Office (has access to all RDCs).
        /// </summary>
        protected bool IsHeadOfficeUser()
        {
            var rdcId = GetUserRdcId();
            return !rdcId.HasValue; // Head Office users have no RDC restriction
        }

        /// <summary>
        /// Get current user ID from JWT claims.
        /// </summary>
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        /// <summary>
        /// Apply RDC filter to a queryable collection.
        /// If user is Head Office, returns all records.
        /// If user has RDC assignment, filters by RdcId.
        /// </summary>
        protected IQueryable<T> ApplyRdcFilter<T>(IQueryable<T> query) where T : class
        {
            var userRdcId = GetUserRdcId();

            // Head Office users see all data
            if (!userRdcId.HasValue)
            {
                return query;
            }

            // Filter by RDC for regional users
            var rdcIdProperty = typeof(T).GetProperty("RdcId");
            if (rdcIdProperty != null)
            {
                // Filter where RdcId matches user's RDC
                return query.Where(item => 
                    EF.Property<int?>(item, "RdcId") == userRdcId);
            }

            return query;
        }
    }
}
