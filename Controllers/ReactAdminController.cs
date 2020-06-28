// using DotNetCoreReactAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetCoreReactAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ReactAdminController<T> : ControllerBase, IReactAdminController<T> where T : class, new()
    {
        protected readonly DbContext _context;
        protected DbSet<T> _table;
        protected PropertyInfo FirstKeyProperty
        {
            get 
            {
                return GetFirstKey();
            }
        }
        protected string FirstKeyName
        {
            get
            {
                return GetFirstKey().Name;
            }
        }
        protected Type FirstKeyType
        {
            get
            {
                return GetFirstKey().PropertyType;
            }
        }

        public ReactAdminController(DbContext context)
        {
            _context = context;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<T>> Delete(string id)
        {
            Console.WriteLine("[Delete]");
            var entity = await _table.FindAsync(CastPropertyValue(this.FirstKeyProperty, id));
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
                if (Regex.IsMatch(filter, @"^{\S+?\s*?:\s*?\[.*?\]\s*?}$"))
                {
                    var filterVal = (JObject)JsonConvert.DeserializeObject<IEnumerable<string>>(filter);
                    // Type := getMany
                    // TODO
                }
                else
                {
                    // Type := getList || getManyReference
                    var filterVal = (JObject)JsonConvert.DeserializeObject(filter);
                    foreach (var f in filterVal)
                    {
                        if (typeof(T).GetProperty(f.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)?.PropertyType == typeof(string))
                        {
                            entityQuery = entityQuery.Where($"{f.Key}.Contains(@0)", f.Value.ToString());
                        }
                        else
                        {
                            entityQuery = entityQuery.Where($"{f.Key} == @0", f.Value.ToString());
                        }
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

        [HttpGet("{id}")]
        public async Task<ActionResult<T>> Get(string id)
        {
            Console.WriteLine("[Get] id");
            var entity = await _table.FindAsync(CastPropertyValue(this.FirstKeyProperty, id));

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
            var id = typeof(T).GetProperty(this.FirstKeyName).GetValue(entity);
            Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Ok(await _table.FindAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, T entity)
        {
            Console.WriteLine("[Put]");
            var entityId = typeof(T).GetProperty(this.FirstKeyName).GetValue(entity);
            if (CastPropertyValue(this.FirstKeyProperty, id) != entityId)
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
                if (!EntityExists(id))
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

        private bool EntityExists(string id)
        {
            return _table.Any(e => typeof(T).GetProperty(this.FirstKeyName).GetValue(e) == CastPropertyValue(this.FirstKeyProperty, id));
        }

        // credit to <https://stackoverflow.com/a/12836049/9920172>
        private IEnumerable<PropertyInfo> GetKeys()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var attribute = Attribute.GetCustomAttribute(property, typeof(KeyAttribute))
                    as KeyAttribute;

                if (attribute != null) // This property has a KeyAttribute
                {
                    yield return property;
                }
            }
        }

        private PropertyInfo GetFirstKey()
        {
            foreach (PropertyInfo property in GetKeys())
            {
                return property;
            }
            return null;
        }

        // credit to <https://stackoverflow.com/a/909666/9920172>
        public object CastPropertyValue(PropertyInfo property, string value)
        {
            if (property == null || String.IsNullOrEmpty(value))
                return null;
            if (property.PropertyType.IsEnum)
            {
                Type enumType = property.PropertyType;
                if (Enum.IsDefined(enumType, value))
                    return Enum.Parse(enumType, value);
            }
            if (property.PropertyType == typeof(bool))
                return value == "1" || value == "true" || value == "on" || value == "checked";
            else if (property.PropertyType == typeof(Uri))
                return new Uri(Convert.ToString(value));
            else
                return Convert.ChangeType(value, property.PropertyType);
        }

    }
}
