using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace TMT.DatabaseManager
{
    internal class DbParams
    {
        private readonly string CONDITION = " `{0}` {1} {2} ";
        private readonly string CONDITION_CASE_IGNORED = " UPPER(`{0}`) {1} UPPER({2}) ";
        private readonly string CONDITION_IF_FUNCTION = " {0} {1} {2} ";
        private readonly string CONDITION_IF_FUNCTION_CASE_IGNORED = " UPPER({0}) {1} UPPER({2}) ";
        private readonly string CONDITION_MYSQL = " `{0}` {1} {2} COLLATE utf8_bin";
        private readonly bool isCaseSensitive;
        private readonly CultureInfo myCultureInfo;
        private readonly string PARAM = "@param";
        private readonly Type parameterType;

        internal DbParams(Type parameterType, CultureInfo myCultureInfo, bool isCaseSensitive)
        {
            this.parameterType = parameterType;
            this.myCultureInfo = myCultureInfo;
            this.isCaseSensitive = isCaseSensitive;
        }

        public (string sWhere, IList<DbParameter> paramList) GetWhere(DbSearchCondition[] searchConditionList, bool isMySql)
        {
            string sWhere = string.Empty;
            var paramList = new List<DbParameter>();
            if (searchConditionList == null ||
                searchConditionList.Length <= 0)
            {
                return (sWhere, paramList);
            }
            foreach (var searchCondition in searchConditionList)
            {
                var sValueList = searchCondition.GetSearchValueList();

                if (sValueList == null || sValueList.Count == 0)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sWhere) == false)
                {
                    sWhere += " AND";
                }
                var sParamNameList = this.GetParamList(paramList.Count, sValueList.Count);

                for (int i = 0; i < sValueList.Count; i++)
                {
                    if (i == 0)
                    {
                        sWhere += " (";
                    }
                    else if (i > 0)
                    {
                        sWhere += " OR";
                    }

                    string sValue = sValueList[i].Trim();
                    string operatorSymbol = GetDatabaseOperator(sValue);

                    sValue = sValue.Replace(operatorSymbol, string.Empty).Trim();
                    if (searchCondition.SearchValuesType == typeof(string))
                    {
                        if (isCaseSensitive == false)
                        {
                            sWhere += string.Format((searchCondition.IsFuntion) ? CONDITION_IF_FUNCTION_CASE_IGNORED : CONDITION_CASE_IGNORED,
                                searchCondition.DbColumnName, operatorSymbol, sParamNameList[i]);
                        }
                        else if (isMySql)
                        {
                            sWhere += string.Format((searchCondition.IsFuntion) ? CONDITION_IF_FUNCTION : CONDITION_MYSQL,
                                searchCondition.DbColumnName, operatorSymbol, sParamNameList[i]);
                        }
                        else
                        {
                            sWhere += string.Format((searchCondition.IsFuntion) ? CONDITION_IF_FUNCTION : CONDITION,
                                searchCondition.DbColumnName, operatorSymbol, sParamNameList[i]);
                        }
                    }
                    else
                    {
                        sWhere += string.Format((searchCondition.IsFuntion) ? CONDITION_IF_FUNCTION : CONDITION,
                            searchCondition.DbColumnName, operatorSymbol, sParamNameList[i]);
                    }

                    object oValue = sValue;
                    if (searchCondition.SearchValuesType == typeof(DateTime))
                    {
                        oValue = Convert.ToDateTime(sValue, myCultureInfo.DateTimeFormat);
                    }

                    var parameter = GetParameter(sParamNameList[i], oValue);

                    if (searchCondition.SearchValuesType == typeof(DateTime))
                    {
                        parameter.DbType = DbType.DateTime;
                    }
                    else if (searchCondition.SearchValuesType == typeof(int) ||
                             searchCondition.SearchValuesType == typeof(Int32) ||
                             searchCondition.SearchValuesType == typeof(Int64))
                    {
                        parameter.DbType = DbType.Int64;
                    }

                    paramList.Add(parameter);
                }

                if (string.IsNullOrWhiteSpace(sWhere) == false)
                {
                    sWhere += ")";
                }
            }
            if (paramList.Count > 0)
            {
                sWhere = $"WHERE {sWhere}";
            }
            return (sWhere, paramList);
        }

        private static string GetDatabaseOperator(string sValue)
        {
            string operatorSymbol = "=";
            if (sValue.StartsWith("<>", StringComparison.Ordinal))
            {
                operatorSymbol = "!=";
            }
            else if (sValue.StartsWith("!=", StringComparison.Ordinal))
            {
                operatorSymbol = "!=";
            }
            else if (sValue.StartsWith("<=", StringComparison.Ordinal))
            {
                operatorSymbol = "<=";
            }
            else if (sValue.StartsWith(">=", StringComparison.Ordinal))
            {
                operatorSymbol = ">=";
            }
            else if (sValue.StartsWith("<", StringComparison.Ordinal))
            {
                operatorSymbol = "<";
            }
            else if (sValue.StartsWith(">", StringComparison.Ordinal))
            {
                operatorSymbol = ">";
            }
            else if (sValue.Contains("%"))
            {
                operatorSymbol = "LIKE";
            }
            return operatorSymbol;
        }

        private DbParameter GetParameter(string parameterName, object parameterValue)
        {
            return Activator.CreateInstance(parameterType, parameterName, parameterValue) as DbParameter;
        }

        private IList<string> GetParamList(int startIndex, int count)
        {
            var sParamList = new List<string>();
            for (int i = startIndex; i < startIndex + count; i++)
            {
                sParamList.Add($"{PARAM}{i}");
            }
            return sParamList;
        }
    }
}