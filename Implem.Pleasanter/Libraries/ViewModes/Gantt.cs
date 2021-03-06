﻿using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.DataTypes;
using Implem.Pleasanter.Libraries.Responses;
using Implem.Pleasanter.Libraries.Settings;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Implem.Pleasanter.Libraries.ViewModes
{
    public class Gantt : List<GanttElement>
    {
        public Column GroupBy;
        public Column SortBy;

        public Gantt(SiteSettings ss, IEnumerable<DataRow> dataRows, string groupBy, string sortBy)
        {
            GroupBy = ss.GetColumn(groupBy);
            SortBy = ss.GetColumn(sortBy);
            var status = ss.GetColumn("Status");
            var workValue = ss.GetColumn("WorkValue");
            var progressRate = ss.GetColumn("ProgressRate");
            dataRows.ForEach(dataRow =>
                Add(new GanttElement(
                    GroupBy != null
                        ? dataRow["GroupBy"].ToString()
                        : string.Empty,
                    SortBy != null
                        ? dataRow["SortBy"]
                        : string.Empty,
                    dataRow["Id"].ToLong(),
                    new Title(ss, dataRow, "Id").DisplayValue,
                    dataRow["WorkValue"].ToDecimal(),
                    dataRow["StartTime"].ToDateTime(),
                    dataRow["CompletionTime"].ToDateTime(),
                    dataRow["ProgressRate"].ToDecimal(),
                    dataRow["Status"].ToInt(),
                    dataRow["Owner"].ToInt(),
                    dataRow["Updator"].ToInt(),
                    dataRow["CreatedTime"].ToDateTime(),
                    dataRow["UpdatedTime"].ToDateTime(),
                    status,
                    workValue,
                    progressRate,
                    ss.ShowGanttProgressRate.ToBool())));
            AddSummary(ss, dataRows, status, workValue, progressRate);
        }

        private void AddSummary(
            SiteSettings ss,
            IEnumerable<DataRow> dataRows,
            Column status,
            Column workValue,
            Column progressRateColumn)
        {
            var groupCount = 0;
            if (GroupBy != null)
            {
                GroupBy.EditChoices(insertBlank: true).ForEach(choice =>
                {
                    var groupBy = dataRows.Where(o => o["GroupBy"].ToString() == choice.Key);
                    if (groupBy.Any())
                    {
                        groupCount++;
                        AddSummary(
                            ss,
                            choice.Key,
                            choice.Value.Text,
                            status,
                            workValue,
                            progressRateColumn,
                            groupBy);
                    }
                });
            }
            else
            {
                if (dataRows.Any())
                {
                    groupCount++;
                    AddSummary(
                        ss,
                        string.Empty,
                        string.Empty,
                        status,
                        workValue,
                        progressRateColumn,
                        dataRows);
                }
            }
        }

        private void AddSummary(
            SiteSettings ss,
            string groupBy,
            string title,
            Column status,
            Column workValue,
            Column progressRate,
            IEnumerable<DataRow> dataRows)
        {
            var workValueData = dataRows.Sum(o => o["WorkValue"].ToDecimal());
            Add(new GanttElement(
                groupBy,
                string.Empty,
                0,
                Displays.Total() + ": " + title,
                workValueData,
                dataRows.Min(o => o["StartTime"].ToDateTime()),
                dataRows.Max(o => o["CompletionTime"].ToDateTime()),
                workValueData != 0
                    ? dataRows.Sum(o =>
                        o["WorkValue"].ToDecimal() *
                        o["ProgressRate"].ToDecimal()) /
                        workValueData
                    : 0,
                dataRows.Select(o => o["Status"]).AllEqual()
                    ? dataRows.FirstOrDefault()["Status"].ToInt()
                    : 0,
                dataRows.Select(o => o["Owner"]).AllEqual()
                    ? dataRows.FirstOrDefault()["Owner"].ToInt()
                    : 0,
                dataRows.Select(o => o["Updator"]).AllEqual()
                    ? dataRows.FirstOrDefault()["Updator"].ToInt()
                    : 0,
                dataRows.Min(o => o["CreatedTime"].ToDateTime()),
                dataRows.Max(o => o["UpdatedTime"].ToDateTime()),
                status,
                workValue,
                progressRate,
                ss.ShowGanttProgressRate.ToBool(),
                summary: true));
        }

        public string Json()
        {
            var choices = GroupBy?
                .EditChoices(insertBlank: true)
                .Select(o => o.Key)
                .ToList();
            return this
                .OrderBy(o => choices?.IndexOf(o.GroupBy))
                .ThenByDescending(o => o.GroupSummary)
                .Sort(SortBy)
                .ThenBy(o => o.StartTime)
                .ThenBy(o => o.CompletionTime)
                .ThenBy(o => o.Title)
                .ToList()
                .ToJson();
        }
    }
}
