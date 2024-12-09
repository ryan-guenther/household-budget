using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.Domain.Interfaces
{
    public interface ITrackable
    {
        string CreatedBy { get; set; }  // User or system that created the entity
        DateTime CreatedAt { get; set; }  // Timestamp of creation

        string? ModifiedBy { get; set; }  // User or system that last modified the entity
        DateTime? ModifiedAt { get; set; }  // Timestamp of the last modification
    }
}
