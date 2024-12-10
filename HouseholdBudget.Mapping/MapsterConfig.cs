using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO;

using Mapster;

namespace HouseholdBudget.Mapping;

public static class MapsterConfig
{
    public static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        // This automatically maps properties with the same name and compatible types
        config.NewConfig<Expense, ExpenseDTO>();  // No need to define each property

        return config;
    }
}
