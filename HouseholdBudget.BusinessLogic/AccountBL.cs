using HouseholdBudget.Domain.Entities;

namespace HouseholdBudget.BusinessLogic;
public static class AccountBL
{
    public static decimal AdjustAccountBalance(decimal startingBalance, TransactionType transactionType, decimal offsetAmount)
    {
        switch (transactionType)
        {
            case TransactionType.Credit:
                return startingBalance + offsetAmount;
            case TransactionType.Expense:
                return startingBalance - offsetAmount;
            default:
                throw new ArgumentException("Invalid transaction type", nameof(transactionType));
        }
    }
}