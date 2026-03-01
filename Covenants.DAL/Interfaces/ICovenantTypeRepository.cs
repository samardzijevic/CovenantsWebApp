using System.Collections.Generic;
using Covenants.Models;

namespace Covenants.DAL.Interfaces
{
    public interface ICovenantTypeRepository
    {
        IEnumerable<CovenantType> GetAll();
        IEnumerable<CovenantType> GetAllActive();
        CovenantType GetById(int id);
        int Insert(CovenantType type);
        void Update(CovenantType type);
        void SetActive(int id, bool isActive);
    }
}
