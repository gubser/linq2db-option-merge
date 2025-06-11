# Error when using Merge() with None values.

Minimal reproducible example to show issue with Merge() and my F# option mapping.

Run the tests with

```
dotnet test
```

Output on my machine:

```
Restore complete (0.5s)
  linq2db-option-merge succeeded (2.3s) → bin/Debug/net9.0/linq2db-option-merge.dll
NUnit Adapter 4.6.0.0: Test execution started
Running all tests in /home/elio/Projects/linq2db-option-merge/bin/Debug/net9.0/linq2db-option-merge.dll
   NUnit3TestExecutor discovered 4 of 4 NUnit test cases using Current Discovery mode, Non-Explicit run
NUnit Adapter 4.6.0.0: Test execution complete
  linq2db-option-merge test failed with 1 error(s) (5.8s)
    /home/elio/Projects/linq2db-option-merge/TestCases.fs(121): error TESTERROR: 
      Can merge none (72ms): Error Message: Npgsql.PostgresException : 42804: column "value" is of type inte
      ger but expression is of type text
      
      POSITION: 203
      Data:
        Severity: ERROR
        InvariantSeverity: ERROR
        SqlState: 42804
        MessageText: column "value" is of type integer but expression is of type text
        Hint: You will need to rewrite or cast the expression.
        Position: 203
        File: parse_target.c
        Line: 587
        Routine: transformAssignedExpr
      Stack Trace:
         at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoading
      Mode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Th
      reading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancell
      ationToken)
         at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancell
      ationToken)
         at Npgsql.NpgsqlDataReader.NextResult()
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken ca
      ncellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken ca
      ncellationToken)
         at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteNonQuery()
         at LinqToDB.Data.DataConnection.ExecuteNonQuery(DbCommand command)
         at LinqToDB.Data.DataConnection.ExecuteNonQuery()
         at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryImpl(DataConnection dataConnection, Exec
      utionPreparedQuery executionQuery)
         at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQuery()
         at LinqToDB.Linq.QueryRunner.NonQueryQuery(Query query, IDataContext dataContext, IQueryExpressions
       expressions, Object[] parameters, Object[] preambles)
         at LinqToDB.Linq.QueryRunner.<>c__DisplayClass28_0.<SetNonQueryQuery>b__0(IDataContext db, IQueryEx
      pressions expr, Object[] ps, Object[] preambles)
         at LinqToDB.Linq.ExpressionQuery`1.System.Linq.IQueryProvider.Execute[TResult](Expression expressio
      n)
         at LinqToDB.LinqExtensions.Merge[TTarget,TSource](IMergeable`2 merge)
         at OptionMergeTest.Can merge none() in /home/elio/Projects/linq2db-option-merge/TestCases.fs:line 1
      21
         at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean 
      isConstructor)
         at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)

Test summary: total: 4, failed: 1, succeeded: 3, skipped: 0, duration: 5.8s
Build failed with 1 error(s) in 9.1s
```

