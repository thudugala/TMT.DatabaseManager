using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace TMT.DatabaseManager
{
    public class DbManager
    {
        public DatabaseConnectionDetail connectionDetail;

        public DbManager(DatabaseConnectionDetail ConnectionDetail)
        {
            connectionDetail = ConnectionDetail;
        }

        public Task CheckDbConnection()
        {
            return Task.Run(() =>
            {
                DbConnection connection = null;
                try
                {
                    connection = DbUtility.GetDbConnection(this.connectionDetail);

                    connection.Open();
                }
                catch (Exception ex)
                {
                    throw DbUtility.GetDbException(ex);
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            });
        }

        public Task<int[]> Save(DbSaveEventArgs argument)
        {
            return Task.Run(() =>
            {
                DbConnection connection = null;
                DbTransaction transaction = null;
                int[] updatedRowCount;
                try
                {
                    if (argument.ChangedTableSet == null || argument.ChangedTableSet.Tables.Count <= 0)
                    {
                        throw new Exception(Properties.Resources.ERROR_ChangeTableEmpty);
                    }

                    updatedRowCount = new int[argument.ChangedTableSet.Tables.Count];

                    string select = string.Empty;

                    connection = DbUtility.GetDbConnection(this.connectionDetail);
                    connection.Open();

                    transaction = connection.BeginTransaction();

                    for (int i = 0; i < argument.ChangedTableSet.Tables.Count; i++)
                    {
                        var columnNameList = argument.ChangedTableSet.Tables[i].Columns.Cast<DataColumn>().Select(c => c.ColumnName);

                        select = string.Format(@" SELECT `{0}`
                                              FROM {1} ", string.Join("`, `", columnNameList),
                                                              argument.ChangedTableSet.Tables[i].TableName);

                        var adapter = DbUtility.GetDataAdapter(this.connectionDetail.AdapterType, select, connection);

                        var myCB = DbUtility.GetCommandBuilder(this.connectionDetail.CommandBuilderType, adapter);
                        adapter.SelectCommand.Transaction = transaction;

                        adapter.InsertCommand = myCB.GetInsertCommand();
                        adapter.UpdateCommand = myCB.GetUpdateCommand();
                        adapter.DeleteCommand = myCB.GetDeleteCommand();

                        adapter.ContinueUpdateOnError = argument.ContinueUpdateOnError;

                        updatedRowCount[i] = adapter.Update(argument.ChangedTableSet.Tables[i]);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                    throw DbUtility.GetDbException(ex);
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
                return updatedRowCount;
            });
        }

        public Task<DataTable> Select(DbLoadEventArgs argument)
        {
            return Task.Run(() =>
            {
                DbConnection connection = null;
                try
                {
                    var tmtDbParams = new DbParams(this.connectionDetail.ParameterType, this.connectionDetail.Culture, argument.IsCaseSensitive);

                    if (string.IsNullOrWhiteSpace(argument.TableName))
                    {
                        throw new Exception(Properties.Resources.ERROR_TableNameEmpty);
                    }

                    string selectColums;
                    if (argument.ColumnDbNameList?.Length > 0)
                    {
                        if (argument.UseBackticks)
                        {
                            selectColums = $"`{string.Join("`, `", argument.ColumnDbNameList)}`";
                        }
                        else
                        {
                            selectColums = string.Join(", ", argument.ColumnDbNameList);
                        }
                    }
                    else
                    {
                        selectColums = "*";
                    }

                    var select = $" SELECT {selectColums} FROM {argument.TableName} ";

                    var (sWhere, paramArray) = tmtDbParams.GetWhere(argument.SearchConditionList, this.connectionDetail.IsMysql);
                    if (paramArray.Count > 0)
                    {
                        if (string.IsNullOrWhiteSpace(argument.DefaultWhereStatement) == false)
                        {
                            select += $"{sWhere} AND {argument.DefaultWhereStatement}";
                        }
                        else
                        {
                            select += sWhere;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(argument.DefaultWhereStatement) == false)
                        {
                            select += $"WHERE {argument.DefaultWhereStatement}";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(argument.DefaultOrderByStatement) == false)
                    {
                        select += $" ORDER BY {argument.DefaultOrderByStatement} ";
                    }

                    if (argument.LimitOffset.HasValue)
                    {
                        select += $" LIMIT 100 OFFSET {argument.LimitOffset} ";
                    }
                    else
                    {
                        select += " LIMIT 100 ";
                    }
                    connection = DbUtility.GetDbConnection(this.connectionDetail);
                    connection.Open();
                    var adapter = DbUtility.GetDataAdapter(this.connectionDetail.AdapterType, select, connection);
                    if (paramArray.Count > 0)
                    {
                        adapter.SelectCommand.Parameters.AddRange(paramArray.ToArray());
                    }

                    var table = new DataTable(argument.TableName);
                    if (argument.LoadSchema)
                    {
                        adapter.FillSchema(table, SchemaType.Source);
                    }
                    adapter.Fill(table);

                    return table;
                }
                catch (Exception ex)
                {
                    throw DbUtility.GetDbException(ex);
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            });
        }
    }
}