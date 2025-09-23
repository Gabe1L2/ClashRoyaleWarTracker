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

    public class UserWithRoles
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Define specific permissions for features
    /// </summary>
    public static class Permissions
    {
        public const string ManageUsers = "ManageUsers";
        public const string ManageClans = "ManageClans";
        public const string ViewStatistics = "ViewStatistics";
        public const string UpdateWarData = "UpdateWarData";
        public const string ModifyPlayerData = "ModifyPlayerData";
        public const string ViewWarHistory = "ViewWarHistory";
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
                Permissions.ManageUsers,
                Permissions.ManageClans,
                Permissions.UpdateWarData,
                Permissions.ModifyPlayerData,
                Permissions.ViewWarHistory,
                Permissions.ViewStatistics
            },
            [UserRole.Management] = new()
            {
                Permissions.UpdateWarData,
                Permissions.ModifyPlayerData,
                Permissions.ViewWarHistory,
                Permissions.ViewStatistics
            },
            [UserRole.Coleader] = new()
            {
                Permissions.ModifyPlayerData,
                Permissions.ViewWarHistory,
                Permissions.ViewStatistics
            },
            [UserRole.Member] = new()
            {
                Permissions.ViewWarHistory,
                Permissions.ViewStatistics
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