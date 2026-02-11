namespace ISDN.Constants
{
    /// <summary>
    /// Static class containing role names used throughout the ISDN Distribution Management System.
    /// Using constants prevents typos and makes role management centralized and maintainable.
    /// </summary>
    public static class UserRoles
    {
        /// <summary>
        /// Admin role - Highest privilege level.
        /// Can manage users, roles, permissions, view audit logs, and oversee all operations.
        /// </summary>
        public const string Admin = "ADMIN";

        /// <summary>
        /// Head Office role - Senior management level.
        /// Can view reports, KPIs, and manage high-level operations.
        /// </summary>
        public const string HeadOffice = "HEAD_OFFICE";

        /// <summary>
        /// RDC Staff role - Regional Distribution Center staff.
        /// Can manage inventory and process orders.
        /// </summary>
        public const string RdcStaff = "RDC_STAFF";

        /// <summary>
        /// Logistics role - Logistics coordination team.
        /// Can schedule deliveries, track shipments, and manage logistics operations.
        /// </summary>
        public const string Logistics = "LOGISTICS";

        /// <summary>
        /// Driver role - Delivery drivers.
        /// Can view and update only their assigned deliveries.
        /// </summary>
        public const string Driver = "DRIVER";

        /// <summary>
        /// Finance role - Finance and accounting team.
        /// Can manage payments, invoices, and financial reports.
        /// </summary>
        public const string Finance = "FINANCE";

        /// <summary>
        /// Sales Rep role - Sales representatives.
        /// Can create orders on behalf of customers and track sales.
        /// </summary>
        public const string SalesRep = "SALES_REP";

        /// <summary>
        /// Customer role - End customers.
        /// Can browse products, place orders, track deliveries, and view invoices.
        /// </summary>
        public const string Customer = "CUSTOMER";
    }
}

