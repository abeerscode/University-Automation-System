SQL Server integration test plan (requires live test database and .NET 10 SDK):
- Run initial migration on separate test SQL Server database (never production) and seed twice.
- Check all 61 original entity tables, foreign keys, check constraints and indexes.
- Verify RolePermission/user-role composite keys and delete NO ACTION behavior.
- Verify audit interceptor adds an Audit_Entry to the same transaction on changes.
- Verify rollback on constraint failures and no orphan audit rows.
- Verify approval reference uniqueness under concurrent creates (currently a known race risk).
