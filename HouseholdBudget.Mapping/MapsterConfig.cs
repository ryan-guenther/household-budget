using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO;
using HouseholdBudget.DTO.Account;
using HouseholdBudget.DTO.Transaction;

using Mapster;

namespace HouseholdBudget.Mapping;

public static class MapsterConfig
{
    public static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        // Outgoing Transaciton Mappings
        config.NewConfig<Transaction, TransactionListResponseDTO>();
        config.NewConfig<Transaction, TransactionDetailResponseDTO>();

        // Incoming Transaction Mappings
        config.NewConfig<TransactionCreateRequestDTO, Transaction>();
        config.NewConfig<TransactionUpdateRequestDTO, Transaction>();

        // Outgoing Account Mappings
        config.NewConfig<Account, AccountListResponseDTO>();
        config.NewConfig<Account, AccountDetailResponseDTO>();

        // Incoming Account Mappings
        config.NewConfig<AccountCreateRequestDTO, Account>();
        config.NewConfig<AccountUpdateRequestDTO, Account>();

        return config;
    }
}
