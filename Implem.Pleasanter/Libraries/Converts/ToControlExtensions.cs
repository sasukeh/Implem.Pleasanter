﻿using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.Server;
using Implem.Pleasanter.Libraries.Settings;
using System;
namespace Implem.Pleasanter.Libraries.Converts
{
    public static class ToControlExtensions
    {
        public static string ToControl(this Enum self, SiteSettings ss, Column column)
        {
            switch (column.TypeName)
            {
                case "int": return self.ToInt().ToString();
                case "bigint": return self.ToLong().ToString();
                default: return self.ToInt().ToString();
            }
        }

        public static string ToControl(this bool self, SiteSettings ss, Column column)
        {
            return self.ToString();
        }

        public static string ToControl(this DateTime self, SiteSettings ss, Column column)
        {
            return column.DisplayControl(self.ToLocal());
        }

        public static string ToControl(this string self, SiteSettings ss, Column column)
        {
            return self;
        }

        public static string ToControl(this int self, SiteSettings ss, Column column)
        {
            return self.ToString(column.StringFormat);
        }

        public static string ToControl(this long self, SiteSettings ss, Column column)
        {
            return self.ToString(column.StringFormat);
        }

        public static string ToControl(this decimal self, SiteSettings ss, Column column)
        {
            return column.ControlType == "Spinner"
                ? column.Display(self, format: false)
                : column.Display(ss, self, format: column.Format == "C");
        }
    }
}