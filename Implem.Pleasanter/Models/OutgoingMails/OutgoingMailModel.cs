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
    public class OutgoingMailModel : BaseModel
    {
        public string ReferenceType = string.Empty;
        public long ReferenceId = 0;
        public int ReferenceVer = 0;
        public long OutgoingMailId = 0;
        public string Host = string.Empty;
        public int Port = 0;
        public System.Net.Mail.MailAddress From = null;
        public string To = string.Empty;
        public string Cc = string.Empty;
        public string Bcc = string.Empty;
        public Title Title = new Title();
        public string Body = string.Empty;
        public Time SentTime = new Time();
        public string DestinationSearchRange = string.Empty;
        public string DestinationSearchText = string.Empty;
        public string SavedReferenceType = string.Empty;
        public long SavedReferenceId = 0;
        public int SavedReferenceVer = 0;
        public long SavedOutgoingMailId = 0;
        public string SavedHost = string.Empty;
        public int SavedPort = 0;
        public string SavedFrom = "null";
        public string SavedTo = string.Empty;
        public string SavedCc = string.Empty;
        public string SavedBcc = string.Empty;
        public string SavedTitle = string.Empty;
        public string SavedBody = string.Empty;
        public DateTime SavedSentTime = 0.ToDateTime();
        public string SavedDestinationSearchRange = string.Empty;
        public string SavedDestinationSearchText = string.Empty;
        public bool ReferenceType_Updated { get { return ReferenceType != SavedReferenceType && ReferenceType != null; } }
        public bool ReferenceId_Updated { get { return ReferenceId != SavedReferenceId; } }
        public bool ReferenceVer_Updated { get { return ReferenceVer != SavedReferenceVer; } }
        public bool OutgoingMailId_Updated { get { return OutgoingMailId != SavedOutgoingMailId; } }
        public bool Host_Updated { get { return Host != SavedHost && Host != null; } }
        public bool Port_Updated { get { return Port != SavedPort; } }
        public bool From_Updated { get { return From.ToString() != SavedFrom && From.ToString() != null; } }
        public bool To_Updated { get { return To != SavedTo && To != null; } }
        public bool Cc_Updated { get { return Cc != SavedCc && Cc != null; } }
        public bool Bcc_Updated { get { return Bcc != SavedBcc && Bcc != null; } }
        public bool Title_Updated { get { return Title.Value != SavedTitle && Title.Value != null; } }
        public bool Body_Updated { get { return Body != SavedBody && Body != null; } }
        public bool SentTime_Updated { get { return SentTime.Value != SavedSentTime && SentTime.Value != null; } }

        public OutgoingMailModel()
        {
        }

        public OutgoingMailModel(
            bool setByForm = false,
            MethodTypes methodType = MethodTypes.NotSet)
        {
            OnConstructing();
            if (setByForm) SetByForm();
            MethodType = methodType;
            OnConstructed();
        }

        public OutgoingMailModel(
            long outgoingMailId,
            bool clearSessions = false,
            bool setByForm = false,
            MethodTypes methodType = MethodTypes.NotSet)
        {
            OnConstructing();
            OutgoingMailId = outgoingMailId;
            Get();
            if (clearSessions) ClearSessions();
            if (setByForm) SetByForm();
            MethodType = methodType;
            OnConstructed();
        }

        public OutgoingMailModel(DataRow dataRow)
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

        public OutgoingMailModel Get(
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal,
            SqlColumnCollection column = null,
            SqlJoinCollection join = null,
            SqlWhereCollection where = null,
            SqlOrderByCollection orderBy = null,
            SqlParamCollection param = null,
            bool distinct = false,
            int top = 0)
        {
            Set(Rds.ExecuteTable(statements: Rds.SelectOutgoingMails(
                tableType: tableType,
                column: column ?? Rds.OutgoingMailsDefaultColumns(),
                join: join ??  Rds.OutgoingMailsJoinDefault(),
                where: where ?? Rds.OutgoingMailsWhereDefault(this),
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
            OutgoingMailId = newId != 0 ? newId : OutgoingMailId;
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
                Rds.InsertOutgoingMails(
                    tableType: tableType,
                        selectIdentity: true,
                    param: param ?? Rds.OutgoingMailsParamDefault(
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
            var siteModel = new ItemModel(ReferenceId).GetSite();
            var ss = SiteSettingsUtilities.Get(siteModel, siteModel.SiteId);
            Libraries.Search.Indexes.Create(ss, ReferenceId, force: true);
            return Error.Types.None;
        }

        public List<SqlStatement> UpdateStatements(
            SqlParamCollection param, bool paramAll = false)
        {
            var timestamp = Timestamp.ToDateTime();
            return new List<SqlStatement>
            {
                Rds.UpdateOutgoingMails(
                    verUp: VerUp,
                    where: Rds.OutgoingMailsWhereDefault(this)
                        .UpdatedTime(timestamp, _using: timestamp.InRange()),
                    param: param ?? Rds.OutgoingMailsParamDefault(this, paramAll: paramAll),
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
                Rds.UpdateOrInsertOutgoingMails(
                    selectIdentity: true,
                    where: where ?? Rds.OutgoingMailsWhereDefault(this),
                    param: param ?? Rds.OutgoingMailsParamDefault(this, setDefault: true))
            };
            var newId = Rds.ExecuteScalar_long(
                rdsUser: rdsUser,
                transactional: true,
                statements: statements.ToArray());
            OutgoingMailId = newId != 0 ? newId : OutgoingMailId;
            Get();
            return Error.Types.None;
        }

        public Error.Types Delete()
        {
            Rds.ExecuteNonQuery(
                transactional: true,
                statements: new SqlStatement[]
                {
                    Rds.DeleteOutgoingMails(
                        where: Rds.OutgoingMailsWhere().OutgoingMailId(OutgoingMailId))
                });
            return Error.Types.None;
        }

        public Error.Types Restore(long outgoingMailId)
        {
            OutgoingMailId = outgoingMailId;
            Rds.ExecuteNonQuery(
                connectionString: Parameters.Rds.OwnerConnectionString,
                transactional: true,
                statements: new SqlStatement[]
                {
                    Rds.RestoreOutgoingMails(
                        where: Rds.OutgoingMailsWhere().OutgoingMailId(OutgoingMailId))
                });
            return Error.Types.None;
        }

        public Error.Types PhysicalDelete(
            Sqls.TableTypes tableType = Sqls.TableTypes.Normal)
        {
            Rds.ExecuteNonQuery(
                transactional: true,
                statements: Rds.PhysicalDeleteOutgoingMails(
                    tableType: tableType,
                    param: Rds.OutgoingMailsParam().OutgoingMailId(OutgoingMailId)));
            return Error.Types.None;
        }

        public void SetByForm()
        {
            Forms.Keys().ForEach(controlId =>
            {
                switch (controlId)
                {
                    case "OutgoingMails_To": To = Forms.List(controlId).Join(";"); break;
                    case "OutgoingMails_Cc": Cc = Forms.List(controlId).Join(";"); break;
                    case "OutgoingMails_Bcc": Bcc = Forms.List(controlId).Join(";"); break;
                    case "OutgoingMails_Title": Title = new Title(OutgoingMailId, Forms.Data(controlId)); break;
                    case "OutgoingMails_Body": Body = Forms.Data(controlId).ToString(); break;
                    case "OutgoingMails_SentTime": SentTime = new Time(Forms.Data(controlId).ToDateTime(), byForm: true); break;
                    case "OutgoingMails_DestinationSearchRange": DestinationSearchRange = Forms.Data(controlId).ToString(); break;
                    case "OutgoingMails_DestinationSearchText": DestinationSearchText = Forms.Data(controlId).ToString(); break;
                    case "OutgoingMails_Timestamp": Timestamp = Forms.Data(controlId).ToString(); break;
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
                    case "ReferenceType": if (dataRow[name] != DBNull.Value) { ReferenceType = dataRow[name].ToString(); SavedReferenceType = ReferenceType; } break;
                    case "ReferenceId": if (dataRow[name] != DBNull.Value) { ReferenceId = dataRow[name].ToLong(); SavedReferenceId = ReferenceId; } break;
                    case "ReferenceVer": if (dataRow[name] != DBNull.Value) { ReferenceVer = dataRow[name].ToInt(); SavedReferenceVer = ReferenceVer; } break;
                    case "OutgoingMailId": if (dataRow[name] != DBNull.Value) { OutgoingMailId = dataRow[name].ToLong(); SavedOutgoingMailId = OutgoingMailId; } break;
                    case "Ver": Ver = dataRow[name].ToInt(); SavedVer = Ver; break;
                    case "Host": Host = dataRow[name].ToString(); SavedHost = Host; break;
                    case "Port": Port = dataRow[name].ToInt(); SavedPort = Port; break;
                    case "From": From = new System.Net.Mail.MailAddress(dataRow.String("From")); SavedFrom = From.ToString(); break;
                    case "To": To = dataRow[name].ToString(); SavedTo = To; break;
                    case "Cc": Cc = dataRow[name].ToString(); SavedCc = Cc; break;
                    case "Bcc": Bcc = dataRow[name].ToString(); SavedBcc = Bcc; break;
                    case "Title": Title = new Title(dataRow, "OutgoingMailId"); SavedTitle = Title.Value; break;
                    case "Body": Body = dataRow[name].ToString(); SavedBody = Body; break;
                    case "SentTime": SentTime = new Time(dataRow, "SentTime"); SavedSentTime = SentTime.Value; break;
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
                ReferenceType_Updated ||
                ReferenceId_Updated ||
                ReferenceVer_Updated ||
                OutgoingMailId_Updated ||
                Ver_Updated ||
                Host_Updated ||
                Port_Updated ||
                From_Updated ||
                To_Updated ||
                Cc_Updated ||
                Bcc_Updated ||
                Title_Updated ||
                Body_Updated ||
                SentTime_Updated ||
                Comments_Updated ||
                Creator_Updated ||
                Updator_Updated ||
                CreatedTime_Updated ||
                UpdatedTime_Updated;
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public OutgoingMailModel(string reference, long referenceId)
        {
            if (reference.ToLower() == "items")
            {
                var itemModel = new ItemModel(referenceId);
                ReferenceType = itemModel.ReferenceType;
            }
            else
            {
                ReferenceType = reference.ToLower();
            }
            ReferenceId = referenceId;
            ReferenceVer = Forms.Int("Ver");
            From = OutgoingMailUtilities.From();
            SetByForm();
            if (Libraries.Mails.Addresses.FixedFrom(From))
            {
                Body += "\n\n{0}<{1}>".Params(From.DisplayName, From.Address);
            }
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public string GetDestinations()
        {
            if (!Contract.Mail())
            {
                return Error.Types.Restricted.MessageJson();
            }
            var siteModel = new ItemModel(ReferenceId).GetSite();
            var ss = siteModel.SitesSiteSettings(ReferenceId);
            return new OutgoingMailsResponseCollection(this)
                .Html("#OutgoingMails_MailAddresses",
                    new HtmlBuilder().SelectableItems(
                        listItemCollection: OutgoingMailUtilities.Destinations(
                            referenceId: siteModel.InheritPermission,
                            addressBook: OutgoingMailUtilities.AddressBook(ss),
                            searchRange: DestinationSearchRange,
                            searchText: DestinationSearchText),
                        selectedValueTextCollection: new List<string>())).ToJson();
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public Error.Types Send()
        {
            var error = Create();
            if (error.Has()) return error;
            Host = Parameters.Mail.SmtpHost;
            Port = Parameters.Mail.SmtpPort;
            switch (Host)
            {
                case "smtp.sendgrid.net": SendBySendGrid(); break;
                default: SendBySmtp(); break;
            }
            SentTime = new Time(DateTime.Now);
            error = Update();
            return error.Has()
                ? error
                : Error.Types.None;
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private void SendBySmtp()
        {
            new Smtp(
                Host,
                Port,
                From,
                To,
                Cc,
                Bcc,
                Title.Value,
                Body)
                    .Send();
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        private void SendBySendGrid()
        {
            new SendGridMail(
                Host,
                From,
                To,
                Cc,
                Bcc,
                Title.Value,
                Body)
                    .Send();
        }
    }
}
