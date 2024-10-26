using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoenixApi.UnitofWork;

namespace PhoenixApi.Repositories
{
    public class RepositoryBase<T> : ControllerBase, IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected DbSet<T> dbSet;
        private readonly IUnitOfWork _unitOfWork;
        public RepositoryBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            dbSet = _unitOfWork.Context.Set<T>();
        }

        //Get Request
        public async Task<ActionResult<IEnumerable<T>>> Get()
        {
            var data = await dbSet.ToListAsync();
            return Ok(data);
        }

        //Create Request
        public async Task<ActionResult<T>> Create(T entity)
        {
            dbSet.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        //Update Request
        public async Task<IActionResult> Update(int id, T entity)
        {
            //if (id != entity.Id)
            //{
            //    return BadRequest();
            //}

            var existingOrder = await dbSet.FindAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            _unitOfWork.Context.Entry(existingOrder).CurrentValues.SetValues(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        //Delete Request
        public async Task<IActionResult> Delete(int id)
        {
            var data = await dbSet.FindAsync(id);
            if (data == null)
            {
                return NotFound();
            }

            dbSet.Remove(data);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
    }
}
