﻿using Implem.Libraries.Classes;
namespace Implem.Libraries.DataSources.SqlServer
{
    public class SqlParamCollection : ListEx<SqlParam>
    {
        public SqlParamCollection(params SqlParam[] sqlParamCollection)
        {
            AddRange(sqlParamCollection);
        }

        public SqlParamCollection Add(
            string columnBracket,
            string name,
            object value,
            SqlStatement sub,
            string raw,
            bool _using)
        {
            Add(new SqlParam(columnBracket, name, value, sub, raw, _using: _using));
            return this;
        }

        public void Prefix(string prefix)
        {
            ForEach(o => o.VariableName += prefix);
        }
    }
}
