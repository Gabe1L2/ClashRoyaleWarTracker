namespace ClashRoyaleWarTracker.Application.Models
{
    /// <summary>
    /// User role hierarchy ordered by permission level (lower values = higher permissions)
    /// </summary>
    public enum UserRole
    {
        Admin = 0,      // Highest permissions
        Management = 1,
        Coleader = 2,
        Member = 3,
        Guest = 4       // Lowest permissions
    }

    /// <summary>
    /// Define specific permissions for features
    /// </summary>
    public static class Permissions
    {
        public const string ManageClans = "ManageClans";
        public const string ViewStatistics = "ViewStatistics";
        public const string UpdateData = "UpdateData";
        public const string ManageUsers = "ManageUsers";
        public const string ViewReports = "ViewReports";
        public const string ModifyPlayerData = "ModifyPlayerData";
    }

    /// <summary>
    /// Maps roles to their allowed permissions
    /// </summary>
    public static class RolePermissions
    {
        private static readonly Dictionary<UserRole, HashSet<string>> _rolePermissions = new()
        {
            [UserRole.Admin] = new()
            {
                Permissions.ManageClans,
                Permissions.ViewStatistics,
                Permissions.UpdateData,
                Permissions.ManageUsers,
                Permissions.ViewReports,
                Permissions.ModifyPlayerData
            },
            [UserRole.Management] = new()
            {
                Permissions.ViewStatistics,
                Permissions.UpdateData,
                Permissions.ViewReports,
                Permissions.ModifyPlayerData
            },
            [UserRole.Coleader] = new()
            {
                Permissions.ViewStatistics,
                Permissions.UpdateData,
                Permissions.ViewReports
            },
            [UserRole.Member] = new()
            {
                Permissions.ViewStatistics,
                Permissions.ViewReports
            },
            [UserRole.Guest] = new()
            {
                Permissions.ViewStatistics
            }
        };

        public static bool HasPermission(UserRole role, string permission)
        {
            return _rolePermissions.TryGetValue(role, out var permissions) && 
                   permissions.Contains(permission);
        }

        public static HashSet<string> GetPermissions(UserRole role)
        {
            return _rolePermissions.TryGetValue(role, out var permissions) 
                ? new HashSet<string>(permissions) 
                : new HashSet<string>();
        }
    }
}