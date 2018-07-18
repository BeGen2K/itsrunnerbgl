using System;
using System.Collections.Generic;
using System.Text;

namespace ItsRunnerBgl.Models.Repositories
{
    public interface IRepositoryBase<TKey, TVal>
    {
        IEnumerable<TVal> Get();
        TVal Get(TKey id);
        void Update(TVal value);
        int Insert(TVal value);
        void Delete(TKey id);
    }
}
