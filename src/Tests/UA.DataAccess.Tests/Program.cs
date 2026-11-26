using Microsoft.EntityFrameworkCore;
using UA.DataAccess.Data;
using UA.Entities;
static void Check(bool ok, string name)
{
    if (!ok) throw new Exception("FAILED: " + name);
    Console.WriteLine("PASS " + name);
}
var dbSetCount = typeof(UniversityDbContext).GetProperties()
    .Count(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
Check(dbSetCount == 61, $"DbSet count remains 61 (found {dbSetCount})");
var options = new DbContextOptionsBuilder<UniversityDbContext>()
    .UseSqlServer("Server=localhost;Database=UAS_schema_check;Trusted_Connection=True;TrustServerCertificate=True;")
    .Options;
using var db = new UniversityDbContext(options);
Check(db.Model.FindEntityType(typeof(UserAccount))?.GetTableName() == "User_Account", "User table mapped");
Check(db.Model.FindEntityType(typeof(AuditEntry))?.GetTableName() == "Audit_Entry", "Audit table mapped");
Check(db.Model.FindEntityType(typeof(ApprovalRecord))?.GetTableName() == "Approval_Record", "Approval table mapped");
Console.WriteLine("Phase-1 mapping checks passed (no network or DB connection required).");
Check(db.Model.FindEntityType(typeof(FeeType))?.GetTableName()=="Fee_Type","P5: Fee_Type mapping");
Check(db.Model.FindEntityType(typeof(Due))?.GetTableName()=="Due","P5: Due mapping");
Check(db.Model.FindEntityType(typeof(Payment))?.GetTableName()=="Payment","P5: Payment mapping");
Check(db.Model.FindEntityType(typeof(PaymentAllocation))?.FindPrimaryKey()?.Properties.Count==2,"P5: allocation composite key");
Check(db.Model.FindEntityType(typeof(Receipt))?.GetIndexes().Any(i=>i.IsUnique && i.Properties.Any(p=>p.Name==nameof(Receipt.ReceiptNo)))==true,"P5: unique receipt number");
