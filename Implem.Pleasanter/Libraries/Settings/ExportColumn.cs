﻿using Implem.Libraries.Utilities;
using System;
namespace Implem.Pleasanter.Libraries.Settings
{
    [Serializable]
    public class ExportColumn
    {
        public long SiteId;
        public int Id;
        public string ColumnName;
        public string LabelText;
        public Types? Type;
        public string Format;
        [NonSerialized]
        public string SiteTitle;
        [NonSerialized]
        public Column Column;

        public ExportColumn()
        {
        }

        public ExportColumn(SiteSettings ss, string columnName, int id = 0)
        {
            Id = id;
            SiteId = ss.SiteId;
            SiteTitle = ss.Title;
            ColumnName = columnName;
            Init(ss);
        }

        public enum Types
        {
            Value,
            Text,
            TextMini
        }

        public void Init(SiteSettings ss)
        {
            SiteId = ss.SiteId;
            SiteTitle = ss.Title;
            Column = ss.GetColumn(ColumnName);
        }

        public string GetColumnLabelText()
        {
            return "[" + SiteTitle + "]" + Column?.LabelText;
        }

        public string GetLabelText(bool withSiteTitle = false)
        {
            return (withSiteTitle
                ? "[" + SiteTitle + "]"
                : string.Empty)
                    + (LabelText ?? Column?.LabelText);
        }

        public new string GetType()
        {
            return (Type ?? Types.Text).ToInt().ToString();
        }

        public string GetFormat()
        {
            switch (Column?.TypeName)
            {
                case "datetime": return Format ?? Column?.EditorFormat;
                default: return null;
            }
        }

        public void Update(string labelText, Types type, string format)
        {
            if (labelText != Column?.LabelText)
            {
                LabelText = labelText;
            }
            if (type != Types.Text)
            {
                Type = type;
            }
            if (!format.IsNullOrEmpty() && format != Column?.EditorFormat)
            {
                Format = format;
            }
        }
    }
}