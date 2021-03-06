﻿using Implem.DefinitionAccessor;
using Implem.Libraries.Classes;
using Implem.Libraries.DataSources.SqlServer;
using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.Converts;
using Implem.Pleasanter.Libraries.DataSources;
using Implem.Pleasanter.Libraries.DataTypes;
using Implem.Pleasanter.Libraries.General;
using Implem.Pleasanter.Libraries.Html;
using Implem.Pleasanter.Libraries.HtmlParts;
using Implem.Pleasanter.Libraries.Models;
using Implem.Pleasanter.Libraries.Requests;
using Implem.Pleasanter.Libraries.Responses;
using Implem.Pleasanter.Libraries.Security;
using Implem.Pleasanter.Libraries.Server;
using Implem.Pleasanter.Libraries.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Implem.Pleasanter.Models
{
    public class HealthModel : BaseModel
    {
        public long HealthId = 0;
        public int TenantCount = 0;
        public int UserCount = 0;
        public int ItemCount = 0;
        public int ErrorCount = 0;
        public double Elapsed = 0;
        public long SavedHealthId = 0;
        public int SavedTenantCount = 0;
        public int SavedUserCount = 0;
        public int SavedItemCount = 0;
        public int SavedErrorCount = 0;
        public double SavedElapsed = 0;
        public bool HealthId_Updated { get { return HealthId != SavedHealthId; } }
        public bool TenantCount_Updated { get { return TenantCount != SavedTenantCount; } }
        public bool UserCount_Updated { get { return UserCount != SavedUserCount; } }
        public bool ItemCount_Updated { get { return ItemCount != SavedItemCount; } }
        public bool ErrorCount_Updated { get { return ErrorCount != SavedErrorCount; } }
        public bool Elapsed_Updated { get { return Elapsed != SavedElapsed; } }

        public HealthModel()
        {
        }

        public HealthModel(
            bool setByForm = false,
            MethodTypes methodType = MethodTypes.NotSet)
        {
            OnConstructing();
            if (setByForm) SetByForm();
            MethodType = methodType;
            OnConstructed();
        }

        public HealthModel(
            long healthId,
            bool clearSessions = false,
            bool setByForm = false,
            MethodTypes methodType = MethodTypes.NotSet)
        {
            OnConstructing();
            HealthId = healthId;
            Get();
            if (clearSessions) ClearSessions();
            if (setByForm) SetByForm();
            MethodType = methodType;
            OnConstructed();
        }

        public HealthModel(DataRow dataRow)
        {
            OnConstructing();
            Set(dataRow);
            OnConstructed();
        }

        private void OnConstructing()
        {
        }

        private void OnConstructed()
        {
        }

        public void ClearSessions()
        {
        }

        public HealthModel Get(
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal,
            SqlColumnCollection column = null,
            SqlJoinCollection join = null,
            SqlWhereCollection where = null,
            SqlOrderByCollection orderBy = null,
            SqlParamCollection param = null,
            bool distinct = false,
            int top = 0)
        {
            Set(Rds.ExecuteTable(statements: Rds.SelectHealths(
                tableType: tableType,
                column: column ?? Rds.HealthsDefaultColumns(),
                join: join ??  Rds.HealthsJoinDefault(),
                where: where ?? Rds.HealthsWhereDefault(this),
                orderBy: orderBy ?? null,
                param: param ?? null,
                distinct: distinct,
                top: top)));
            return this;
        }

        public Error.Types Create(
            RdsUser rdsUser = null,
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal,
            SqlParamCollection param = null,
            bool paramAll = false,
            bool get = true)
        {
            var statements = CreateStatements(tableType, param, paramAll);
            var newId = Rds.ExecuteScalar_long(
                rdsUser: rdsUser,
                transactional: true,
                statements: statements.ToArray());
            HealthId = newId != 0 ? newId : HealthId;
            if (get) Get();
            return Error.Types.None;
        }

        public List<SqlStatement> CreateStatements(
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal,
            SqlParamCollection param = null,
            bool paramAll = false)
        {
            return new List<SqlStatement>
            {
                Rds.InsertHealths(
                    tableType: tableType,
                        selectIdentity: true,
                    param: param ?? Rds.HealthsParamDefault(
                        this, setDefault: true, paramAll: paramAll))
            };
        }

        public Error.Types Update(
            RdsUser rdsUser = null,
            SqlParamCollection param = null,
            bool paramAll = false,
            bool get = true)
        {
            SetBySession();
            var statements = UpdateStatements(param, paramAll);
            var count = Rds.ExecuteScalar_int(
                rdsUser: rdsUser,
                transactional: true,
                statements: statements.ToArray());
            if (count == 0) return Error.Types.UpdateConflicts;
            if (get) Get();
            return Error.Types.None;
        }

        public List<SqlStatement> UpdateStatements(
            SqlParamCollection param, bool paramAll = false)
        {
            var timestamp = Timestamp.ToDateTime();
            return new List<SqlStatement>
            {
                Rds.UpdateHealths(
                    verUp: VerUp,
                    where: Rds.HealthsWhereDefault(this)
                        .UpdatedTime(timestamp, _using: timestamp.InRange()),
                    param: param ?? Rds.HealthsParamDefault(this, paramAll: paramAll),
                    countRecord: true)
            };
        }

        public Error.Types UpdateOrCreate(
            RdsUser rdsUser = null,
            SqlWhereCollection where = null,
            SqlParamCollection param = null)
        {
            SetBySession();
            var statements = new List<SqlStatement>
            {
                Rds.UpdateOrInsertHealths(
                    selectIdentity: true,
                    where: where ?? Rds.HealthsWhereDefault(this),
                    param: param ?? Rds.HealthsParamDefault(this, setDefault: true))
            };
            var newId = Rds.ExecuteScalar_long(
                rdsUser: rdsUser,
                transactional: true,
                statements: statements.ToArray());
            HealthId = newId != 0 ? newId : HealthId;
            Get();
            return Error.Types.None;
        }

        public Error.Types Delete()
        {
            Rds.ExecuteNonQuery(
                transactional: true,
                statements: new SqlStatement[]
                {
                    Rds.DeleteHealths(
                        where: Rds.HealthsWhere().HealthId(HealthId))
                });
            return Error.Types.None;
        }

        public Error.Types Restore(long healthId)
        {
            HealthId = healthId;
            Rds.ExecuteNonQuery(
                connectionString: Parameters.Rds.OwnerConnectionString,
                transactional: true,
                statements: new SqlStatement[]
                {
                    Rds.RestoreHealths(
                        where: Rds.HealthsWhere().HealthId(HealthId))
                });
            return Error.Types.None;
        }

        public Error.Types PhysicalDelete(
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal)
        {
            Rds.ExecuteNonQuery(
                transactional: true,
                statements: Rds.PhysicalDeleteHealths(
                    tableType: tableType,
                    param: Rds.HealthsParam().HealthId(HealthId)));
            return Error.Types.None;
        }

        public void SetByForm()
        {
            Forms.Keys().ForEach(controlId =>
            {
                switch (controlId)
                {
                    case "Healths_TenantCount": TenantCount = Forms.Data(controlId).ToInt(); break;
                    case "Healths_UserCount": UserCount = Forms.Data(controlId).ToInt(); break;
                    case "Healths_ItemCount": ItemCount = Forms.Data(controlId).ToInt(); break;
                    case "Healths_ErrorCount": ErrorCount = Forms.Data(controlId).ToInt(); break;
                    case "Healths_Elapsed": Elapsed = Forms.Data(controlId).ToDouble(); break;
                    case "Healths_Timestamp": Timestamp = Forms.Data(controlId).ToString(); break;
                    case "Comments": Comments = Comments.Prepend(Forms.Data("Comments")); break;
                    case "VerUp": VerUp = Forms.Data(controlId).ToBool(); break;
                    default: break;
                }
            });
            if (Routes.Action() == "deletecomment")
            {
                DeleteCommentId = Forms.ControlId().Split(',')._2nd().ToInt();
                Comments.RemoveAll(o => o.CommentId == DeleteCommentId);
            }
            Forms.FileKeys().ForEach(controlId =>
            {
                switch (controlId)
                {
                    default: break;
                }
            });
        }

        private void SetBySession()
        {
        }

        private void Set(DataTable dataTable)
        {
            switch (dataTable.Rows.Count)
            {
                case 1: Set(dataTable.Rows[0]); break;
                case 0: AccessStatus = Databases.AccessStatuses.NotFound; break;
                default: AccessStatus = Databases.AccessStatuses.Overlap; break;
            }
        }

        private void Set(DataRow dataRow)
        {
            AccessStatus = Databases.AccessStatuses.Selected;
            foreach(DataColumn dataColumn in dataRow.Table.Columns)
            {
                var name = dataColumn.ColumnName;
                switch(name)
                {
                    case "HealthId": if (dataRow[name] != DBNull.Value) { HealthId = dataRow[name].ToLong(); SavedHealthId = HealthId; } break;
                    case "Ver": Ver = dataRow[name].ToInt(); SavedVer = Ver; break;
                    case "TenantCount": TenantCount = dataRow[name].ToInt(); SavedTenantCount = TenantCount; break;
                    case "UserCount": UserCount = dataRow[name].ToInt(); SavedUserCount = UserCount; break;
                    case "ItemCount": ItemCount = dataRow[name].ToInt(); SavedItemCount = ItemCount; break;
                    case "ErrorCount": ErrorCount = dataRow[name].ToInt(); SavedErrorCount = ErrorCount; break;
                    case "Elapsed": Elapsed = dataRow[name].ToDouble(); SavedElapsed = Elapsed; break;
                    case "Comments": Comments = dataRow["Comments"].ToString().Deserialize<Comments>() ?? new Comments(); SavedComments = Comments.ToJson(); break;
                    case "Creator": Creator = SiteInfo.User(dataRow.Int(name)); SavedCreator = Creator.Id; break;
                    case "Updator": Updator = SiteInfo.User(dataRow.Int(name)); SavedUpdator = Updator.Id; break;
                    case "CreatedTime": CreatedTime = new Time(dataRow, "CreatedTime"); SavedCreatedTime = CreatedTime.Value; break;
                    case "UpdatedTime": UpdatedTime = new Time(dataRow, "UpdatedTime"); Timestamp = dataRow.Field<DateTime>("UpdatedTime").ToString("yyyy/M/d H:m:s.fff"); SavedUpdatedTime = UpdatedTime.Value; break;
                    case "IsHistory": VerType = dataRow[name].ToBool() ? Versions.VerTypes.History : Versions.VerTypes.Latest; break;
                }
            }
        }

        public bool Updated()
        {
            return
                HealthId_Updated ||
                Ver_Updated ||
                TenantCount_Updated ||
                UserCount_Updated ||
                ItemCount_Updated ||
                ErrorCount_Updated ||
                Elapsed_Updated ||
                Comments_Updated ||
                Creator_Updated ||
                Updator_Updated ||
                CreatedTime_Updated ||
                UpdatedTime_Updated;
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public HealthModel(DateTime time)
        {
            var now = DateTime.Now;
            var dataTable = Rds.ExecuteDataSet(statements: new SqlStatement[]
            {
                Rds.SelectTenants(
                    dataTableName: "TenantCount",
                    column: Rds.TenantsColumn().TenantsCount()),
                Rds.SelectUsers(
                    dataTableName: "UserCount",
                    column: Rds.UsersColumn().UsersCount()),
                Rds.SelectItems(
                    dataTableName: "ItemCount",
                    column: Rds.ItemsColumn().ItemsCount()),
                Rds.SelectSysLogs(
                    dataTableName: "ErrorCount",
                    column: Rds.SysLogsColumn().SysLogsCount(),
                    where: Rds.SysLogsWhere()
                        .CreatedTime(time, _operator: ">=")
                        .ErrMessage(_operator: " is not null"))
            });
            TenantCount = dataTable.Tables["TenantCount"].Rows[0][0].ToInt();
            UserCount = dataTable.Tables["UserCount"].Rows[0][0].ToInt();
            ItemCount = dataTable.Tables["ItemCount"].Rows[0][0].ToInt();
            ErrorCount = dataTable.Tables["ErrorCount"].Rows[0][0].ToInt();
            Elapsed = (DateTime.Now - now).Milliseconds;
            Create();
        }
    }
}
