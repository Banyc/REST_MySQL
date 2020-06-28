using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Credit to <https://github.com/tYoshiyuki/dotnet-core-react-admin/blob/master/DotNetCoreReactAdmin/Controllers/IReactAdminController.cs>

namespace DotNetCoreReactAdmin.Controllers
{
    public interface IReactAdminController<T>
    {
        Task<ActionResult<IEnumerable<T>>> Get(string filter = "", string range = "", string sort = "");
        Task<ActionResult<T>> Get(string id);
        Task<IActionResult> Put(string id, T entity);
        Task<ActionResult<T>> Post(T entity);
        Task<ActionResult<T>> Delete(string id);
    }
}
