using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace System.Data
{
    public class ADOHelper: IDisposable
	{
		
		protected string _connString = null;
		protected SqlConnection _conn = null;
		protected SqlTransaction _trans = null;
		protected bool _disposed = false;
        
		
		public static string ConnectionString { get; set; }
		public SqlTransaction Transaction { get { return _trans; } }
        public ADOHelper()
		{
			_connString = ConnectionString;
            if (_connString == null) _connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(); 
			Connect();
		}
		public ADOHelper(string connString)
		{
			_connString = connString;
			Connect();
		}
		protected void Connect()
		{
			_conn = new SqlConnection(_connString);
			_conn.Open();
		}

		public SqlCommand CreateCommand(string qry, CommandType type, params object[] args)
		{
			SqlCommand cmd = new SqlCommand(qry, _conn);
			if (_trans != null)
				cmd.Transaction = _trans;
			cmd.CommandType = type;
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] is string && i < (args.Length - 1))
				{
					SqlParameter parm = new SqlParameter();
					parm.ParameterName = (string)args[i];
				    var temp = args[++i];
                    if (temp != null)
                        parm.Value = temp;
                    else parm.Value = DBNull.Value;
					cmd.Parameters.Add(parm);
				}
				else if (args[i] is SqlParameter)
				{
					cmd.Parameters.Add((SqlParameter)args[i]);
				}
				else throw new ArgumentException("Invalid number or type of arguments supplied");
			}
			return cmd;
		}

		#region Exec Members

 
		public int ExecNonQuery(string qry, params object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
			{
				return cmd.ExecuteNonQuery();
			}
		}

 
		public int ExecNonQueryProc(string proc, params object[] args)
		{
			using (SqlCommand cmd = CreateCommand(proc, CommandType.StoredProcedure, args))
			{
				return cmd.ExecuteNonQuery();
			}
		}
 
		public object ExecScalar(string qry, params object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
			{
				return cmd.ExecuteScalar();
			}
		}

	 
		public object ExecScalarProc(string qry, params object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args))
			{
				return cmd.ExecuteScalar();
			}
		}
 
		public SqlDataReader ExecDataReader(string qry, params object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
			{
				return cmd.ExecuteReader();
			}
		}

	 
		public SqlDataReader ExecDataReaderProc(string qry, params object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args))
			{
				return cmd.ExecuteReader();
			}
		}

		/// <summary>
		/// Executes a query and returns the results as a DataSet
		/// </summary>
		/// <param name="qry">Query text</param>
		/// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
		/// <returns>Results as a DataSet</returns>
		public DataSet ExecDataSet(string qry, params object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
			{
				SqlDataAdapter adapt = new SqlDataAdapter(cmd);
				DataSet ds = new DataSet();
				adapt.Fill(ds);
				return ds;
			}
		}

		/// <summary>
		/// Executes a stored procedure and returns the results as a Data Set
		/// </summary>
		/// <param name="proc">Name of stored proceduret</param>
		/// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
		/// <returns>Results as a DataSet</returns>
		public DataSet ExecDataSetProc(string qry, params object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args))
			{
				SqlDataAdapter adapt = new SqlDataAdapter(cmd);
				DataSet ds = new DataSet();
				adapt.Fill(ds);
				return ds;
			}
		}

		#endregion

		#region Transaction Members

		/// <summary>
		/// Begins a transaction
		/// </summary>
		/// <returns>The new SqlTransaction object</returns>
		public SqlTransaction BeginTransaction()
		{
			Rollback();
			_trans = _conn.BeginTransaction();
			return Transaction;
		}

		/// <summary>
		/// Commits any transaction in effect.
		/// </summary>
		public void Commit()
		{
			if (_trans != null)
			{
				_trans.Commit();
				_trans = null;
			}
		}

		/// <summary>
		/// Rolls back any transaction in effect.
		/// </summary>
		public void Rollback()
		{
			if (_trans != null)
			{
				_trans.Rollback();
				_trans = null;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{ 
				if (disposing)
				{
					if (_conn != null)
					{
						Rollback();
						_conn.Dispose();
						_conn = null;
					}
				}
				_disposed = true;
			}
		}

		#endregion
	}
}
