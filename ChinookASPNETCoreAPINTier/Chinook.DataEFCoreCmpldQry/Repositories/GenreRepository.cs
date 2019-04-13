﻿using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Chinook.Domain.Repositories;
using Chinook.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.DataEFCoreCmpldQry.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly ChinookContext _context;
        
        
        
        private IMemoryCache _cache;

        public GenreRepository(ChinookContext context, IMemoryCache memoryCache)
        {
            _context = context;
            
            _cache = memoryCache;
        }

        private async Task<bool> GenreExists(int id, CancellationToken ct = default) => await GetByIdAsync(id, ct) != null;

        public void Dispose() => _context.Dispose();

        public async Task<List<Genre>> GetAllAsync(CancellationToken ct = default) => await _context.GetAllGenresAsync();

        public async Task<Genre> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var cachedGenre = _cache.Get<Genre>(id);

            if (cachedGenre != null)
            {
                return cachedGenre;
            }

            var dbGenre = await _context.GetGenreAsync(id);

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800));
            _cache.Set(dbGenre.FirstOrDefault().GenreId, dbGenre, cacheEntryOptions);

            return dbGenre.FirstOrDefault();
        }

        public async Task<Genre> AddAsync(Genre newGenre, CancellationToken ct = default)
        {
            _context.Genre.Add(newGenre);
            await _context.SaveChangesAsync(ct);
            return newGenre;
        }

        public async Task<bool> UpdateAsync(Genre genre, CancellationToken ct = default)
        {
            if (!await GenreExists(genre.GenreId, ct))
                return false;
            _context.Genre.Update(genre);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            if (!await GenreExists(id, ct))
                return false;
            var toRemove = _context.Genre.Find(id);
            _context.Genre.Remove(toRemove);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}