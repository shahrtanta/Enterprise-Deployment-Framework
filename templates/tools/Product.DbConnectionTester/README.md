# Product.DbConnectionTester

Silent-capable SQL Server/LocalDB connection tester for installers and support tools.

```powershell
Product.DbConnectionTester.exe --request-ini request.ini --result-json result.json --silent
```

Exit codes: `0` success, `1` invalid invocation, `2` connection failure, `3` unexpected failure.
