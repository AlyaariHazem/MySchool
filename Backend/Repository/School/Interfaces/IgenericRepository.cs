using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Repository
{
     public interface IgenericRepository<T> where T : class
    {
        void Delete(int id);
        T GetById(int id);
        
    }
   
}