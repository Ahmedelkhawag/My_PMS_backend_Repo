using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PMS.Infrastructure.Context;
using PMS.Domain.Entities.BackOffice;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer("Server=db41123.public.databaseasp.net; Database=db41123; User Id=db41123; Password=Ahmed@123852; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;");

using (var db = new ApplicationDbContext(optionsBuilder.Options))
{
    Console.WriteLine("--- Checking Stage 3 Accounts ---");
    var ids = new[] { 214, 2141, 514, 5141 };
    var existing = db.Accounts.Where(a => ids.Contains(a.Id)).Select(a => a.Id).ToList();
    
    Console.WriteLine($"Found {existing.Count} matching accounts in DB: {string.Join(", ", existing)}");
    
    var missing = ids.Except(existing).ToList();
    if(missing.Count == 0) {
        Console.WriteLine("All accounts are present.");
        return;
    }
    
    Console.WriteLine("Inserting missing accounts: " + string.Join(", ", missing));
    
    using var tx = db.Database.BeginTransaction();
    try
    {
        db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Accounts ON;");
        var now = DateTime.UtcNow;

        if (missing.Contains(214))
            db.Accounts.Add(new Account { Id = 214, Code = "214", NameAr = "?????? ?????? ?????", NameEn = "Commission-Related Payables", Type = PMS.Domain.Enums.BackOffice.AccountType.Liability, ParentAccountId = 21, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" });

        if (missing.Contains(2141))
            db.Accounts.Add(new Account { Id = 2141, Code = "2141", NameAr = "?????? ?????? ?????? ???????", NameEn = "Commissions Payable", Type = PMS.Domain.Enums.BackOffice.AccountType.Liability, ParentAccountId = 214, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" });

        if (missing.Contains(514))
            db.Accounts.Add(new Account { Id = 514, Code = "514", NameAr = "?????? ????? ???????", NameEn = "TA Commission Expenses", Type = PMS.Domain.Enums.BackOffice.AccountType.Expense, ParentAccountId = 51, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" });

        if (missing.Contains(5141))
            db.Accounts.Add(new Account { Id = 5141, Code = "5141", NameAr = "????? ????? ???? ???????", NameEn = "TA Commission Expense", Type = PMS.Domain.Enums.BackOffice.AccountType.Expense, ParentAccountId = 514, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" });

        db.SaveChanges();
        db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Accounts OFF;");
        tx.Commit();
        
        Console.WriteLine("SUCCESS: Missing accounts inserted.");
    }
    catch (Exception ex)
    {
        tx.Rollback();
        Console.WriteLine("ERROR: " + ex.Message);
        if(ex.InnerException != null) Console.WriteLine("INNER: " + ex.InnerException.Message);
    }
}
