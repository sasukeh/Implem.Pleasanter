﻿using Implem.DefinitionAccessor;
using Implem.Libraries.DataSources.SqlServer;
using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.DataSources;
using Implem.Pleasanter.Libraries.Requests;
using Implem.Pleasanter.Libraries.Server;
using Implem.Pleasanter.Libraries.Settings;
using Implem.Pleasanter.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Implem.Pleasanter.Libraries.Security
{
    public static class Permissions
    {
        public enum Types : long
        {
            NotSet = 0,                         // 00000000000000000000000000000000
            Read = 1,                           // 00000000000000000000000000000001
            Create = 2,                         // 00000000000000000000000000000010
            Update = 4,                         // 00000000000000000000000000000100
            Delete = 8,                         // 00000000000000000000000000001000
            SendMail = 16,                      // 00000000000000000000000000010000
            Export = 32,                        // 00000000000000000000000000100000
            Import = 64,                        // 00000000000000000000000001000000
            ManageSite = 128,                   // 00000000000000000000000010000000
            ManagePermission = 256,             // 00000000000000000000000100000000
            ManageTenant = 1073741824,          // 01000000000000000000000000000000
            ManageService = 2147483648,         // 10000000000000000000000000000000
        }

        public static Types Get(string name)
        {
            switch (name)
            {
                case "NotSet": return Types.NotSet;
                case "Read": return Types.Read;
                case "Create": return Types.Create;
                case "Update": return Types.Update;
                case "Delete": return Types.Delete;
                case "SendMail": return Types.SendMail;
                case "Export": return Types.Export;
                case "Import": return Types.Import;
                case "ManageSite": return Types.ManageSite;
                case "ManagePermission": return Types.ManagePermission;
                case "ManageTenant": return Types.ManageTenant;
                case "ManageService": return Types.ManageService;
                default: return Types.NotSet;
            }
        }

        public static Types General()
        {
            return (Types)Parameters.Permissions.General;
        }

        public static Types Manager()
        {
            return (Types)Parameters.Permissions.Manager;
        }

        public enum ColumnPermissionTypes
        {
            Deny,
            Read,
            Update
        }

        public static Dictionary<long, Types> Get(IEnumerable<long> targets)
        {
            return Hash(
                dataRows: Rds.ExecuteTable(statements:
                    Rds.SelectPermissions(
                        distinct: true,
                        column: Rds.PermissionsColumn()
                            .ReferenceId()
                            .PermissionType(),
                        where: Rds.PermissionsWhere()
                            .ReferenceId_In(targets.Where(o => o != 0))
                            .Or(Rds.PermissionsWhere()
                                .GroupId_In(sub: Rds.SelectGroupMembers(
                                    column: Rds.GroupMembersColumn().GroupId(),
                                    where: Rds.GroupMembersWhere()
                                        .Add(raw: DeptOrUser("GroupMembers"))))
                                .Add(raw: DeptOrUser("Permissions")))))
                                    .AsEnumerable());
        }

        public static SqlWhereCollection SetCanReadWhere(SiteSettings ss, SqlWhereCollection where)
        {
            if (ss.AllowedIntegratedSites != null)
            {
                return where.Or(new SqlWhereCollection()
                    .Add(raw: "[SiteId] in ({0})".Params(
                        ss.AllowedIntegratedSites.Join()))
                    .Add(
                        subLeft: ExistsPermissions(ss),
                        _operator: string.Empty));
            }
            else if (!ss.CanRead(site: true))
            {
                return where
                    .Add(raw: "[SiteId]={0}".Params(ss.SiteId))
                    .Add(
                        subLeft: ExistsPermissions(ss),
                        _operator: string.Empty);
            }
            else
            {
                return Routes.Controller() == "items"
                    ? where.Add(raw: "[SiteId]={0}".Params(ss.SiteId))
                    : where;
            }
        }

        public static SqlExists ExistsPermissions(SiteSettings ss)
        {
            return Rds.ExistsPermissions(
                where: Rds.PermissionsWhere()
                    .ReferenceId(raw: ss.IdColumnBracket())
                    .PermissionType(_operator: " & 1 = 1")
                    .Or(Rds.PermissionsWhere()
                        .GroupId_In(sub: Rds.SelectGroupMembers(
                            column: Rds.GroupMembersColumn().GroupId(),
                            where: Rds.GroupMembersWhere()
                                .Add(raw: DeptOrUser("GroupMembers"))))
                        .Add(raw: DeptOrUser("Permissions"))));
        }

        private static string DeptOrUser(string tableName)
        {
            return "((@_D <> 0 and [{0}].[DeptId]=@_D) or(@_U <> 0 and [{0}].[UserId]=@_U))"
                .Params(tableName);
        }

        private static Dictionary<long, Types> Hash(EnumerableRowCollection<DataRow> dataRows)
        {
            var hash = dataRows
                .Select(o => o["ReferenceId"].ToLong())
                .Distinct()
                .ToDictionary(o => o, o => Types.NotSet);
            dataRows.ForEach(dataRow =>
            {
                var key = dataRow["ReferenceId"].ToLong();
                hash[key] |= (Types)dataRow["PermissionType"].ToLong();
            });
            return hash;
        }

        public static Types Get(string controller, long id)
        {
            return controller.ToLower() == "items"
                ? Get(InheritPermission(id))
                : Types.NotSet;
        }

        public static Types Get(long siteId)
        {
            var data = Get(new List<long> { siteId });
            return data.Count() == 1
                ? data.First().Value
                : Types.NotSet;
        }

        public static bool Can(long siteId, Types type)
        {
            return ((Get(siteId) & type) == type);
        }

        public static bool CanRead(long siteId)
        {
            return ((Get(siteId) & Types.Read) == Types.Read);
        }

        public static long InheritPermission(long id)
        {
            return Rds.ExecuteScalar_long(statements:
                Rds.SelectSites(
                    column: Rds.SitesColumn().InheritPermission(),
                    where: Rds.SitesWhere()
                        .SiteId(sub: Rds.SelectItems(
                            column: Rds.ItemsColumn().SiteId(),
                            where: Rds.ItemsWhere().ReferenceId(id)))));
        }

        public static IEnumerable<long> AllowSites(
            IEnumerable<long> sites, string referenceType = null)
        {
            return Rds.ExecuteTable(statements:
                Rds.SelectSites(
                    column: Rds.SitesColumn().SiteId(),
                    where: Rds.SitesWhere()
                        .TenantId(Sessions.TenantId())
                        .SiteId_In(sites)
                        .ReferenceType(referenceType, _using: referenceType != null)
                        .Add(raw: Def.Sql.CanReadSites)))
                            .AsEnumerable()
                            .Select(o => o["SiteId"].ToLong());
        }

        public static IEnumerable<Column> AllowedColumns(
            this IEnumerable<Column> columns,
            bool checkPermission,
            IEnumerable<ColumnAccessControl> readColumnAccessControls)
        {
            return columns
                .Where(o => !checkPermission || o.CanRead || readColumnAccessControls?.Any(p =>
                    p.ColumnName == o.ColumnName && p.AllowedUsers?.Any() == true) == true);
        }

        public static IEnumerable<string> AllowedColumns(SiteSettings ss)
        {
            return ss.Columns.AllowedColumns(
                checkPermission: true,
                readColumnAccessControls: ss.ReadColumnAccessControls)
                    .Select(o => o.ColumnName)
                    .ToList();
        }

        public static bool Allowed(
            this List<ColumnAccessControl> columnAccessControls,
            Column column,
            Types? type,
            List<string> mine)
        {
            return columnAccessControls?
                .FirstOrDefault(o => o.ColumnName == column.ColumnName)?
                .Allowed(type, mine) != false;
        }

        public static bool HasPermission(this SiteSettings ss)
        {
            return ss.PermissionType != null || ss.ItemPermissionType != null;
        }

        public static bool CanRead(this SiteSettings ss, bool site = false)
        {
            switch (Routes.Controller())
            {
                case "depts":
                    return CanManageTenant();
                case "groups":
                    return CanReadGroup();
                case "users":
                    return CanManageTenant() || Sessions.UserId() == Routes.Id();
                default:
                    return ss.Can(Types.Read, site);
            }
        }

        public static bool CanCreate(this SiteSettings ss, bool site = false)
        {
            switch (Routes.Controller())
            {
                case "depts":
                case "users":
                    return CanManageTenant();
                case "groups":
                    return CanEditGroup();
                default:
                    return ss.Can(Types.Create, site);
            }
        }

        public static bool CanUpdate(this SiteSettings ss, bool site = false)
        {
            switch (Routes.Controller())
            {
                case "depts":
                    return CanManageTenant();
                case "groups":
                    return CanEditGroup();
                case "users":
                    return CanManageTenant() || Sessions.UserId() == Routes.Id();
                default:
                    if (ss.ReferenceType == "Sites")
                    {
                        return ss.CanManageSite();
                    }
                    else
                    {
                        return ss.Can(Types.Update, site);
                    }
            }
        }

        public static bool CanMove(SiteSettings source, SiteSettings destination)
        {
            return source.CanUpdate() && destination.CanUpdate();
        }

        public static bool CanDelete(this SiteSettings ss, bool site = false)
        {
            switch (Routes.Controller())
            {
                case "depts":
                    return CanManageTenant();
                case "groups":
                    return CanEditGroup();
                case "users":
                    return CanManageTenant() &&
                        Sessions.UserId() != Routes.Id();
                default:
                    if (ss.ReferenceType == "Sites")
                    {
                        return ss.CanManageSite();
                    }
                    else
                    {
                        return ss.Can(Types.Delete, site);
                    }
            }
        }

        public static bool CanSendMail(this SiteSettings ss, bool site = false)
        {
            if (!Contract.Mail()) return false;
            switch (ss.ReferenceType.ToLower())
            {
                case "depts":
                    return CanManageTenant();
                case "groups":
                    return CanEditGroup();
                case "users":
                    return CanManageTenant() || Sessions.UserId() == Routes.Id();
                default:
                    if (ss.ReferenceType == "Sites")
                    {
                        return ss.CanManageSite();
                    }
                    else
                    {
                        return ss.Can(Types.SendMail, site);
                    }
            }
        }

        public static bool CanImport(this SiteSettings ss, bool site = false)
        {
            return Contract.Import() && ss.Can(Types.Import, site);
        }

        public static bool CanExport(this SiteSettings ss, bool site = false)
        {
            return Contract.Export() && ss.Can(Types.Export, site);
        }

        public static bool CanManageSite(this SiteSettings ss, bool site = false)
        {
            return ss.Can(Types.ManageSite, site);
        }

        public static bool CanManagePermission(this SiteSettings ss, bool site = false)
        {
            return ss.Can(Types.ManagePermission, site);
        }

        public static ColumnPermissionTypes ColumnPermissionType(this Column self)
        {
            switch(Url.RouteData("action").ToLower())
            {
                case "new":
                    return self.CanCreate
                        ? ColumnPermissionTypes.Update
                        : ColumnPermissionTypes.Deny;
                default:
                    return self.CanRead && self.CanUpdate
                        ? ColumnPermissionTypes.Update
                        : self.CanRead
                            ? ColumnPermissionTypes.Read
                            : ColumnPermissionTypes.Deny;
            }
        }

        public static bool CanManageTenant()
        {
            return Sessions.User().TenantManager;
        }

        public static bool CanReadGroup()
        {
            return 
                Sessions.UserSettings().DisableGroupAdmin != true &&
                (Routes.Id() == 0 || CanManageTenant() || Groups().Any());
        }

        public static bool CanEditGroup()
        {
            return
                Sessions.UserSettings().DisableGroupAdmin != true &&
                (Routes.Id() == 0 || CanManageTenant() || Groups().Any(o => o["Admin"].ToBool()));
        }

        private static bool Can(this SiteSettings ss, Types type, bool site)
        {
            return (ss.GetPermissionType(site) & type) == type;
        }

        private static EnumerableRowCollection<DataRow> Groups()
        {
            return Rds.ExecuteTable(statements:
                Rds.SelectGroupMembers(
                    column: Rds.GroupMembersColumn().Admin(),
                    where: Rds.GroupMembersWhere()
                        .GroupId(Routes.Id())
                        .Or(Rds.GroupMembersWhere()
                            .DeptId(Sessions.DeptId())
                            .UserId(Sessions.UserId()))))
                                .AsEnumerable();
        }

        public static Types? Admins(Types? type = Types.NotSet)
        {
            var user = Sessions.User();
            if (user.TenantManager) type |= Types.ManageTenant;
            if (user.ServiceManager) type |= Types.ManageService;
            return type;
        }
    }
}