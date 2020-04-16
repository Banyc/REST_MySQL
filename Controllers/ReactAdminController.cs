﻿// using DotNetCoreReactAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace DotNetCoreReactAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ReactAdminController<T> : ControllerBase, IReactAdminController<T> where T : class, new()
    {
        protected readonly DbContext _context;
        protected DbSet<T> _table;

        public ReactAdminController(DbContext context)
        {
            _context = context;
        }

        [HttpDelete("{uid}")]
        public async Task<ActionResult<T>> Delete(long uid)
        {
            Console.WriteLine("[Delete]");
            var entity = await _table.FindAsync(uid);
            if (entity == null)
            {
                return NotFound();
            }

            _table.Remove(entity);
            await _context.SaveChangesAsync();

            Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Ok(entity);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<T>>> Get(string filter = "", string range = "", string sort = "")
        {
            Console.WriteLine("[Get]");
            var entityQuery = _table.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                var filterVal = (JObject)JsonConvert.DeserializeObject(filter);
                var t = new T();
                foreach (var f in filterVal)
                {
                    if (t.GetType().GetProperty(f.Key).PropertyType == typeof(string))
                    {
                        entityQuery = entityQuery.Where($"{f.Key}.Contains(@0)", f.Value.ToString());
                    }
                    else
                    {
                        entityQuery = entityQuery.Where($"{f.Key} == @0", f.Value.ToString());
                    }
                }
            }
            var count = entityQuery.Count();

            if (!string.IsNullOrEmpty(sort))
            {
                var sortVal = JsonConvert.DeserializeObject<List<string>>(sort);
                var condition = sortVal.First();
                var order = sortVal.Last() == "ASC" ? "" : "descending";
                entityQuery = entityQuery.OrderBy($"{condition} {order}");
            }

            var from = 0;
            var to = 0;
            if (!string.IsNullOrEmpty(range))
            {
                var rangeVal = JsonConvert.DeserializeObject<List<int>>(range);
                from = rangeVal.First();
                to = rangeVal.Last();
                entityQuery = entityQuery.Skip(from).Take(to - from + 1);
            }

            Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"{typeof(T).Name.ToLower()} {from}-{to}/{count}");
            return await entityQuery.ToListAsync();
        }

        [HttpGet("{uid}")]
        public async Task<ActionResult<T>> Get(long uid)
        {
            Console.WriteLine("[Get] uid");
            var entity = await _table.FindAsync(uid);

            if (entity == null)
            {
                return NotFound();
            }

            Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return entity;
        }

        [HttpPost]
        public async Task<ActionResult<T>> Post(T entity)
        {
            Console.WriteLine("[Post]");
            _table.Add(entity);
            await _context.SaveChangesAsync();
            var uid = (int)typeof(T).GetProperty("Id").GetValue(entity);
            Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Ok(await _table.FindAsync(uid));
        }

        [HttpPut("{uid}")]
        public async Task<IActionResult> Put(long uid, T entity)
        {
            Console.WriteLine("[Put]");
            var entityId = (int)typeof(T).GetProperty("Id").GetValue(entity);
            if (uid != entityId)
            {
                return BadRequest();
            }

            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists(uid))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Ok(await _table.FindAsync(entityId));
        }

        private bool EntityExists(long uid)
        {
            return _table.Any(e => (int)typeof(T).GetProperty("Id").GetValue(e) == uid);
        }

    }
}