﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170826
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170826 11:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rafy.Data.Providers
{
    /// <summary>
    /// 根据提供的字符串类型数据库提供器转换类型，可以获取指定的 DbProviderFactory 和 ISqlProvider 类型的具体实例
    /// </summary>
    internal class DbConnectorFactory
    {
        private static DbProviderFactory _sql, _sqlCe, _oracle,_mySql;
        private static ISqlProvider _sqlConverter, _oracleConverter, _odbcConverter,_mySqlConverter;

        /// <summary>   
        /// 以快速键值对照来获取 DbProviderFactory。
        /// </summary>
        /// <param name="provider"></param>
        /// <returns>返回 DbProviderFactory 类型的具体对象实例</returns>
        public static DbProviderFactory GetFactory(string provider)
        {
            switch (provider)
            {
                case DbSetting.Provider_SqlClient:
                    if (_sql == null) { _sql = System.Data.SqlClient.SqlClientFactory.Instance; }
                    return _sql;
                case DbSetting.Provider_SqlCe:
                    if (_sqlCe == null) { _sqlCe = System.Data.SqlClient.SqlClientFactory.Instance; }
                    return _sqlCe;
                case DbSetting.Provider_MySql:
                    if (_mySql == null) { _mySql = MySql.Data.MySqlClient.MySqlClientFactory.Instance; }
                    return _mySql;
                default:
                    if (DbSetting.IsOracleProvider(provider))
                    {
                        if (_oracle == null) { _oracle = Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance; }
                        return _oracle;
                    }
                    return System.Data.SqlClient.SqlClientFactory.Instance;
                //throw new NotSupportedException("This type of database is not supportted now:" + provider);
            }
        }

        /// <summary>
        /// 创建指定的链接字符串的转换器
        /// </summary>
        /// <param name="provider">指定的数据库的提供程序</param>
        /// <returns>返回针对指定数据库的 ISqlProvider 类型的具体对象实例</returns>
        public static ISqlProvider Create(string provider)
        {
            //ISqlConverter Factory
            switch (provider)
            {
                case DbSetting.Provider_Odbc:
                    if (_odbcConverter == null) _odbcConverter = new ODBCProvider();
                    return _odbcConverter;

                case DbSetting.Provider_SqlClient:
                case DbSetting.Provider_SqlCe:
                    if (_sqlConverter == null) _sqlConverter = new SqlServerProvider();
                    return _sqlConverter;
                // PatrickLiu 增加的有关 MySql 的代码
                case DbSetting.Provider_MySql:
                    if (_mySqlConverter == null) _mySqlConverter = new MySqlServerProvider();
                    return _mySqlConverter;

                default:
                    if (DbSetting.IsOracleProvider(provider))
                    {
                        if (_oracleConverter == null) _oracleConverter = new OracleProvider();
                        return _oracleConverter;
                    }

                    if (_sqlConverter == null) _sqlConverter = new SqlServerProvider();
                    return _sqlConverter;
            }
        }

        /// <summary>
        /// 在 FormatSQL 中的参数格式定义。
        /// </summary>
        internal static readonly Regex ReParameterName = new Regex(@"{(?<number>\d+)}", RegexOptions.Compiled);
    }
}