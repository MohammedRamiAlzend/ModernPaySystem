using System;
using System.Collections.Generic;
using System.Text;
namespace ModernPaySystem.Domain.Commons.Auth;

public static class Permission
{
    public static class TransactionSystem
    {
        public const string ViewTransactions = "TransactionSystem.ViewTransactions";
        public const string CreateTransaction = "TransactionSystem.CreateTransaction";
        public const string UpdateTransaction = "TransactionSystem.UpdateTransaction";
        public const string DeleteTransaction = "TransactionSystem.DeleteTransaction";
    }
}
