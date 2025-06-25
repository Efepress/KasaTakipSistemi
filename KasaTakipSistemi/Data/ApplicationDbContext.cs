using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KasaTakipSistemi.Models;

namespace KasaTakipSistemi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Safe> Safes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CurrentAccount> CurrentAccounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<SalaryPayment> SalaryPayments { get; set; }
        public DbSet<CurrencyExchange> CurrencyExchanges { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<SafeUser> SafeUsers { get; set; } 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); 

          
            builder.Entity<Currency>().HasData(
                new Currency { Id = 1, Name = "Türk Lirası", Symbol = "₺" },
                new Currency { Id = 2, Name = "Dolar", Symbol = "$" },
                new Currency { Id = 3, Name = "İngiliz Sterlini", Symbol = "£" }
              
            );

            builder.Entity<Bank>().HasData(
               new Bank { Id = 1, Name = "T.C. ZİRAAT BANKASI A.Ş." },
               new Bank { Id = 2, Name = "TÜRKİYE İŞ BANKASI A.Ş." },
               new Bank { Id = 3, Name = "TÜRKİYE GARANTİ BANKASI A.Ş." },
               new Bank { Id = 4, Name = "YAPI VE KREDİ BANKASI A.Ş." },
               new Bank { Id = 5, Name = "AKBANK T.A.Ş." },
               new Bank { Id = 6, Name = "TÜRKİYE HALK BANKASI A.Ş." },
               new Bank { Id = 7, Name = "TÜRKİYE VAKIFLAR BANKASI T.A.O." },
               new Bank { Id = 8, Name = "QNB FİNANSBANK A.Ş." },
               new Bank { Id = 9, Name = "DENİZBANK A.Ş." },
               new Bank { Id = 10, Name = "HSBC BANK A.Ş." }
            );

            builder.Entity<Safe>()
        .HasOne(s => s.User) 
        .WithMany(u => u.Safes) 
        .HasForeignKey(s => s.UserId)
        .OnDelete(DeleteBehavior.Restrict); 
                                           

         
            builder.Entity<SafeUser>()
                .HasKey(su => new { su.ApplicationUserId, su.SafeId });

         
            builder.Entity<SafeUser>()
                .HasOne(su => su.ApplicationUser) 
                .WithMany(u => u.AuthorizedSafes) 
                .HasForeignKey(su => su.ApplicationUserId) 
                .OnDelete(DeleteBehavior.Cascade);

           
            builder.Entity<SafeUser>()
                .HasOne(su => su.Safe) 
                .WithMany(s => s.AuthorizedUsers) 
                .HasForeignKey(su => su.SafeId) 
                .OnDelete(DeleteBehavior.Cascade); 

            

           
            builder.Entity<Safe>()
                .HasOne(s => s.User) 
                .WithMany(u => u.Safes) 
                .HasForeignKey(s => s.UserId) 
                .OnDelete(DeleteBehavior.Restrict); 
                                                    

  
            builder.Entity<Transaction>()
                .HasOne(t => t.Safe)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.SafeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Transaction>()
                .HasOne(t => t.User) 
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<Transaction>()
                .HasOne(t => t.Currency)
                .WithMany()
                .HasForeignKey(t => t.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict); 

          
            builder.Entity<CurrentAccount>()
               .HasOne(ca => ca.User) 
               .WithMany()
               .HasForeignKey(ca => ca.UserId)
               .OnDelete(DeleteBehavior.Cascade); 

          
            builder.Entity<Employee>()
              .HasOne(e => e.User) 
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade); 

            builder.Entity<Employee>()
                .HasOne(e => e.SalaryCurrency)
                .WithMany()
                .HasForeignKey(e => e.SalaryCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasOne(e => e.DefaultSafe)
                .WithMany()
                .HasForeignKey(e => e.DefaultSafeId)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.SetNull);

            
            builder.Entity<SalaryPayment>()
               .HasOne(sp => sp.Employee)
               .WithMany(e => e.SalaryPayments)
               .HasForeignKey(sp => sp.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade); 

            builder.Entity<SalaryPayment>()
                .HasOne(sp => sp.Currency)
                .WithMany()
                .HasForeignKey(sp => sp.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalaryPayment>()
                .HasOne(sp => sp.Safe) 
                .WithMany()
                .HasForeignKey(sp => sp.SafeId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<SalaryPayment>()
               .HasOne(sp => sp.User) 
               .WithMany()
               .HasForeignKey(sp => sp.UserId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalaryPayment>()
                .HasOne(sp => sp.Transaction) 
                .WithMany()
                .HasForeignKey(sp => sp.TransactionId)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.SetNull); 

            builder.Entity<CurrencyExchange>()
               .HasOne(ce => ce.SoldCurrency)
               .WithMany()
               .HasForeignKey(ce => ce.SoldCurrencyId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CurrencyExchange>()
                .HasOne(ce => ce.BoughtCurrency)
                .WithMany()
                .HasForeignKey(ce => ce.BoughtCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CurrencyExchange>()
                .HasOne(ce => ce.CurrentAccount) 
                .WithMany()
                .HasForeignKey(ce => ce.CurrentAccountId)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CurrencyExchange>()
                .HasOne(ce => ce.User) 
                .WithMany()
                .HasForeignKey(ce => ce.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CurrencyExchange>()
                .HasOne(ce => ce.MainSafe) 
                .WithMany()
                .HasForeignKey(ce => ce.MainSafeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CurrencyExchange>()
                .HasOne(ce => ce.ExpenseTransaction) 
                .WithMany()
                .HasForeignKey(ce => ce.ExpenseTransactionId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<CurrencyExchange>()
                .HasOne(ce => ce.IncomeTransaction) 
                .WithMany()
                .HasForeignKey(ce => ce.IncomeTransactionId)
                .OnDelete(DeleteBehavior.Restrict); 

          
            builder.Entity<BankAccount>()
                .HasOne(ba => ba.Bank)
                .WithMany(b => b.BankAccounts)
                .HasForeignKey(ba => ba.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BankAccount>()
                .HasOne(ba => ba.Currency)
                .WithMany()
                .HasForeignKey(ba => ba.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BankAccount>()
                .HasOne(ba => ba.User) 
                .WithMany()
                .HasForeignKey(ba => ba.UserId)
                .OnDelete(DeleteBehavior.Cascade); 

    
            builder.Entity<SafeUser>()
                .HasKey(su => new { su.ApplicationUserId, su.SafeId });

           
            builder.Entity<SafeUser>()
                .HasOne(su => su.ApplicationUser)
                .WithMany(u => u.AuthorizedSafes) 
                .HasForeignKey(su => su.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade); 

           
            builder.Entity<SafeUser>()
                .HasOne(su => su.Safe)
                .WithMany(s => s.AuthorizedUsers) 
                .HasForeignKey(su => su.SafeId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}