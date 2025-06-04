using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Life.DB;
using SQLite;

namespace SJournalier
{
    public class Journalier
    {
        [AutoIncrement][PrimaryKey] public int Id { get; set; }

        public int PlayerId { get; set; }

        public long LastUse { get; set; }

        public static async Task<List<Journalier>> GetJournalier(Expression<Func<Journalier, bool>> expression) => await LifeDB.db.Table<Journalier>().Where(expression).ToListAsync();

        public async Task<bool> Remove()
        {
            await LifeDB.db.DeleteAsync(this);
            return await Task.FromResult(true);
        }

        public async Task<bool> Write()
        {
            if (await LifeDB.db.FindAsync<Journalier>(Id) == null) await LifeDB.db.InsertAsync(this);
            else await LifeDB.db.UpdateAsync(this);
            return true;
        }
    }
}
