using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
    private readonly DataContext _context;

    public ValuesController(DataContext context){
         _context=context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Value>>> Get()
    {
        var values=await _context.Values.ToListAsync();
        return Ok(values);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Value>> Get(int id)
    {
        var value=await _context.Values.FindAsync(id);
        if(value==null)
        {
           return NotFound();
        }
        return Ok(value);
    }
       
    }
}
