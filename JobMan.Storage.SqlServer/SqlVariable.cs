
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Storage.SqlServer;

internal class SqlVariableType
{
    public SqlDbType DbType { get; set; }
    public int Lenght { get; set; }
    public int Precision { get; set; }

    public bool IsString { get { return this.DbType == SqlDbType.NChar || this.DbType == SqlDbType.NText || this.DbType == SqlDbType.NVarChar || this.DbType == SqlDbType.Text || this.DbType == SqlDbType.VarChar; } }

    public SqlVariableType()
    {

    }

    public SqlVariableType(SqlDbType dbType, int lenght, int precision)
    {
        this.DbType = dbType;
        this.Lenght = lenght;
        this.Precision = precision;
    }

    public SqlVariableType(SqlDbType dbType, int lenght)
    {
        this.DbType = dbType;
        this.Lenght = lenght;
        this.Precision = 0;
    }

    public SqlVariableType(SqlDbType dbType)
    {
        this.DbType = dbType;
        this.Lenght = 0;
        this.Precision = 0;
    }
}

internal class SqlVariable : SqlVariableType
{
    object _value;
    public object Value { get { return _value ?? DBNull.Value; } set { _value = value; } }
    public string ColumnName { get; set; }
    public string ParameterName { get; set; }

    public SqlVariable()
    {

    }

    public SqlVariable(SqlDbType dbType, int lenght, int precision, object value)
        : base(dbType, lenght, precision)
    {
        this.Value = value;
    }

    public SqlVariable(SqlDbType dbType, int lenght, int precision)
       : base(dbType, lenght, precision)
    {

    }

    public SqlVariable(SqlDbType dbType, int lenght, object value)
        : base(dbType, lenght)
    {
        this.Value = value;
    }

    public SqlVariable(SqlDbType dbType, int lenght)
      : base(dbType, lenght)
    {
    }

    public SqlVariable(SqlDbType dbType, object value)
        : base(dbType)
    {
        this.Value = value;
    }

    public SqlVariable(SqlDbType dbType)
        : base(dbType)
    {

    }

    public void Set(SqlVariableType type)
    {
        this.DbType = type.DbType;
        this.Lenght = type.Lenght;
        this.Precision = type.Precision;
    }
}

internal class SqlVariableCollection : Dictionary<string, SqlVariable>
{
    public SqlVariableCollection()
    {

    }

    public SqlVariableCollection(SqlVariable variable)
    {
        this.Add(variable);
    }

    public void AddRange(SqlVariableCollection collection)
    {
        this.AddRange(collection.Values);
    }

    public void AddRange(IEnumerable<SqlVariable> items)
    {
        foreach (SqlVariable item in items)
            this.Add(item);
    }

    public string IncrementParameterName(string parameterName)
    {
        if (parameterName[parameterName.Length - 2] != '_')
            parameterName = parameterName + "_0";
        else
            parameterName = parameterName.Increment();
        return parameterName;
    }

    public void Add(SqlVariable item)
    {
        if (this.ContainsKey(item.ParameterName))
        {
            item.ParameterName = this.IncrementParameterName(item.ParameterName);
            this.Add(item);
        }
        else
            base[item.ParameterName] = item;
    }

    public SqlVariable First()
    {
        foreach (string key in this.Keys)
        {
            return this[key];
        }

        return null;
    }

    public new SqlVariable this[string columnName]
    {
        get
        {
            return this.ContainsKey(columnName) ? base[columnName] : null;

            //SqlVariable x = (from sv in this where sv.ColumnName == columnName select sv).FirstOrDefault();
            //return x;
        }

    }

}
